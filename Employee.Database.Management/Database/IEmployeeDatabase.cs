namespace Employee.Database.Management.Database
{
    public interface IEmployeeDatabase
    {
        Task<Employee> AddEmployee(Employee employee);
        Task<List<Employee>> GetAll();
        Task<Employee> GetEmployee(Guid employeeId);
        Task UpdateEmployee(Employee updatedEmployee);
    }
}