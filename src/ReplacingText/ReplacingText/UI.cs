using Terminal.Gui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ReplacingText
{
    internal class UI
    {
        private event EventHandler<Models.ReplaceStatus> _statusEventFiles;
        private event EventHandler<Models.ReplaceStatus> _statusEventFile;

        private Window _mainWindow;

        private TextField _textFieldPath;
        private ProgressBar _progressBarFiles;
        private ProgressBar _progressBarFile;
        private ListView _listMessage;
        private List<string> _listMessageItems = new();

        internal UI(Replacer replaser)
        {
            Replacer = replaser;

            Application.Init();

            Colors.Base.Normal = Application.Driver.MakeAttribute(Color.Gray, Color.Black);

            Toplevel topLevelApp = Application.Top;
            topLevelApp.Add(_mainWindow);

            CreateMainWindow();

            Application.Top.Add(CreateMainMenu(), _mainWindow);

            CreateEnterPath();

            CreateStartProgressBar(replaser);

            CreateListMessage();

            Application.Run();
            Application.Shutdown();
        }

        internal Replacer Replacer { get; init; }



        private MenuBar CreateMainMenu()
        {
            return new(new MenuBarItem[]
            {
                new MenuBarItem("File", new MenuItem[]
                {
                    new MenuItem("Quit", "", () =>
                    {
                        Application.RequestStop ();
                    }, shortcut: Key.Q | Key.q)
                }),
                new MenuBarItem("Help", new MenuItem[]
                {
                    new MenuItem("About", "", () =>
                    {
                        Window  aboutView = new()
                        {
                            X = Pos.Center(),
                            Y = Pos.Center(),
                            Width = 50,
                            Height = 7,
                        };
                        _mainWindow.Add(aboutView);

                        Label username = new("https://github.com/djserega/ReplacingText")
                        {
                            X = Pos.Center(),
                            Y = Pos.Center() + 1,
                        };
                        aboutView.Add (username);

                        Button buttonClose = new ("X");
                        buttonClose.Clicked += () => { _mainWindow.Remove(aboutView); };

                        buttonClose.X = aboutView.Frame.Width - 8;
                        buttonClose.Y = 0;

                        aboutView.Add(buttonClose);
                    })
                })
            });
        }

        private void CreateMainWindow()
        {
            _mainWindow = new("Replacing text from the file (directory)")
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill() - 1
            };
        }

        private void CreateEnterPath()
        {
            int yPosition = 1;

            Label label = new("File or directory:")
            {
                X = 1,
                Y = yPosition
            };
            _textFieldPath = new("")
            {
                X = Pos.Right(label) + 1,
                Y = yPosition,
                Width = _mainWindow.Width - 1,
                Text = ""
            };
            //_textFieldPath.Width = _mainWindow.Width - 1;

            _mainWindow.Add(label, _textFieldPath);
        }

        private void CreateStartProgressBar(Replacer replaser)
        {
            Button buttonStart = new(5, 7, "Start")
            {
                Height = 1
            };
            buttonStart.Clicked += () =>
            {
                replaser.ProcessingData(_textFieldPath.Text.ToString(), _statusEventFiles, _statusEventFile);
            };

            Label labelFiles = new("Directory")
            {
                X = Pos.Right(buttonStart) + 6,
                Y = 6,
            };
            Label labelFile = new("File")
            {
                X = Pos.Right(buttonStart) + 6,
                Y = 8,
            };
            
            _progressBarFiles = new()
            {
                X = Pos.Right(buttonStart) + 16,
                Y = 6,
                Width = _mainWindow.Width - 35,
                Height = 1,
                SegmentCharacter = '█',
                ProgressBarFormat = ProgressBarFormat.SimplePlusPercentage
            };
  
            _progressBarFile = new()
            {
                X = Pos.Right(buttonStart) + 16,
                Y = 8,
                Width = _mainWindow.Width - 35,
                Height = 1,
                SegmentCharacter = '█',
                ProgressBarFormat = ProgressBarFormat.SimplePlusPercentage
            };


            _mainWindow.Add(buttonStart, _progressBarFiles, _progressBarFile, labelFiles, labelFile);
        }


        private void CreateListMessage()
        {
            Label label = new("Messages:")
            {
                X = 1,
                Y = 11
            };

            _listMessage = new(_listMessageItems)
            {
                X = Pos.Right(label) + 1,
                Y = 11,
                Width = _mainWindow.Width - 1,
                //Height = 2
            };

            _mainWindow.Add(label, _listMessage);

            _statusEventFile += (object sender, Models.ReplaceStatus e) =>
            {
                if (string.IsNullOrEmpty(e.Message))
                {
                    _progressBarFile.Fraction = (float)e.Percent / 100;
                    Application.Refresh();
                }
                else
                {
                    _listMessageItems.Insert(0, e.Message);
                }

                for (int i = 5; i < _listMessageItems.Count; i++)
                    _listMessageItems.RemoveAt(5);

                _listMessage.Height = _listMessageItems.Count;
                _listMessage.SetSource(_listMessageItems);
            };

            _statusEventFiles += (object sender, Models.ReplaceStatus e) =>
            {
                if (string.IsNullOrEmpty(e.Message))
                {
                    _progressBarFiles.Fraction = (float)e.Percent / 100;
                    Application.Refresh();
                }
                else
                {
                    _listMessageItems.Insert(0, e.Message);
                }

                for (int i = 5; i < _listMessageItems.Count; i++)
                    _listMessageItems.RemoveAt(5);

                _listMessage.Height = _listMessageItems.Count;
                _listMessage.SetSource(_listMessageItems);
            };
        }
    }
}
