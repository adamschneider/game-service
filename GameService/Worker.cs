using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GameService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ServerConfiguration _configuration;
        private TaskCompletionSource<int> _serviceStoppedTask;
        private Process _process;

        public Worker(ILogger<Worker> logger, ServerConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_process is null)
            {
                _serviceStoppedTask = new TaskCompletionSource<int>();
                _process = new Process();
                _process.StartInfo.FileName = _configuration.Executable;
                _process.StartInfo.Arguments = _configuration.Arguments;
                _process.StartInfo.WorkingDirectory = _configuration.WorkingDirectory;
                _process.StartInfo.CreateNoWindow = true;
                _process.StartInfo.UseShellExecute = false;
                _process.StartInfo.RedirectStandardOutput = true;
                _process.StartInfo.RedirectStandardError = true;
                _process.StartInfo.RedirectStandardInput = true;
                _process.EnableRaisingEvents = true;
                _process.OutputDataReceived += (sender, data) => _logger.LogInformation(data.Data);
                _process.ErrorDataReceived += (sender, data) => _logger.LogError(data.Data);
                _process.Exited += (sender, data) =>
                {
                    _logger.LogInformation("Server stopped.");
                    _serviceStoppedTask.TrySetResult(0);
                };
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
                await _process.StandardInput.WriteLineAsync(_configuration.StopInput);
                await _serviceStoppedTask.Task;
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
