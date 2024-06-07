
namespace Employee.Database.Management.Service
{
    public interface IPublicHolidayService
    {
        Task<List<PublicHoliday>> GetPublicHolidayByCountryCodeAndYear(string countryCode, int year);

        Task<List<PublicHoliday>> GetPublicHolidayForCurrent7DaysByEmployee(Guid employeeId);

        Task<List<PublicHoliday>> TriggerEmailAlert(string countryCode);
    }
}