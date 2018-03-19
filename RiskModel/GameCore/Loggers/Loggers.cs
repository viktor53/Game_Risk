using System;
using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Risk.Model.GameCore.Loggers
{
  /// <summary>
  /// Class that provides loggers.
  /// </summary>
  public static class Loggers
  {
    /// <summary>
    /// Creates logger with RollingFileAppender that writes logs into file named as current date.
    /// dd.MM.yyyy[hh.mm.ss].log
    /// </summary>
    /// <param name="pathToLogDir">absolute path to directory with logs</param>
    /// <returns>ILog that writes logs into file</returns>
    public static ILog GetDateFileLogger(string pathToLogDir)
    {
      // creates log message pattern
      PatternLayout pattern = new PatternLayout();
      pattern.ConversionPattern = "%message%newline";
      pattern.ActivateOptions();

      DateTime date = DateTime.Now;

      // setup RollingFileAppender
      RollingFileAppender roller = new RollingFileAppender();
      roller.AppendToFile = true;
      roller.File = pathToLogDir + $"\\{date.Day}.{date.Month}.{date.Year}[{date.Hour}.{date.Minute}.{date.Second}].log";
      roller.Layout = pattern;
      roller.MaximumFileSize = "500KB";
      roller.RollingStyle = RollingFileAppender.RollingMode.Size;
      roller.StaticLogFileName = true;
      roller.ActivateOptions();
      roller.Name = date.ToFileTime().ToString();

      // creates logger
      ILog log = LogManager.GetLogger(date.ToFileTime().ToString());
      var logger = (Logger)log.Logger;
      logger.AddAppender(roller);
      logger.Level = logger.Hierarchy.LevelMap["ALL"];
      return log;
    }
  }
}