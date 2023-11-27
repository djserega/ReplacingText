using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terminal.Gui;

namespace ReplacingText
{
    internal class Program
    {

        #region Fields

        private const string _start = "@@@";
        private const string _middle = "###";
        private const string _end = "$$$";

        #endregion

        static void Main(string[] args)
        {
            Replacer replaser = new();

            Init(args, replaser);

            Logger.Inf("Starting");

            InitReplacesText(replaser);

            if (!args.Contains("--noui"))
            {
                _ = new UI(replaser);
            }
            else
            {
                if (replaser.MultipleProcessing)
                {
                    ConsoleKey userAnswer;
                    do
                    {
                        replaser.ProcessingData();

                        Console.WriteLine("\nNext process - press 'y'.\nPress any key to break processing");

                        userAnswer = Console.ReadKey().Key;

                        Console.WriteLine();

                    } while (userAnswer == ConsoleKey.Y);
                }
                else
                    replaser.ProcessingData();

                Console.WriteLine("Press any key to close this window...");
                Console.ReadKey();
            }
        }

        private static void Init(string[] args, Replacer replaser)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Special args:\n" +
                    "--delete source : deleting the source file after ends of process\n" +
                    "--multiple : do not close app after processing data");
                Console.WriteLine();
            }
            else
            {
                foreach (string item in args)
                {
                    if (item == "--delete source")
                    {
                        Console.WriteLine("INF: Deleting source is enabled");
                        replaser.DeletingSource = true;
                    }
                    else if (item == "--multiple")
                    {
                        Console.WriteLine("INF: Multiple processing is enabled");
                        replaser.MultipleProcessing = true;
                    }
                }

                Console.WriteLine();
            }

        }
        
        private static void InitReplacesText(Replacer replaser)
        {
            FileInfo info = new(@"Text.txt");
            using StreamReader stream = new(info.OpenRead());

            string originalData = "";
            string tempRow = "";
            string row;

            int countReplacingTemplate = 0;

            string _tempOriginal = "<original>";
            string _tempNew = "<new>";
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
                            if (!replaser.DataReplace.ContainsKey(originalData))
                            {
                                replaser.DataReplace.Add(originalData, tempRow);
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
