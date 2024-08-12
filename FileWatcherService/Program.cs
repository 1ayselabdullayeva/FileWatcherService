using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.ServiceProcess;
using FileWatcherLibrary;
using System.Diagnostics;

namespace FileWatcherService
{
    internal static class Program
    {
        static void Main()
        {
#if (!DEBUG)
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service1()
            };
            //var configuration = new ConfigurationBuilder()
            //            .AddJsonFile("C:\\Users\\agilesolutions\\Desktop\\Release\\appsettings.json")
            //            .Build();
            //string folderPath = configuration["FileSettings:FolderPath"];
            //string logFilePath = configuration["FileSettings:LogFilePath"];
            //string databaseConfig = configuration["ConnectionStrings:DefaultConnection"];
            //string folderPath = "C:\\Users\\Aysel\\source\\repos\\FileWatcher\\Location";
            //string logFilePath = "C:\\Users\\Aysel\\source\\repos\\FileWatcher\\Logs";
            //string databaseConfig = "Host=localhost;Port=5432;Database=FileWatcher;Username=postgres;Password=Aysel123";
            //Queue<string> fileQueue = new Queue<string>();
            //Queue<string> failedQueue = new Queue<string>();
            //FileMonitor fileMonitor = new FileMonitor(folderPath, logFilePath, fileQueue, failedQueue,databaseConfig);
            //fileMonitor.Start();
            ServiceBase.Run(ServicesToRun);
#else
            new Service1().Test();
#endif
        }
    }
}
