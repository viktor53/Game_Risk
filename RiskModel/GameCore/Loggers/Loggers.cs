using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Risk.Model.GameCore.Loggers
{
  public static class Loggers
  {
    public static ILog GetDateFileLogger(string pathToLogDir)
    {
      PatternLayout pattern = new PatternLayout();
      pattern.ConversionPattern = "%message%newline";
      pattern.ActivateOptions();

      DateTime date = DateTime.Now;

      RollingFileAppender roller = new RollingFileAppender();
      roller.AppendToFile = true;
      roller.File = pathToLogDir + $"\\{date.Day}.{date.Month}.{date.Year}[{date.Hour}.{date.Minute}.{date.Second}].log";
      roller.Layout = pattern;
      roller.MaximumFileSize = "500KB";
      roller.RollingStyle = RollingFileAppender.RollingMode.Size;
      roller.StaticLogFileName = true;
      roller.ActivateOptions();
      roller.Name = date.ToFileTime().ToString();

      ILog log = LogManager.GetLogger(date.ToFileTime().ToString());
      var logger = (Logger)log.Logger;
      logger.AddAppender(roller);
      logger.Level = logger.Hierarchy.LevelMap["ALL"];
      return log;
    }
  }
}