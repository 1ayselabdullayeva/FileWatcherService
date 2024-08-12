using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using FileWatcherLibrary;
using System;
using System.IO;

namespace FileWatcherConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("C:\\Users\\agilesolutions\\Desktop\\net5.0\\appsettings.json", optional: true, reloadOnChange: true).Build();

            string folderPath = config["FileSettings:FolderPath"];
            string logFilePath = config["FileSettings:LogFilePath"];
            string databaseConfig = config["ConnectionStrings:DefaultConnection"];

            WavFileCollector wavFileCollector = new WavFileCollector(new FileMonitor(folderPath, logFilePath, databaseConfig));
            var resp = wavFileCollector.GetWavFiles(folderPath);
            wavFileCollector.SaveWavFilesToDatabase(resp);


            Console.WriteLine("Press [enter] to exit...");
            Console.ReadLine();


        }
    }
}

