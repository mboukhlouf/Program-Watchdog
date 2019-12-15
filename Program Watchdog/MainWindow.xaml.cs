using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Program_Watchdog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Process process;
        private CancellationTokenSource restartCancellationTokenSource;

        public bool Running { get; private set; } = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonStart_OnClick(object sender, RoutedEventArgs e)
        {
            if (!Running)
            {
                String[] tokens = TextBoxProgram.Text
                    .Trim()
                    .Split(' ');

                if (tokens.Length > 0)
                {
                    bool parsingResult = int.TryParse(TextBoxRestartValue.Text, out int restartTime);
                    if (!parsingResult)
                    {
                        MessageBox.Show("The value of restart time should be numeric.", "Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    var programName = tokens[0];
                    var args = "";
                    if (tokens.Length >= 2)
                    {
                        args = tokens[1];
                    }
                    
                    StartProcess(programName, args);

                    TextBoxProgram.IsReadOnly = true;
                    TextBoxRestartValue.IsReadOnly = true;
                    ButtonStart.Content = "Stop";
                    Running = true;

                    if (restartTime > 0)
                    {
                        restartCancellationTokenSource = new CancellationTokenSource();
                        RestartProcess(restartTime * 60 * 1000);
                    }
                }
            }
            else
            {
                Running = false;
                restartCancellationTokenSource.Cancel();
                process.Kill();
                if (!process.HasExited)
                {
                    process.WaitForExit();
                }
                TextBoxProgram.IsReadOnly = false;
                TextBoxRestartValue.IsReadOnly = false;
                ButtonStart.Content = "Start";
            }
        }

        private void TextBoxProgram_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ButtonStart_OnClick(sender, e);
            }
        }

        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Dispatcher?.Invoke(() => LogBox.Log(e.Data));
        }

        private void ProcessOnExited(object sender, EventArgs e)
        {
            if (Running)
            {
                Dispatcher?.Invoke(() => LogBox.Log("Process exited. Restarting..."));
                StartProcess(process.StartInfo.FileName, process.StartInfo.Arguments);
            }
        }

        private void TextBoxInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var input = TextBoxInput.Text.Trim();
                if (input != "")
                {
                    process.StandardInput.WriteLine(input);
                    TextBoxInput.Clear();
                }
            }
        }

        private void StartProcess(String fileName, String arguments)
        {
            process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                },
                EnableRaisingEvents = true
            };
            process.OutputDataReceived += ProcessOnOutputDataReceived;
            process.ErrorDataReceived += ProcessOnOutputDataReceived;
            process.Exited += ProcessOnExited;

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }

        private void RestartProcess(int restartTime)
        {
            Task.Delay(restartTime, restartCancellationTokenSource.Token).ContinueWith(task =>
            {
                if (Running)
                {
                    Dispatcher?.Invoke(() =>
                    {
                        Running = false;
                        process.Kill();
                        if (!process.HasExited)
                        {
                            process.WaitForExit();
                        }
                        Dispatcher?.Invoke(() => LogBox.Log("Restarting..."));
                        StartProcess(process.StartInfo.FileName, process.StartInfo.Arguments);
                        Running = true;
                    });
                    RestartProcess(restartTime);
                }
            });
        }
    }
}
