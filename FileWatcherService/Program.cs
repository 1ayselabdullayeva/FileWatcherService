using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.ServiceProcess;
using FileWatcherLibrary;

namespace FileWatcherService
{
    internal static class Program
    {
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service1()
            };
            var configuration = new ConfigurationBuilder()
                                   .AddJsonFile("appsettings.json")
                                   .Build();

            string folderPath = configuration["FileSettings:FolderPath"];
            string logFilePath = configuration["FileSettings:LogFilePath"];

            Queue<string> fileQueue = new Queue<string>();
            Queue<string> failedQueue = new Queue<string>();


            FileMonitor fileMonitor = new FileMonitor(folderPath, logFilePath, fileQueue, failedQueue);
            fileMonitor.Start();
            ServiceBase.Run(ServicesToRun);
        }
    }
}
