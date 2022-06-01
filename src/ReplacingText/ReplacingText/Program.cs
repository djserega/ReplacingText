using System;
using System.Collections.Generic;
using System.IO;

namespace ReplacingText
{
    internal class Program
    {
        private readonly static Dictionary<string, string> _dataReplace = new();

        private const string _start = "@@@";
        private const string _middle = "###";
        private const string _end = "$$$";

        private const string _tempOriginal = "<original>";
        private const string _tempNew = "<new>";

        private const string _statusText = "Обработка строки: ";
        private const string _replacedText = "Выполнено замен: ";

        private static int _replacedCount = 0;
        private static int _processedLine = 0;

        static void Main(string[] args)
        {
            ReadTextToDataReplace();

            Console.WriteLine("Укажите путь к файлу:");
            string path = Console.ReadLine();

            Console.WriteLine();

            FileInfo originalData = new(path.Trim('"'));

            if (!originalData.Exists)
            {
                Console.WriteLine($"Не удалось получить доступ к файлу:\n{originalData.FullName}");
                Console.WriteLine();
                Console.WriteLine("Для выхода из приложения нажмите любую клавишу...");
                Console.ReadKey();
                return;
            }

            using StreamReader reader = new(originalData.OpenRead());

            using StreamWriter writer = new("Result.txt");

            string rows = "";

            rows = ReadManyRows(reader, rows, 10);

            do
            {
                rows += $"{reader.ReadLine()}\n";

                foreach (KeyValuePair<string, string> itemReplace in _dataReplace)
                {
                    if (rows.IndexOf(itemReplace.Key) > 0)
                    {
                        rows = rows.Replace(itemReplace.Key, itemReplace.Value);
                        _replacedCount++;

                        rows = ReadManyRows(reader, rows, 10);
                    }
                }

                _processedLine++;

                WriteStatus();

                int idFirstLine = rows.IndexOf("\n");
                writer.WriteLine(rows[..idFirstLine]);

                rows = rows[(idFirstLine + 1)..];

            } while (!reader.EndOfStream);


            reader.Close();
            reader.Dispose();

            writer.Flush();
            writer.Close();
            writer.Dispose();


            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Данные обработаны");
            Console.WriteLine();

            Console.WriteLine("Файл результата:");
            Console.WriteLine(new FileInfo("Result.txt").FullName);
            Console.WriteLine();

            Console.WriteLine("Для выхода из приложения нажмите любую клавишу...");
            Console.ReadKey();
        }

        private static string ReadManyRows(StreamReader reader, string rows, int countRow)
        {
            for (int i = 1; i < countRow; i++)
            {
                rows += $"{reader.ReadLine()}\n";

                WriteStatus();
            }

            return rows;
        }

        private static void WriteStatus()
        {
            if (_processedLine % 1000 == 0)
                Console.Write($"\r{_statusText}{_processedLine}   -   {_replacedText}{_replacedCount}");
        }

        private static void ReadTextToDataReplace()
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
