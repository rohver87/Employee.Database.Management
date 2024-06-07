# Employee.Database.Management

Project setup

Employee.Database.Management.Api (startup project)

	=> Contains the endpoints for employee database management and public holiday 

Employee.Database.Management

    => Contains the database class for employee and public holiday service

Employee.Database.Management.Worker 
   
    => Cron job worker checks if email needs to be triggered for upcoming holidays. Event is raised when email has to be triggered, 
      which will be handled by the respective hanlders

Employee.Database.Management.Tests

    => Contains the unit test cases


Swagger endpoint:

http://localhost:5205/swagger/index.html

Public holiday Service:

3rd party API is configured to get the holiday for a given country and given year. 

Example: https://date.nager.at/api/v3/publicholidays/2024/ES

Employee class is modifed to add countryCode. 
Upcoming holidays can be found based on the country code of the employee.
Data is cached by key = "holidaylist-{countryCode}-{year}".
HttpClient is configured for reties and circuit breaker.

====================
Problem solving case
====================

We can handle the new use case in few ways.

Option1: 
We can add a priority field in the request_category. Higher priority entry will be considered. 
For this we can allow the multiple rows to get inserted into time_off_request table and let the priority decide which one to consider.

Option2:
We can keep the contraint in place and use the splitting to rows in case when both are valid, 
such that there are not overlapping period

For example: 

WFH row exists for 01-06-2024 (start_date) - 30-06-2024 (end_date).

Now new request for Annual leave comes for 11-06-2024. Then we do following in a single transaction:

1. Update existing row for WFH from 01-06-2024 (start_date) - 10-06-2024 (end_date)
2. Add new row for Annual leave from 11-06-2024 (start_date) - 11-06-2024 (end_date)
3. Add new row for WFH from 12-06-2024 (start_date) - 30-06-2024 (end_date)

For data consistency, option 2 would be a better choice





