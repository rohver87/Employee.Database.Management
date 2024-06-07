using Employee.Database.Management.Database;
using Employee.Database.Management.Service;
using Employee.Database.Management.Worker;
using Microsoft.EntityFrameworkCore;
using Polly;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddScoped<IPublicHolidayService, PublicHolidayService>();
string? HolidayApiClient = builder.Configuration["HolidayApiClient"];
ArgumentException.ThrowIfNullOrEmpty(HolidayApiClient);

string? HolidayApiUrl = builder.Configuration["HolidayApiUrl"];
ArgumentException.ThrowIfNullOrEmpty(HolidayApiUrl);

//HttpClient with retry and circuit breaker
builder.Services.AddHttpClient(
    HolidayApiClient,
    client =>
    {
        client.BaseAddress = new Uri(HolidayApiUrl);
    })
    .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(new[]
      {
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(10)
    }))
    .AddTransientHttpErrorPolicy(builder => builder.CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 3,
        durationOfBreak: TimeSpan.FromSeconds(30)
    ));

//in memory cache 
builder.Services.AddDistributedMemoryCache();

builder.Services.AddDbContext<EmployeeDbContext>(options =>
            options.UseSqlite("Data Source=employees.db"));
builder.Services.AddScoped<IEmployeeDatabase, EmployeeDatabase>();

var host = builder.Build();
host.Run();
