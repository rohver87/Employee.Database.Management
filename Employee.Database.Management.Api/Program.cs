using Employee.Database.Management.Api;
using Employee.Database.Management.Database;
using Employee.Database.Management.Service;
using Microsoft.EntityFrameworkCore;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<EmployeeDbContext>(options =>
            options.UseSqlite("Data Source=employees.db"));
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddScoped<IEmployeeDatabase, EmployeeDatabase>();
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
