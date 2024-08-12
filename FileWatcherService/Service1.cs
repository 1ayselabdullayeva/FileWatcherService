using FileWatcherLibrary;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace FileWatcherService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            WriteLog("Service is started");

            //////SaveDb();
            //string folderPath = "C:\\ProgramData\\3CX\\Instance1\\Data\\Recordings";
            //string logFilePath = "C:\\Users\\agilesolutions\\Desktop\\fw\\Log";

            //string folderPath = "C:\\Users\\Aysel\\source\\repos\\FileWatcher\\Location";
            //string logFilePath = "C:\\Users\\Aysel\\source\\repos\\FileWatcher\\Logs";
            //string databaseConfig = "Host=localhost;Port=5432;Database=FileWatcher;Username=postgres;Password=Aysel123";

            //  string databaseConfig = "Host=10.170.32.11;Port=5432;Database=dialerdb;Username=postgres;Password=!%aaQTFbbd*!";


            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("C:\\Users\\Aysel\\source\\repos\\FileWatcher\\FileWatcherService\\appsettings.json", optional: true, reloadOnChange: true).Build();

            string folderPath = config["FileSettings:FolderPath"];
            string logFilePath = config["FileSettings:LogFilePath"];
            string databaseConfig = config["ConnectionStrings:DefaultConnection"];
            Queue<string> fileQueue = new Queue<string>();
            Queue<string> failedQueue = new Queue<string>();
            FileMonitor fileMonitor = new FileMonitor(folderPath, logFilePath, fileQueue, failedQueue, databaseConfig);
            fileMonitor.Start();

            WriteLog("End of OnStart");

        }

        protected override void OnStop()
        {
            WriteLog("Service is stopped");
        }

        public void Test()
        {
            WriteLog("Test service is started");

            string folderPath = "C:\\Users\\Aysel\\source\\repos\\FileWatcher\\Location";
            string logFilePath = "C:\\Users\\Aysel\\source\\repos\\FileWatcher\\Logs";
            string databaseConfig = "Host=localhost;Port=5432;Database=FileWatcher;Username=postgres;Password=Aysel123";
            Queue<string> fileQueue = new Queue<string>();
            Queue<string> failedQueue = new Queue<string>();
            FileMonitor fileMonitor = new FileMonitor(folderPath, logFilePath, fileQueue, failedQueue, databaseConfig);
            fileMonitor.Start();

            WriteLog("End of start service, fileMonitor.Start();\r\n");

        }
        private void WriteLog(string msg)
        {
            var logFilePath = AppDomain.CurrentDomain.BaseDirectory + "\\log-file.txt";

            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{msg},date: {DateTime.Now}");
            }
        }

        
        private void SaveDb()
        {
            string databaseConfig = "Host=localhost;Port=5432;Database=FileWatcher;Username=postgres;Password=Aysel123";

            try
            {
                WriteLog($"try to  connect to db 234234");

                //var file = callRecord;
                using (var con = new NpgsqlConnection(databaseConfig))
                {
                    con.Open();

                    var cmd = con.CreateCommand();
                    cmd.CommandText = "INSERT INTO FILEINFO (CallInfo, Part1,Part2,DateTimeStr,FileName,CallID,FilePath,FolderPath) VALUES (@CallInfo, @Part1,@Part2,@DateTimeStr,@FileName,@CallID,@FilePath,@FolderPath)";
                    cmd.Parameters.AddWithValue("@CallInfo", "file.CallInfo");
                    cmd.Parameters.AddWithValue("@Part1", "file.Part1");
                    cmd.Parameters.AddWithValue("@Part2", "file.Part2");
                    cmd.Parameters.AddWithValue("@DateTimeStr", "file.DateTimeStr");
                    cmd.Parameters.AddWithValue("@FileName", "file.FileName");
                    cmd.Parameters.AddWithValue("@CallID", 1234);
                    cmd.Parameters.AddWithValue("@FilePath", "file.FilePath");
                    cmd.Parameters.AddWithValue("@FolderPath", "file.FolderPath");
                    var result = cmd.ExecuteNonQuery();

                    con.Close();
                }

                WriteLog($"end of db connection 43453");

            }
            catch (Exception e)
            {
                WriteLog($"failed connect to database, msg:" + e.Message);

                //throw;
            }

        }
        
    }
}
