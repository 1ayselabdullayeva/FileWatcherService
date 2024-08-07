using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using Npgsql;
using System;

namespace FileWatcherLibrary
{
    public class FileMonitor
    {
        private FileSystemWatcher _watcher;
        private string _logFilePath;
        private string _folderPath;
        private Queue<string> _fileQueue;
        private Queue<string> _failedQueue;
        private static readonly object _dbLock = new object();

        public FileMonitor(string folderPath, string logFilePath, Queue<string> fileQueue, Queue<string> failedQueue)
        {
            _logFilePath = logFilePath;
            _fileQueue = fileQueue;
            _folderPath = folderPath;
            _failedQueue = failedQueue;
        }

        public void Start()
        {
            _watcher = new FileSystemWatcher
            {
                Path = _folderPath,
                Filter = "*.*",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.CreationTime,
                IncludeSubdirectories = true
            };
            _watcher.EnableRaisingEvents = true;

            _watcher.Created += async (sender, e) => await OnCreated(sender, e);
        }

        public void Stop()
        {
            _watcher.EnableRaisingEvents = false;

        }

        private async Task OnCreated(object sender, FileSystemEventArgs e)
        {
            if (Directory.Exists(e.FullPath))
            {

                LogFolderCreation(e.FullPath);
            }
            else if (Path.GetExtension(e.FullPath).Equals(".txt", StringComparison.OrdinalIgnoreCase))
            {
                Task.Run(() =>
                {
                    if (DateTime.Now.Second % 5 == 0)
                    {
                        ProcessQueueForFailed();
                    }
                });

                Task.Run(() =>
                {
                    _fileQueue.Enqueue(e.FullPath);
                });

                Task.Run(() =>
                {
                    ProcessQueue();
                });
            }
        }
        private void ProcessQueue()
        {
            string filePath = _fileQueue.Peek();
            var res = ParseFileName(filePath);
            if (res == true)
            {
                _fileQueue.Dequeue();
            }
        }
        private void ProcessQueueForFailed()
        {
            while (_failedQueue.Count > 0)
            {
                string filePath = _failedQueue.Peek();

                var res = ParseFileName(filePath);

                if (res == true)
                {
                    _failedQueue.Dequeue();
                }
                else
                {
                    LogFailedFile(filePath);
                    _failedQueue.Dequeue();
                }
            }
        }
        private void LogFailedFile(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(_logFilePath, true))
            {
                writer.WriteLine($"Failed to process file: {filePath} at {DateTime.Now}");
            }
        }

        private bool ParseFileName(string filePath)

        {
            string fileName = Path.GetFileName(filePath);

            string[] parts = fileName.Split('_');

            if (parts.Length < 3)
            {
                Console.WriteLine("File name does not match expected format.");
                return false;
            }

            string callInfo = Regex.Replace(parts[0], @"[\[\]]", "");
            string[] partyParts = parts[1].Split('-');
            string part1 = partyParts.Length > 0 ? partyParts[0] : string.Empty;
            string part2 = partyParts.Length > 1 ? partyParts[1] : string.Empty;

            string dateTimeStr = Regex.Match(parts[2], @"(\d{14})").Value;
            string callIdMatch = Regex.Match(parts[2], @"\((\d+)\)").Groups[1].Value;
            string callId = !string.IsNullOrEmpty(callIdMatch) ? callIdMatch : string.Empty;
            Model.FileInfo callRecord = new Model.FileInfo
            {
                CallInfo = callInfo,
                Part1 = part1,
                Part2 = part2,
                DateTimeStr = dateTimeStr,
                CallID = callId,
                FileName = fileName,
                FilePath = filePath,
                FolderPath = _folderPath
            };
            LogFileName(callRecord);
            return AddFileInfo(callRecord);

        }

        private void LogFileName(Model.FileInfo obj)
        {
            try
            {
                var callInfo = obj.CallInfo;
                var party1 = obj.Part1;
                var party2 = obj.Part2;
                var dateTimeStr = obj.DateTimeStr;
                var callId = obj.CallID;
                var fileName = obj.FileName;
                var filePath = obj.FilePath;
                var folderPath = obj.FolderPath;

                using (StreamWriter sw = new StreamWriter(_logFilePath, true))
                {
                    sw.WriteLine($"{DateTime.Now}: {fileName} was created.");
                    sw.WriteLine($"CallInfo: {callInfo}");
                    sw.WriteLine($"Party1: {party1}");
                    sw.WriteLine($"Party2: {party2}");
                    sw.WriteLine($"DateTimeStr: {dateTimeStr}");
                    sw.WriteLine($"CallID: {callId}");
                    sw.WriteLine($"FileName: {fileName}");
                    sw.WriteLine($"FilePath: {filePath}");
                    sw.WriteLine($"FolderPath: {folderPath}");

                    sw.WriteLine();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while logging the results: {ex.Message}");
            }

        }
        private void LogFolderCreation(string folderPath)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(_logFilePath, true))
                {
                    sw.WriteLine($"Folder Created: {folderPath}");
                    sw.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while logging the folder creation: {ex.Message}");
            }
        }
        public bool AddFileInfo(FileWatcherLibrary.Model.FileInfo file)
        {
            string connectionString = "Host=localhost;Port=5432;Database=FileWatcher;Username=postgres;Password=Aysel123";
            //string connectionString = "Host=10.0.100.240;Port=5432;Database=dialer;Username=postgres;Password=postgres";

            var con = new NpgsqlConnection(connectionString);
            lock (_dbLock)
            {
                try
                {
                    con.Open();
                    var cmd = con.CreateCommand();
                    cmd.CommandText = "INSERT INTO FILEINFO (CallInfo, Part1,Part2,DateTimeStr,FileName,CallID,FilePath,FolderPath) VALUES (@CallInfo, @Part1,@Part2,@DateTimeStr,@FileName,@CallID,@FilePath,@FolderPath)";
                    cmd.Parameters.AddWithValue("@CallInfo", file.CallInfo);
                    cmd.Parameters.AddWithValue("@Part1", file.Part1);
                    cmd.Parameters.AddWithValue("@Part2", file.Part2);
                    cmd.Parameters.AddWithValue("@DateTimeStr", file.DateTimeStr);
                    cmd.Parameters.AddWithValue("@FileName", file.FileName);
                    cmd.Parameters.AddWithValue("@CallID", file.CallID);
                    cmd.Parameters.AddWithValue("@FilePath", file.FilePath);
                    cmd.Parameters.AddWithValue("@FolderPath", file.FolderPath);
                    cmd.ExecuteNonQuery();
                    con.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    if (con.State == ConnectionState.Open)
                    {
                        con.Close();
                    }
                    return false;
                }
            }
        }
    }
}
