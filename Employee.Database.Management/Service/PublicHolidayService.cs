using Employee.Database.Management.Database;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace Employee.Database.Management.Service
{
    public class PublicHolidayService : IPublicHolidayService
    {
        private readonly IHttpClientFactory _httpClientFactory = null!;
        private readonly string _holidayApiClient = null!;
        private readonly IDistributedCache _cache;
        private readonly IEmployeeDatabase _database;

        public PublicHolidayService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IDistributedCache cache, IEmployeeDatabase database)
        {
            _httpClientFactory = httpClientFactory;
            _holidayApiClient = configuration.GetValue<string>("HolidayApiClient");
            _cache = cache;
            _database = database;
        }

        public async Task<List<PublicHoliday>> GetPublicHolidayByCountryCodeAndYear(string countryCode, int year)
        {
            var cachedKey = $"HolidayList-{countryCode}-{year}";
            byte[]? cachedData = await GetCachedHolidayList(cachedKey);

            if (cachedData == null)
            {
                //Get from 3rd party API and cache the data
                var holidays = await GetPublicHolidaysAsync(countryCode, year);
                await SaveHolidayListInCache(cachedKey, holidays);
                return holidays;
            }

            return await JsonSerializer.DeserializeAsync<List<PublicHoliday>>(new MemoryStream(cachedData));
        }

        private async Task SaveHolidayListInCache(string cachedKey, List<PublicHoliday> holidays)
        {
            var data = JsonSerializer.Serialize(holidays);
            await _cache.SetAsync(cachedKey, System.Text.Encoding.UTF8.GetBytes(data));
        }

        private async Task<byte[]?> GetCachedHolidayList(string cachedKey)
        {
            return await _cache.GetAsync(cachedKey);
        }

        private async Task<List<PublicHoliday>> GetPublicHolidaysAsync(string countryCode, int year)
        {
            using HttpClient client = _httpClientFactory.CreateClient(_holidayApiClient ?? "");

            var publicHolidays = await client.GetFromJsonAsync<List<PublicHoliday>>(
               $"api/v3/publicholidays/{year}/{countryCode}",
               new JsonSerializerOptions(JsonSerializerDefaults.Web));

            return publicHolidays;
        }

        public async Task<List<PublicHoliday>> GetPublicHolidayForCurrent7DaysByEmployee(Guid employeeId)
        {
            var employee = await _database.GetEmployee(employeeId);
            var holidaysForEmployeeCountry = await GetPublicHolidaysAsync(employee.CountryCode, DateTime.Now.Year);
            List<PublicHoliday> upcomingHolidays = GetHolidaysIn7DaysRange(holidaysForEmployeeCountry);
            return upcomingHolidays;
        }

        public async Task<List<PublicHoliday>> TriggerEmailAlert(string countryCode)
        {
            var holidaysForEmployeeCountry = await GetPublicHolidaysAsync(countryCode, DateTime.Now.Year);
            List<PublicHoliday> upcomingHolidays = GetHolidaysIn7DaysRange(holidaysForEmployeeCountry);
            return upcomingHolidays;
        }

        private static List<PublicHoliday> GetHolidaysIn7DaysRange(List<PublicHoliday> holidaysForEmployeeCountry)
        {
            DateTime startDate = DateTime.Today;
            DateTime endDate = DateTime.Today.AddDays(7);

            var upcomingHolidays = holidaysForEmployeeCountry
                .Where(x => x.Date >= startDate && x.Date <= endDate)
                .ToList();
            return upcomingHolidays;
        }
    }
}
