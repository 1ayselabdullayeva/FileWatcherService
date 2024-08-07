using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using FileWatcherLibrary;
using System;

namespace FileWatcherConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                                    .AddJsonFile("appsettings.json")
                                    .Build();

            string folderPath = configuration["FileSettings:FolderPath"];
            string logFilePath = configuration["FileSettings:LogFilePath"];


            Queue<string> fileQueue = new Queue<string>();
            Queue<string> failedQueue = new Queue<string>();


            FileMonitor fileMonitor = new FileMonitor(folderPath, logFilePath, fileQueue, failedQueue);
            fileMonitor.Start();
            Console.WriteLine("Press [enter] to exit...");
            Console.ReadLine();

            fileMonitor.Stop();
        }
    }
}

