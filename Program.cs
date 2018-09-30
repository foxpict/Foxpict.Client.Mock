﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace foxpict.client.web {
  /// <summary>
  /// プログラム本体
  /// </summary>
  public class Program {
    /// <summary>
    /// エントリポイント
    /// </summary>
    /// <param name="args"></param>
    public static void Main (string[] args) {
      // NLog: setup the logger first to catch all errors
      var logger = NLog.Web.NLogBuilder.ConfigureNLog ("nlog.config").GetCurrentClassLogger ();
      try {
        BuildWebHost (args).Run ();
      } catch (Exception ex) {
        //NLog: catch setup errors
        logger.Error (ex, "Stopped program because of exception");
        throw;
      } finally {
        // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
        NLog.LogManager.Shutdown ();
      }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static IWebHost BuildWebHost (string[] args) =>
      WebHost.CreateDefaultBuilder (args)
      .ConfigureAppConfiguration ((hostingContext, config) => {
        var env = hostingContext.HostingEnvironment;
        config.AddJsonFile ("appsettings.json", optional : true, reloadOnChange : true)
          .AddJsonFile ($"appsettings.{env.EnvironmentName}.json", optional : true, reloadOnChange : true);
        config.AddEnvironmentVariables ();
      })
      .ConfigureLogging ((hostingContext, logging) => {
        logging.ClearProviders ();
        logging.SetMinimumLevel (Microsoft.Extensions.Logging.LogLevel.Trace);
      })
      .UseStartup<Startup> ()
      .Build ();
  }
}
