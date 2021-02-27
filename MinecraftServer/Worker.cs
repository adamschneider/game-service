using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MinecraftServer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private Process _process;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_process is null)
            {
                _process = new Process();
                _process.StartInfo.FileName = "java";
                _process.StartInfo.Arguments = "-Xms1G -Xmx4G -jar C:\\Users\\ajssc\\personal\\minecraft\\server.jar --nogui";
                _process.StartInfo.WorkingDirectory = "C:\\Users\\ajssc\\personal\\minecraft";
                _process.StartInfo.CreateNoWindow = true;
                _process.StartInfo.UseShellExecute = false;
                _process.StartInfo.RedirectStandardOutput = true;
                _process.StartInfo.RedirectStandardError = true;
                _process.StartInfo.RedirectStandardInput = true;
                _process.OutputDataReceived += (sender, data) => _logger.LogInformation(data.Data);
                _process.ErrorDataReceived += (sender, data) => _logger.LogError(data.Data);
                _process.Start();
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
            }
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_process != null)
            {
                await _process.StandardInput.WriteLineAsync("stop");
            }
            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override void Dispose()
        {
            if (_process != null)
            {
                _process.Dispose();
                _process = null;
            }   
        }
    }
}
