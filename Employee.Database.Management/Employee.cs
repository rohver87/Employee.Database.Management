using System.ComponentModel.DataAnnotations;

namespace Employee.Database.Management
{
    public class Employee
    {
        [Key]
        public Guid EmployeeId { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string Email { get; set; }
        public float Salary { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string CountryCode { get; set; }
    }

}
