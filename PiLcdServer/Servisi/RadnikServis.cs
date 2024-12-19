using Microsoft.Extensions.Options;
using PiLcdServer.Modeli;

namespace PiLcdServer.Servisi
{

    public class RadnikServis : IHostedService, IDisposable
    {
        private readonly ILogger<RadnikServis> _logger;
        private Timer? _timer = null;
        private readonly LcdServis _lcdServis;
        private int executionCount = 0;
        private readonly AppSettings _appSettings;


        public RadnikServis(ILogger<RadnikServis> loggerFactory, LcdServis lcdServis, IOptions<AppSettings> options)
        {
            _logger = loggerFactory;
            _lcdServis = lcdServis;
            _appSettings = options.Value;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            var count = Interlocked.Increment(ref executionCount);

            if (_appSettings.AutoTurnOffDisplayEnabled)
            {
                var diplays = _lcdServis.GetDisplayConfigs().Where(f => !f.ErrorState && DateTime.Now.Subtract(f.LastWrite ?? DateTime.Now).TotalSeconds > (double)_appSettings.AutoTurnOffDisplayAfterSec);
                foreach (var diplay in diplays) 
                {
                    _lcdServis.UgasiBacklgiht(diplay.IntAdressaIc2Displaya);
                    _logger.LogDebug($"Ugasio sam display {diplay.IntAdressaIc2Displaya}");
                }
            }

            //var a = new Modeli.PisiModel() { LcdUlazIliIzlaz = "ULAZ", DrugiRed = $"Sati {DateTime.Now.ToString("HH:mm:ss")}", PrviRed=$"Klikono sam x{count}" };

            //_lcdServis.PisiNaDisplay(a);
            //a.LcdUlazIliIzlaz = "IZLAZ";
            //_lcdServis.PisiNaDisplay(a);
            //_logger.LogInformation("Timed Hosted Service is working. Count: {Count}", count);
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
