using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReplacingText
{
    internal class Program
    {
        private readonly static Dictionary<string, string> _dataReplace = new();

        #region Fields

        private const string _start = "@@@";
        private const string _middle = "###";
        private const string _end = "$$$";

        private const string _tempOriginal = "<original>";
        private const string _tempNew = "<new>";

        private const string _statusText = "String Processing: ";
        private const string _replacedText = "Replacements completed: ";

        private static int _replacedCount = 0;
        private static int _replacedCountPart = 0;
        private static long _processedLine = 0;
        private static long _processedLinePart = 0;
        private static DateTime _startTime;

        private const int _processedInWork = 100;
        private const int _flushCount = 10000;

        #endregion

        static void Main(string[] args)
        {
            Logger.Inf("Starting");

            InitReplacesText();

            Console.WriteLine("Specify the path to the file or directory:");
            string path = Console.ReadLine().Trim('"');

            Console.WriteLine();

            if (File.Exists(path) || Directory.Exists(path))
            {
                FileAttributes filePath = File.GetAttributes(path);
                if (filePath.HasFlag(FileAttributes.Directory))
                {
                    Logger.Inf($"Processing directory {path}");

                    DirectoryInfo directory = new(path);

                    foreach (FileInfo itemFile in directory.GetFiles())
                    {
                        if (!itemFile.Name.EndsWith($"_processed{itemFile.Extension}"))
                        {
                            Console.WriteLine($"Processing file: {itemFile.Name}\n");

                            _replacedCountPart = 0;
                            ReplacingFile(itemFile);
                            _replacedCount += _replacedCountPart;

                        }
                    }

                    Logger.Inf($"Directory processed. {_replacedText}{_replacedCount}");
                }
                else
                    ReplacingFile(new(path));
            }
            else
            {
                Console.WriteLine("File or directory does not exist");
                Console.WriteLine();
            }

            Console.WriteLine("To quit the App press any keyboard key...");
            Console.ReadKey();
        }

        private static void ReplacingFile(FileInfo originalData)
        {
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

            Logger.Inf("Processed");
        }

        private static void ProcessedFile(StreamReader reader, StreamWriter writer, string rows)
        {
            do
            {
                foreach (KeyValuePair<string, string> itemReplace in _dataReplace)
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

            WriteStatus(true);
        }

        private static string ReadManyRows(StreamReader reader, string rows, int countRow)
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

        private static void WriteStatus(bool showCurrentStatus = false)
        {
            if (_processedLinePart % 1000 == 0 || showCurrentStatus)
                Console.Write($"\r{DateTime.Now - _startTime:dd\\:hh\\:mm\\:ss}" +
                    $"   -   {_statusText}{_processedLinePart}" +
                    $"   -   {_replacedText}{_replacedCountPart}");
        }

        private static void InitReplacesText()
        {
            FileInfo info = new(@"Text.txt");
            using StreamReader stream = new(info.OpenRead());

            string originalData = "";
            string tempRow = "";
            string row;

            int countReplacingTemplate = 0;

            do
            {
                row = stream.ReadLine();

                if (!string.IsNullOrWhiteSpace(row))
                {
                    if (row.Equals(_start))
                    {
                        originalData = "";
                        tempRow = "";
                    }
                    else if (row.Equals(_middle))
                    {
                        originalData = tempRow.Trim();
                        tempRow = "";
                    }
                    else if (row.Equals(_end))
                    {
                        tempRow = tempRow.Trim();

                        if (!(originalData.Equals(_tempOriginal) || tempRow.Equals(_tempNew)))
                        {
                            if (!_dataReplace.ContainsKey(originalData))
                            {
                                _dataReplace.Add(originalData, tempRow);
                                countReplacingTemplate++;
                            }
                        }
                    }
                    else
                        tempRow += $"{row}\n";
                }

            } while (row != null);

            Logger.Inf($"Added replacement templates: {countReplacingTemplate}");
        }
    }
}
