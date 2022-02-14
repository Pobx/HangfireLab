using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HangfireLab.Controllers {
  [ApiController]
  [Route ("[controller]")]
  public class WeatherForecastController : ControllerBase {
    private static readonly string[] Summaries = new [] {
      "Freezing",
      "Bracing",
      "Chilly",
      "Cool",
      "Mild",
      "Warm",
      "Balmy",
      "Hot",
      "Sweltering",
      "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController (ILogger<WeatherForecastController> logger) {
      _logger = logger;
    }

    [HttpGet]
    public IEnumerable<WeatherForecast> Get () {
      var rng = new Random ();
      return Enumerable.Range (1, 5).Select (index => new WeatherForecast {
          Date = DateTime.Now.AddDays (index),
            TemperatureC = rng.Next (-20, 55),
            Summary = Summaries[rng.Next (Summaries.Length)]
        })
        .ToArray ();
    }

    [HttpPost ("App1")]
    public void AddHangfireJob1 () {
      RecurringJob.AddOrUpdate ("easyjob", () => Console.Write ("Easy!"), Cron.Daily);
    }

    [HttpPost ("App2")]
    public void AddHangfireJob2 () {
      Guid guid = Guid.NewGuid ();
      Type executor = Type.GetType ("HangfireLab.Services.TestService");

      if (executor == null) {
        Console.WriteLine ("NOT_FOUND");
        return;
      }

      MethodInfo methodInfo = executor.GetMethod ("ExecuteAsync");

      Console.WriteLine ($"executor ==> {executor}");
      Console.WriteLine ($"methodInfo ==> {methodInfo}");

      var job = new Job (executor, methodInfo);
      var manager = new RecurringJobManager ();

      manager.AddOrUpdate ($"Task-{guid}", job, "* * * * *", TimeZoneInfo.Local, "default");
    }

    [HttpPost ("App3")]
    public void RemoveHangfireJob () {
      RecurringJob.RemoveIfExists ("Task-1abc778b-d4d0-4f19-b770-3918ce9634b9	");
    }
  }
}