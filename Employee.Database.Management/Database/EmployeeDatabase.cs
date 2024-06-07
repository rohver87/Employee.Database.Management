using Microsoft.EntityFrameworkCore;

namespace Employee.Database.Management.Database
{
    public class EmployeeDatabase : IEmployeeDatabase
    {
        private readonly EmployeeDbContext _context;

        public EmployeeDatabase(EmployeeDbContext context)
        {
            _context = context;
        }

        public async Task<Employee> AddEmployee(Employee employee)
        {
            if (employee.EmployeeId == Guid.Empty)
            {
                employee.EmployeeId = Guid.NewGuid();
            }
            else if(await _context.Employees.AnyAsync(e => e.EmployeeId == employee.EmployeeId))
            {
                throw new ArgumentException("Employee with this ID already exists.");
            }

            var currentUtcDate = DateTime.UtcNow;

            employee.CreatedAt = currentUtcDate;
            employee.ModifiedAt = currentUtcDate;

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task UpdateEmployee(Employee updatedEmployee)
        {
            var existingEmployee = await _context.Employees.FindAsync(updatedEmployee.EmployeeId);
            if (existingEmployee == null)
            {
                throw new KeyNotFoundException("Employee with this ID does not exist.");
            }

            updatedEmployee.ModifiedAt = DateTime.UtcNow;
            _context.Entry(existingEmployee).CurrentValues.SetValues(updatedEmployee);
            await _context.SaveChangesAsync();
        }

        public async Task<Employee> GetEmployee(Guid employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                throw new KeyNotFoundException("Employee with this ID does not exist.");
            }

            employee.CreatedAt = ConvertFromUtc(employee.CreatedAt);
            employee.ModifiedAt = ConvertFromUtc(employee.ModifiedAt);

            return employee;
        }

        public async Task<List<Employee>> GetAll()
        {
            var employees =  await _context.Employees.ToListAsync();

            foreach (var employee in employees)
            {
                employee.CreatedAt = ConvertFromUtc(employee.CreatedAt);
                employee.ModifiedAt = ConvertFromUtc(employee.ModifiedAt);
            }

            return employees;
        }

        public override string ToString()
        {
            return string.Join("\n", GetAll().GetAwaiter().GetResult().Select(e => e.ToString()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="utcDateTimeOffset"></param>
        /// <returns></returns>
        private static DateTime ConvertFromUtc(DateTimeOffset utcDateTimeOffset)
        {
            return  TimeZoneInfo.ConvertTimeFromUtc(utcDateTimeOffset.UtcDateTime, TimeZoneInfo.Local);
        }
    }


}
