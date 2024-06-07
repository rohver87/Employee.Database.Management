using System.Net;
using System.Text.Json;
using Employee.Database.Management.Database;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Employee.Database.Management.Service.Tests
{
    public class PublicHolidayServiceTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly Mock<IEmployeeDatabase> _databaseMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly PublicHolidayService _publicHolidayService;

        public PublicHolidayServiceTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _cacheMock = new Mock<IDistributedCache>();
            _databaseMock = new Mock<IEmployeeDatabase>();
            _configurationMock = new Mock<IConfiguration>();

            var configurationSectionMock = new Mock<IConfigurationSection>();

            configurationSectionMock
               .Setup(x => x.Value)
               .Returns("HolidayApiClient");

            _configurationMock
               .Setup(x => x.GetSection("HolidayApiClient"))
               .Returns(configurationSectionMock.Object);

            _publicHolidayService = new PublicHolidayService(
                _httpClientFactoryMock.Object,
                _configurationMock.Object,
                _cacheMock.Object,
                _databaseMock.Object);
        }

        [Fact]
        public async Task GetPublicHolidayByCountryCodeAndYear_ReturnsHolidaysFromCache_WhenCacheIsNotEmpty()
        {
            // Arrange
            var countryCode = "US";
            var year = 2024;
            var cachedKey = $"HolidayList-{countryCode}-{year}";
            var holidays = new List<PublicHoliday>
            {
                new PublicHoliday { Date = DateTime.Today, Name = "New Year's Day" },
                new PublicHoliday { Date = DateTime.Today.AddDays(1), Name = "Holiday 2" }
            };
            var cachedData = JsonSerializer.SerializeToUtf8Bytes(holidays);

            _cacheMock.Setup(c => c.GetAsync(cachedKey, default))
                .ReturnsAsync(cachedData);

            // Act
            var result = await _publicHolidayService.GetPublicHolidayByCountryCodeAndYear(countryCode, year);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            Assert.Equal("New Year's Day", result[0].Name);
            Assert.Equal("Holiday 2", result[1].Name);
        }

        [Fact]
        public async Task GetPublicHolidayByCountryCodeAndYear_ReturnsHolidaysFromApi_WhenCacheIsEmpty()
        {
            // Arrange
            var countryCode = "US";
            var year = 2024;
            var cachedKey = $"HolidayList-{countryCode}-{year}";
            var holidays = new List<PublicHoliday>
            {
                new PublicHoliday { Date = DateTime.Today, Name = "New Year's Day" },
                new PublicHoliday { Date = DateTime.Today.AddDays(1), Name = "Holiday 2" }
            };

            _cacheMock.Setup(c => c.GetAsync(cachedKey, default))
                .ReturnsAsync((byte[]?)null);

            var messageHandler = new MockHttpMessageHandler(JsonSerializer.Serialize(holidays), HttpStatusCode.OK);
            var httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("https://api.example.com") };

            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var result = await _publicHolidayService.GetPublicHolidayByCountryCodeAndYear(countryCode, year);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            Assert.Equal("New Year's Day", result[0].Name);
            Assert.Equal("Holiday 2", result[1].Name);
        }

        [Fact]
        public async Task GetPublicHolidayForCurrent7DaysByEmployee_ReturnsUpcomingHolidays()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var employee = new Employee { CountryCode = "US" };
            var holidays = new List<PublicHoliday>
            {
                new PublicHoliday { Date = DateTime.Today.AddDays(1), Name = "Holiday 1" },
                new PublicHoliday { Date = DateTime.Today.AddDays(8), Name = "Holiday 2" }
            };

            _databaseMock.Setup(db => db.GetEmployee(employeeId)).ReturnsAsync(employee);
            var messageHandler = new MockHttpMessageHandler(JsonSerializer.Serialize(holidays), HttpStatusCode.OK);
            var httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("https://api.example.com") };

            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var result = await _publicHolidayService.GetPublicHolidayForCurrent7DaysByEmployee(employeeId);

            // Assert
            result.Should().NotBeNull();
            Assert.Single(result);
            Assert.Equal("Holiday 1", result[0].Name);
        }

        [Fact]
        public async Task TriggerEmailAlert_ReturnsUpcomingHolidays()
        {
            // Arrange
            var countryCode = "US";
            var holidays = new List<PublicHoliday>
            {
                new PublicHoliday { Date = DateTime.Today.AddDays(1), Name = "Holiday 1" },
                new PublicHoliday { Date = DateTime.Today.AddDays(8), Name = "Holiday 2" }
            };

            var messageHandler = new MockHttpMessageHandler(JsonSerializer.Serialize(holidays), HttpStatusCode.OK);
            var httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("https://api.example.com") };

            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var result = await _publicHolidayService.TriggerEmailAlert(countryCode);

            // Assert
            result.Should().NotBeNull();
            Assert.Single(result);
            Assert.Equal("Holiday 1", result[0].Name);
        }

        // Custom mock HttpMessageHandler for testing purposes
        private class MockHttpMessageHandler : HttpMessageHandler
        {
            private readonly string _response;
            private readonly HttpStatusCode _statusCode;

            public MockHttpMessageHandler(string response, HttpStatusCode statusCode)
            {
                _response = response;
                _statusCode = statusCode;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                return await Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = _statusCode,
                    Content = new StringContent(_response),
                });
            }
        }
    }
}
