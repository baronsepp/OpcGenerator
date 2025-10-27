using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodeGenerator.Application.Abstractions;
using NodeGenerator.Application.Parsers;
using NodeGenerator.Application.Writers;
using NodeGenerator.Options;

namespace NodeGenerator;

public static class Program
{
	public static async Task<int> Main()
	{
		using var host = CreateHostBuilder().Build();

		var generator = host.Services.GetRequiredService<Generator>();
		await generator.GenerateAsync();

		return Environment.ExitCode;
	}

	private static IHostBuilder CreateHostBuilder()
	{
		return new HostBuilder()
			.ConfigureAppConfiguration(app =>
			{
				app.AddJsonFile("appsettings.json");
			})
			.ConfigureLogging(logging =>
			{
				logging.AddSimpleConsole();
			})
			.ConfigureServices((hostContext, services) =>
			{
				services.AddOptions();
				services.Configure<OpcOptions>(hostContext.Configuration.GetRequiredSection(OpcOptions.Section));
				services.Configure<XmlOptions>(hostContext.Configuration.GetSection(XmlOptions.Section));
				services.Configure<CsvOptions>(hostContext.Configuration.GetSection(CsvOptions.Section));

				services.AddSingleton<Generator>();
				services.AddSingleton<IFileWriter, JsonWriter>();
				services.AddSingleton<IFileParser>(provider =>
				{
					var config = provider.GetRequiredService<IConfiguration>();

					var fileName = config.GetValue<string>("InputFileName");
					if (string.IsNullOrEmpty(fileName))
					{
						throw new NullReferenceException("InputFileName");
					}

					var extension = Path.GetExtension(fileName);
					if (".xml".Equals(extension, StringComparison.OrdinalIgnoreCase))
					{
						var opc = provider.GetRequiredService<IOptions<OpcOptions>>();
						var xml = provider.GetRequiredService<IOptions<XmlOptions>>();
						return new XmlParser(xml, opc);
					}

					if (".csv".Equals(extension, StringComparison.OrdinalIgnoreCase))
					{
						var opc = provider.GetRequiredService<IOptions<OpcOptions>>();
						var csv = provider.GetRequiredService<IOptions<CsvOptions>>();
						return new CsvParser(csv, opc);
					}

					throw new NotSupportedException($"Extension '{extension}' is not supported.");
				});
			});
	}
}
