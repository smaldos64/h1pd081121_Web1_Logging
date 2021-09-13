using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Web1Api
{
    public class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .Build();

        public static void Main(string[] args)
        {
#if Test_Logging
#if Test_Serilog
            string ConnectionString = Configuration.GetConnectionString("WebApiContext");

            var columnOptions = new ColumnOptions
            {
                AdditionalColumns = new Collection<SqlColumn>
               {
                   new SqlColumn("UserName", SqlDbType.VarChar)
               }
            }; //through this coulmnsOptions we can dynamically  add custom columns which we want to add in database 

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(new JsonFormatter(), "SeriLogs/Serilog-Common-" + ".txt",
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                    rollingInterval: RollingInterval.Day)
                .WriteTo.File("SeriLogs/Serilog-Errors-" + DateTime.UtcNow.ToString("yyyyMMdd") + ".txt",
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning)
                .WriteTo.MSSqlServer(ConnectionString, sinkOptions: new MSSqlServerSinkOptions { TableName = "TodoItems_Log" }
               , null, null, Serilog.Events.LogEventLevel.Information, null, columnOptions: columnOptions, null, null)
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Error)
                .Enrich.FromLogContext()
                .CreateLogger();
#endif
#endif

            CreateHostBuilder(args).Build().Run();

#if Test_Logging
#if Test_Serilog
            Log.Information("Nu er vores Web Api startet op !!!");
#endif
#endif
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
#if Test_Logging
#if Test_Serilog
                .UseSerilog()
#endif
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddEventLog();
                    logging.AddTraceSource("Information, ActivityTracing");
                    logging.AddFile("Logs/mylog-{Date}.txt");
#if Test_Seqlog
                    logging.AddSeq();
#endif
                })
#endif
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
