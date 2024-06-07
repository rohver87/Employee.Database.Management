using Employee.Database.Management.Service;
using Employee.Database.Management.Worker.EventHandler;

namespace Employee.Database.Management.Worker
{
    public class Worker : IHostedService,IDisposable
    {
        private readonly ILogger<Worker> _logger;
        private Timer? _timer = null;
        private readonly IConfiguration _configuration;
        private event EventHandler<EmailEvent> _emailEventTriggered;
        private readonly IServiceProvider _serviceProvider;
        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        //Timer function to run once in a day
        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(TimeSpan.FromDays(1).TotalSeconds));

            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            //Subcripe to events 
            var emailHandler = new EmailEventHandler();
            this._emailEventTriggered += emailHandler.OnEventTriggered;

            TriggerEmailAlert().GetAwaiter().GetResult();
        }

        private async Task TriggerEmailAlert()
        {
            using var scope = _serviceProvider.CreateScope();
            var scopedProcessingService =
                scope.ServiceProvider
                    .GetRequiredService<IPublicHolidayService>();

            var countryList = _configuration.GetValue<string>("CountryList");

            foreach (var country in countryList.Split(","))
            {
                var holidays = await scopedProcessingService.TriggerEmailAlert(country);

                foreach (var holiday in holidays)
                {
                    OnEventTriggered(holiday);
                }
            }

        }

        public virtual void OnEventTriggered(PublicHoliday holiday)
        {
            _emailEventTriggered?.Invoke(this, new EmailEvent()
            {
                Holiday = holiday
            });
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
