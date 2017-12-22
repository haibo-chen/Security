using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;

namespace insp.Utility.Log
{
    /// <summary>
    /// 
    /// </summary>
    public static class LogUtils
    {
        public static ILog CreateFileLogger(String name,String path)
        {            
            ///LevelRangeFilter  
            log4net.Filter.LevelRangeFilter levfilter = new log4net.Filter.LevelRangeFilter();
            levfilter.LevelMax = log4net.Core.Level.Fatal;
            levfilter.LevelMin = log4net.Core.Level.Error;
            levfilter.ActivateOptions();
            //Appender1  
            log4net.Appender.FileAppender appender1 = new log4net.Appender.FileAppender();
            appender1.AppendToFile = true;
            appender1.File = path + name + ".log";
            appender1.ImmediateFlush = true;
            appender1.LockingModel = new log4net.Appender.FileAppender.MinimalLock();
            appender1.Name = name+"FileAppender";
            appender1.AddFilter(levfilter);

            //Appender2  
            //log4net.Appender.ConsoleAppender appender2 = new log4net.Appender.ConsoleAppender();                                   
            //appender2.Name = name + "ConsoleAppender";
            //appender2.AddFilter(levfilter);

            log4net.Layout.PatternLayout layout = new log4net.Layout.PatternLayout("%date [%thread] %-5level - %message%newline");
            layout.ActivateOptions();

            appender1.Layout = layout;
            appender1.ActivateOptions();
            //appender2.Layout = layout;
            //appender2.ActivateOptions();

            //log4net.Repository.ILoggerRepository repository = null;
            //log4net.Repository.ILoggerRepository[] repositories = log4net.LogManager.GetAllRepositories();
            //if (repositories != null)
            //   repository = repositories.FirstOrDefault(x => x.Name == "DynamicRepository");
            //if (repository == null)            
            //    repository = log4net.LogManager.CreateRepository("DynamicRepository");
            
            log4net.Config.BasicConfigurator.Configure(appender1);
            //log4net.Config.BasicConfigurator.Configure(repository, appender1);
            //log4net.Config.BasicConfigurator.Configure(repository, appender2);

            //((log4net.Repository.Hierarchy.Hierarchy)repository).Root.Level = log4net.Core.Level.Info;
            ((log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository()).Root.AddAppender(appender1);
            
            
            ILog logger = log4net.LogManager.GetLogger(name);
            return logger;
        }
    }
}
