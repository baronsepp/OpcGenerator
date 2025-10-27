using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodeGenerator.Interfaces;
using NodeGenerator.Parsers;
using NodeGenerator.Writers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NodeGenerator
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();

            var generator = host.Services.GetRequiredService<GeneratorService>();
            var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
            await generator.StartAsync(lifetime.ApplicationStopping);

            return Environment.ExitCode;
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseConsoleLifetime()
                .ConfigureLogging(logging =>
                {
                    logging.AddSimpleConsole(c =>
                    {
                        c.UseUtcTimestamp = true;
                        c.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
                    });
                })
                .ConfigureServices((_, services) =>
                {
                    services.AddSingleton<GeneratorService>();
                    services.AddSingleton<IFileWriter, JsonWriter>();
                    services.AddSingleton<IFileParser>(provider =>
                    {
                        var config = provider.GetRequiredService<IConfiguration>();

                        var fileName = config.GetValue<string>("InputFileName");
                        if (string.IsNullOrEmpty(fileName)) throw new NullReferenceException("InputFileName");

                        var extension = Path.GetExtension(fileName);
                        if (".xml".Equals(extension, StringComparison.OrdinalIgnoreCase))
                        {
                            var logger = provider.GetRequiredService<ILogger<XmlParser>>();
                            return new XmlParser(config);
                        }

                        if (".csv".Equals(extension, StringComparison.OrdinalIgnoreCase))
                        {
                            var logger = provider.GetRequiredService<ILogger<CsvParser>>();
                            return new CsvParser(config, logger);
                        }

                        throw new NotSupportedException($"Extension '{extension}' is not supported.");
                    });
                });
        }
    }
}
