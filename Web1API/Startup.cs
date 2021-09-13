using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web1Api.Models;

namespace Web1Api
{
    public class Startup
    {
        private const int StartupLoggingEventID = 1;
        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";  // LTPE Enable Cors

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>  // LTPE Enable Cors
            {
                options.AddPolicy(MyAllowSpecificOrigins,
                builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                    //.AllowCredentials();
                           .SetIsOriginAllowed((host) => true);
                });
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Web1Api", Version = "v1" });
            });

            services.AddDbContext<DatabaseContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("WebApiContext")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env
            #if Test_Logging
                , ILogger<Startup> logger
            #endif
            )
        {

#if Test_Logging
            // below code is needed to get User name for Log             
            app.Use(async (httpContext, next) =>
            {
                var UserName = "Guest"; //Gets user Name from user Identity  
                Serilog.Context.LogContext.PushProperty("UserName", UserName); //Push user in LogContext;  
                await next.Invoke();
            }
            );

            logger.LogInformation(StartupLoggingEventID, "In Startup.cs configure");

            if (env.IsDevelopment())
            {
                logger.LogInformation(StartupLoggingEventID, "IsDevelopment active");
            }

            if (env.IsEnvironment("Test"))
            {
                logger.LogInformation(StartupLoggingEventID, "IsEnvironment active");
            }

            if (env.IsStaging())
            {
                logger.LogInformation(StartupLoggingEventID, "IsStaging active");
            }

            if (env.IsProduction())
            {
                logger.LogInformation(StartupLoggingEventID, "IsProduction active");
            }
#endif
            if ( (env.IsDevelopment()) || (env.IsProduction()) )
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web1Api v1"));
            }
                                    
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseCors(MyAllowSpecificOrigins);  // LTPE Enable Cors. Denne linje kode skal med !!!

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            Console.WriteLine($"MyCustomKey - {Configuration["MyCustomKey"]}");

#if Test_Logging
            logger.LogInformation(StartupLoggingEventID, "MyCustomKey : " + Configuration["MyCustomKey"].ToString());
            logger.LogWarning(StartupLoggingEventID, "MyCustomKey : " + Configuration["MyCustomKey"].ToString());
            logger.LogWarning(StartupLoggingEventID, "ConnectionString : " + Configuration.GetConnectionString("WebApiContext"));
#endif
        }
    }
}
