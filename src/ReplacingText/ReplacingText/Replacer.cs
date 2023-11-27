using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReplacingText
{
    internal class Replacer
    {
 
        private const string _statusText = "String Processing: ";
        private const string _replacedText = "Replacements completed: ";

        private int _replacedCount = 0;
        private int _replacedCountPart = 0;
        private long _processedLine = 0;
        private long _processedLinePart = 0;
        private DateTime _startTime;

        private const int _processedInWork = 100;
        private const int _flushCount = 10000;

        internal Dictionary<string, string> DataReplace = new();
        internal bool DeletingSource { get; set; }
        internal bool MultipleProcessing { get; set; }

        EventHandler<Models.ReplaceStatus> _statusFiles = default;
        EventHandler<Models.ReplaceStatus> _statusFile = default;


        internal void ProcessingData(string path = "", EventHandler<Models.ReplaceStatus>? statusFiles = default, EventHandler<Models.ReplaceStatus>? statusFile = default)
        {
            _statusFiles = statusFiles;
            _statusFile = statusFile;

            Console.WriteLine("Specify the path to the file or directory:");
            if (string.IsNullOrEmpty(path))
                path = Console.ReadLine().Trim('"');

            Console.WriteLine();

            if (File.Exists(path) || Directory.Exists(path))
            {
                FileAttributes filePath = File.GetAttributes(path);
                if (filePath.HasFlag(FileAttributes.Directory))
                {
                    _statusFile?.Invoke(this, new($"Processing directory {path}"));

                    Logger.Inf($"Processing directory {path}");

                    DirectoryInfo directory = new(path);

                    FileInfo[] files = directory.GetFiles();

                    int countFiles = files.Length;
                    int currentFile = 0;
                    foreach (FileInfo itemFile in files)
                    {
                        currentFile++;
                        _statusFiles?.Invoke(this, new(currentFile  * 100 / countFiles));
                     
                        if (!itemFile.Name.EndsWith($"_processed{itemFile.Extension}"))
                        {
                            Console.WriteLine($"Processing file: {itemFile.Name}\n");

                            _replacedCountPart = 0;
                            ReplacingFile(itemFile);
                            _replacedCount += _replacedCountPart;
                        }
                    }
                    _statusFiles?.Invoke(this, new(100));
                    _statusFile?.Invoke(this, new($"Directory processed. {_replacedText}{_replacedCount}"));

                    Logger.Inf($"Directory processed. {_replacedText}{_replacedCount}");
                }
                else
                {
                    _statusFiles?.Invoke(this, new(100));

                    ReplacingFile(new(path));
                }
            }
            else
            {
                string message = "File or directory does not exist";

                if (statusFile != default)
                {
                    statusFile.Invoke(this, new Models.ReplaceStatus(message));
                }
                else
                {
                    Console.WriteLine(message);
                    Console.WriteLine();
                }
            }
        }

        private void ReplacingFile(FileInfo originalData)
        {
            _statusFile?.Invoke(this, new($"Processing file: {originalData.Name}"));
            
            Logger.Inf($"Processing file {originalData.FullName}");

            if (!originalData.Exists)
            {
                Console.WriteLine($"Failed to get the access to the file:\n{originalData.FullName}\n");
                return;
            }

            _startTime = DateTime.Now;

            Console.WriteLine($"Beginning: {_startTime:HH:mm:ss}\n");

            string pathResult = $"{originalData.FullName}_processed{originalData.Extension}";
            using StreamReader reader = new(originalData.OpenRead());
            using StreamWriter writer = new(pathResult);

            string rows = "";

            _processedLinePart = 0;

            rows = ReadManyRows(reader, rows, _processedInWork);

            ProcessedFile(reader, writer, rows);

            _processedLine += _processedLinePart;

            Console.WriteLine($"\n\nEnding: {DateTime.Now:HH:mm:ss}");

            reader.Close();
            reader.Dispose();

            writer.Flush();
            writer.Close();
            writer.Dispose();

            Console.WriteLine("\nData processed\n");

            Console.WriteLine("Results file:");
            Console.WriteLine(pathResult);
            Console.WriteLine();

            if (DeletingSource)
            {

                try
                {
                    FileInfo fileInfoSource = new(originalData.FullName);
                    fileInfoSource.Delete();

                    Console.WriteLine("Original file is deleted");
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERR: Could not deleting source file: {originalData.Name}");
                    Console.WriteLine();
                }
            }

            _statusFile?.Invoke(this, new($"Processing is complete: {originalData.Name}"));
            Logger.Inf("Processed");
        }

        private void ProcessedFile(StreamReader reader, StreamWriter writer, string rows)
        {
            long length = reader.BaseStream.Length;

            do
            {
                long position = reader.BaseStream.Position;
                int percent = (int)(position * 100 / length);

                if (_statusFile != default)
                    _statusFile.Invoke(this, new(percent));

                foreach (KeyValuePair<string, string> itemReplace in DataReplace)
                {
                    if (rows.IndexOf(itemReplace.Key) > 0)
                    {
                        rows = rows.Replace(itemReplace.Key, itemReplace.Value);
                        _replacedCountPart++;
                    }
                }

                WriteStatus();

                int countRowsToSave = rows.Count(ch => ch == '\n') / 2;

                for (int iRow = 0; iRow < countRowsToSave; iRow++)
                {
                    int idFirstLine = rows.IndexOf("\n");
                    writer.WriteLine(rows[..idFirstLine]);

                    rows = rows[(idFirstLine + 1)..];
                }

                countRowsToSave = Math.Max(countRowsToSave, _processedInWork / 2);

                rows = ReadManyRows(reader, rows, countRowsToSave);

                if (_processedLinePart % _flushCount == 0)
                    writer.Flush();

            } while (!reader.EndOfStream);

            writer.WriteLine(rows);
           
            if (_statusFile != default)
                _statusFile.Invoke(this, new(100));

            WriteStatus(true);
        }

        private string ReadManyRows(StreamReader reader, string rows, int countRow)
        {
            for (int i = 0; i < countRow; i++)
            {
                rows += $"{reader.ReadLine()}\n";
                _processedLinePart++;

                WriteStatus();

                if (reader.EndOfStream)
                    break;
            }

            return rows;
        }

        private void WriteStatus(bool showCurrentStatus = false)
        {
            if (_processedLinePart % 1000 == 0 || showCurrentStatus)
                Console.Write($"\r{DateTime.Now - _startTime:dd\\:hh\\:mm\\:ss}" +
                    $"   -   {_statusText}{_processedLinePart}" +
                    $"   -   {_replacedText}{_replacedCountPart}");
        }

    }
}
