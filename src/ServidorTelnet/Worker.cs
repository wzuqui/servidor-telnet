using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServidorTelnet.Telnet;

namespace ServidorTelnet
{
    public class Worker : IHostedService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly TelnetServer _telnetServer;

        public Worker(ILogger<Worker> logger, IHostApplicationLifetime appLifetime)
        {
            _logger = logger;
            _appLifetime = appLifetime;
            _telnetServer = new TelnetServer();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }


        private void OnStarted()
        {
            _logger.LogInformation("OnStarted foi chamado.");

            _telnetServer.Iniciar(IPAddress.Any, 9090);
        }

        private void OnStopping()
        {
            _logger.LogInformation("OnStopping foi chamado.");

            _telnetServer.Parar();
        }

        private void OnStopped()
        {
            _logger.LogInformation("OnStopped foi chamado.");
        }
    }
}