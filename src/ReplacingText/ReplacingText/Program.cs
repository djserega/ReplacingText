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

        private const string _statusText = "Обработка строки: ";
        private const string _replacedText = "Выполнено замен: ";

        private static int _replacedCount = 0;
        private static int _processedLine = 0;
        private static DateTime _startTime;

        private const int _processedInWork = 100;
        private const int _flushCoutn = 10000;

        #endregion

        static void Main(string[] args)
        {
            InitReplacesText();

            Console.WriteLine("Укажите путь к файлу:");
            string path = Console.ReadLine();

            Console.WriteLine();

            FileInfo originalData = new(path.Trim('"'));

            if (!originalData.Exists)
            {
                Console.WriteLine($"Не удалось получить доступ к файлу:\n{originalData.FullName}\n");
                Console.WriteLine("Для выхода из приложения нажмите любую клавишу...");
                Console.ReadKey();
                return;
            }

            _startTime = DateTime.Now;
            Console.WriteLine($"Начало: {_startTime::HH:mm:ss}\n");

            using StreamReader reader = new(originalData.OpenRead());
            using StreamWriter writer = new("Result.txt");

            string rows = "";

            rows = ReadManyRows(reader, rows, _processedInWork);

            rows = ProcessedFile(reader, writer, rows);

            Console.WriteLine($"\n\nЗавершение: {DateTime.Now::HH:mm:ss}");

            reader.Close();
            reader.Dispose();

            writer.Flush();
            writer.Close();
            writer.Dispose();

            Console.WriteLine("\n\nДанные обработаны\n");

            Console.WriteLine("Файл результата:");
            Console.WriteLine(new FileInfo("Result.txt").FullName);
            Console.WriteLine();

            Console.WriteLine("Для выхода из приложения нажмите любую клавишу...");
            Console.ReadKey();
        }

        private static string ProcessedFile(StreamReader reader, StreamWriter writer, string rows)
        {
            do
            {
                foreach (KeyValuePair<string, string> itemReplace in _dataReplace)
                {
                    if (rows.IndexOf(itemReplace.Key) > 0)
                    {
                        rows = rows.Replace(itemReplace.Key, itemReplace.Value);
                        _replacedCount++;
                    }
                }

                WriteStatus();

                int countRowsToSave = rows.Count(ch => ch == '\n') / 2;

                for (int iRow = 0; iRow < countRowsToSave; iRow++)
                {
                    if (string.IsNullOrWhiteSpace(rows))
                        break;
                    else
                    {
                        int idFirstLine = rows.IndexOf("\n");
                        writer.WriteLine(rows[..idFirstLine]);

                        rows = rows[(idFirstLine + 1)..];
                    }
                }

                countRowsToSave = Math.Max(countRowsToSave, _processedInWork / 2);

                rows = ReadManyRows(reader, rows, countRowsToSave);

                if (_processedLine % _flushCoutn == 0)
                    writer.Flush();

            } while (!reader.EndOfStream);
            return rows;
        }

        private static string ReadManyRows(StreamReader reader, string rows, int countRow)
        {
            for (int i = 0; i < countRow; i++)
            {
                rows += $"{reader.ReadLine()}\n";
                _processedLine++;

                WriteStatus();

                if (reader.EndOfStream)
                    break;
            }

            return rows;
        }

        private static void WriteStatus()
        {
            if (_processedLine % 1000 == 0)
                Console.Write($"\r{DateTime.Now - _startTime:dd\\:hh\\:mm\\:ss}" +
                    $"   -   {_statusText}{_processedLine}" +
                    $"   -   {_replacedText}{_replacedCount}");
        }

        private static void InitReplacesText()
        {
            FileInfo info = new(@"Text.txt");
            using StreamReader stream = new(info.OpenRead());

            string originalData = "";
            string tempRow = "";
            string row;

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
                            _dataReplace.Add(originalData, tempRow);
                    }
                    else
                        tempRow += $"{row}\n";
                }

            } while (row != null);
        }
    }
}
