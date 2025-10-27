using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodeGenerator.Factories;
using NodeGenerator.Interfaces;
using NodeGenerator.Parsers;
using NodeGenerator.Writers;
using System.IO;
using System.Threading.Tasks;

namespace NodeGenerator
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();

            await host.RunAsync();

            return 0;
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(conf =>
                {
                    conf.SetBasePath(Directory.GetCurrentDirectory());
                    conf.AddJsonFile("appsettings.json", optional: false);
                    conf.AddCommandLine(args);
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddSimpleConsole(c =>
                    {
                        c.UseUtcTimestamp = true;
                        c.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
                    });
                    //logging.
                })
                .ConfigureServices((hostContext, services) =>
                {
                    if (hostContext.Configuration.GetValue<string>("InputFileName").EndsWith("csv"))
                        services.AddScoped<IFileParser, CsvParser>();

                    if (hostContext.Configuration.GetValue<string>("InputFileName").EndsWith("xml"))
                        services.AddScoped<IFileParser, XmlParser>();


                    services.AddScoped<IFileWriter, JsonWriter>();
                    services.AddTransient<IEndpointFactory, EndpointFactory>();

                    services.AddHostedService<GeneratorService>();
                });
        }
    }
}
