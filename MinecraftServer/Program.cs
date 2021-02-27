using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MinecraftServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                var serverConfiguration = hostContext.Configuration.GetSection("ServerConfiguartion").Get<ServerConfiguration>();
                services.AddSingleton(serverConfiguration);
                services.AddHostedService<Worker>();
            })
            .UseWindowsService();
    }
}
