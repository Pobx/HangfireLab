using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace HangfireLab {
  public class Startup {
    public Startup (IConfiguration configuration) {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices (IServiceCollection services) {

      services.AddControllers ();
      services.AddSwaggerGen (c => {
        c.SwaggerDoc ("v1", new OpenApiInfo { Title = "HangfireLab", Version = "v1" });
      });

      // Add Hangfire services.
      services.AddHangfire (configuration => configuration
        .SetDataCompatibilityLevel (CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer ()
        .UseRecommendedSerializerSettings ()
        .UseSqlServerStorage (Configuration.GetConnectionString ("HangfireConnection"), new SqlServerStorageOptions {
          CommandBatchMaxTimeout = TimeSpan.FromMinutes (5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes (5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));

      // Add the processing server as IHostedService
      services.AddHangfireServer ();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure (IApplicationBuilder app, IWebHostEnvironment env, IBackgroundJobClient backgroundJobs) {
      if (env.IsDevelopment ()) {
        app.UseDeveloperExceptionPage ();
        app.UseSwagger ();
        app.UseSwaggerUI (c => c.SwaggerEndpoint ("/swagger/v1/swagger.json", "HangfireLab v1"));
      }

      app.UseHttpsRedirection ();

      app.UseRouting ();

      app.UseAuthorization ();

      app.UseHangfireDashboard ();
      backgroundJobs.Enqueue (() => Console.WriteLine ("Hello world from Hangfire!"));

      app.UseEndpoints (endpoints => {
        endpoints.MapControllers ();
        endpoints.MapHangfireDashboard();
      });
    }
  }
}