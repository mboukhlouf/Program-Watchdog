using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Program_Watchdog.Controls
{
    public class LogBox : TextBox
    {
        public static readonly DependencyProperty MaxLogsProperty = DependencyProperty.Register("MaxLogs", typeof(int), typeof(LogBox), new UIPropertyMetadata(500));

        public int MaxLogs
        {
            get => (int)GetValue(MaxLogsProperty);
            set => SetValue(MaxLogsProperty, value);
        }

        public LogBox() : base()
        {
            IsReadOnly = true;
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            TextWrapping = TextWrapping.Wrap;
            AcceptsReturn = true;
        }

        public void Log(String message)
        {
            if (message == null)
                return;

            message = message.Replace("\n", " ");
            if (Text.Length != 0)
                AppendText("\n");
            AppendText(message);

            int lineCount = Text.Sum((c) => c == '\n' ? 1 : 0) + 1;
            if (lineCount > MaxLogs)
            {
                Text = Text.Substring(Text.IndexOf('\n') + 1);
            }
        }
    }
}
