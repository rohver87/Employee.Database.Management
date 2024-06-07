using Microsoft.EntityFrameworkCore;

namespace Employee.Database.Management.Database.Tests
{
    public class EmployeeDatabaseTests
    {
        private readonly EmployeeDatabase _employeeDatabase;
        public EmployeeDatabaseTests()
        {
            var options = new DbContextOptionsBuilder<EmployeeDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _employeeDatabase = new EmployeeDatabase(new EmployeeDbContext(options));
        }

        [Fact]
        public async Task AddEmployee_ShouldAddNewEmployee()
        {
            // Arrange

            var employee = new Employee
            {
                Name = "John Doe",
                Position = "Software Engineer",
                Email = "john.doe@example.com",
                Salary = 60000,
                CountryCode = "US"
            };

            // Act
            var result = await _employeeDatabase.AddEmployee(employee);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.EmployeeId);
            Assert.Equal("John Doe", result.Name);
            Assert.Equal("Software Engineer", result.Position);
            Assert.Equal("john.doe@example.com", result.Email);
            Assert.Equal(60000, result.Salary);
            Assert.NotEqual(default(DateTime), result.CreatedAt);
            Assert.NotEqual(default(DateTime), result.ModifiedAt);
        }

        [Fact]
        public async Task AddEmployee_ShouldThrowException_WhenEmployeeIdAlreadyExists()
        {
            // Arrange
            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                Name = "Jane Doe",
                Position = "Product Manager",
                Email = "jane.doe@example.com",
                Salary = 80000,
                CountryCode = "US"
            };

            await _employeeDatabase.AddEmployee(employee);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await _employeeDatabase.AddEmployee(employee));
            Assert.Equal("Employee with this ID already exists.", ex.Message);
        }

        [Fact]
        public async Task UpdateEmployee_ShouldUpdateExistingEmployee()
        {
            // Arrange

            var employee = new Employee
            {
                Name = "John Doe",
                Position = "Software Engineer",
                Email = "john.doe@example.com",
                Salary = 60000,
                CountryCode = "US"

            };

            var addedEmployee = await _employeeDatabase.AddEmployee(employee);

            addedEmployee.Name = "John Smith";
            addedEmployee.Position = "Senior Software Engineer";

            // Act
            await _employeeDatabase.UpdateEmployee(addedEmployee);

            // Assert
            var updatedEmployee = await _employeeDatabase.GetEmployee(addedEmployee.EmployeeId);
            Assert.Equal("John Smith", updatedEmployee.Name);
            Assert.Equal("Senior Software Engineer", updatedEmployee.Position);
            Assert.Equal("john.doe@example.com", updatedEmployee.Email);
            Assert.Equal(60000, updatedEmployee.Salary);
        }

        [Fact]
        public async Task UpdateEmployee_ShouldThrowException_WhenEmployeeDoesNotExist()
        {
            // Arrange
            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                Name = "Non Existent",
                Position = "None",
                Email = "non.existent@example.com",
                Salary = 0,
                CountryCode = "US"

            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(async () => await _employeeDatabase.UpdateEmployee(employee));
            Assert.Equal("Employee with this ID does not exist.", ex.Message);
        }

        [Fact]
        public async Task GetEmployee_ShouldReturnEmployee_WhenEmployeeExists()
        {
            // Arrange

            var employee = new Employee
            {
                Name = "John Doe",
                Position = "Software Engineer",
                Email = "john.doe@example.com",
                Salary = 60000,
                CountryCode = "US"

            };

            var addedEmployee = await _employeeDatabase.AddEmployee(employee);

            // Act
            var result = await _employeeDatabase.GetEmployee(addedEmployee.EmployeeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(addedEmployee.EmployeeId, result.EmployeeId);
            Assert.Equal("John Doe", result.Name);
            Assert.Equal("Software Engineer", result.Position);
            Assert.Equal("john.doe@example.com", result.Email);
            Assert.Equal(60000, result.Salary);
        }

        [Fact]
        public async Task GetEmployee_ShouldThrowException_WhenEmployeeDoesNotExist()
        {
            // Arrange

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(async () => await _employeeDatabase.GetEmployee(Guid.NewGuid()));
            Assert.Equal("Employee with this ID does not exist.", ex.Message);
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllEmployees()
        {
            // Arrange

            var employee1 = new Employee
            {
                Name = "John Doe",
                Position = "Software Engineer",
                Email = "john.doe@example.com",
                Salary = 60000,
                CountryCode = "US"

            };
            var employee2 = new Employee
            {
                Name = "Jane Doe",
                Position = "Product Manager",
                Email = "jane.doe@example.com",
                Salary = 80000,
                CountryCode = "US"

            };

            await _employeeDatabase.AddEmployee(employee1);
            await _employeeDatabase.AddEmployee(employee2);

            // Act
            var result = await _employeeDatabase.GetAll();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, e => e.Name == "John Doe");
            Assert.Contains(result, e => e.Name == "Jane Doe");
        }
    }
}
