using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Diagnostics;

namespace ValCacheProxyHost
{
    public class UILogListener : TraceListener
    {
        private TextBox output;

        public UILogListener(TextBox output)
        {
            this.Name = "Trace";
            this.output = output;
        }


        public override void Write(string message)
        {
            Action append = delegate () {
                output.AppendText(string.Format("[{0}] ", DateTime.Now.ToString()));
                output.AppendText(message);
            };
            if (App.Current.Dispatcher.Thread == System.Threading.Thread.CurrentThread)
            {
                append();
            }
            else
            {
                App.Current.Dispatcher.BeginInvoke(append);
            }
        }

        public override void WriteLine(string message)
        {
            Write(message + Environment.NewLine);
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string LOG_DIR = @".\";
        public const string LOG_NAME = "log.txt";

        public MainWindow()
        {
            InitializeComponent();

            TraceListener logListener = new UILogListener(LogView);
            Trace.Listeners.Add(logListener);

            RefreshCachedFilesList();

            // Setup folder watcher for auto updating cached files list
            FileSystemWatcher cacheWatcher = new FileSystemWatcher();
            cacheWatcher.Path = ValCacheProxyLib.FileServerProxyService.CACHE_DIR;
            cacheWatcher.NotifyFilter = NotifyFilters.LastAccess
                                      | NotifyFilters.LastWrite
                                      | NotifyFilters.FileName
                                      | NotifyFilters.DirectoryName;
            cacheWatcher.Filter = "*.*";

            // Add event handlers.
            cacheWatcher.Changed += OnCacheFolderChanged;
            cacheWatcher.Created += OnCacheFolderChanged;
            cacheWatcher.Deleted += OnCacheFolderChanged;
            cacheWatcher.Renamed += OnCacheFolderChanged;

            cacheWatcher.EnableRaisingEvents = true;
        }

        // Fired when cache is modified
        private void OnCacheFolderChanged(object source, FileSystemEventArgs e)
        {
            Trace.TraceInformation("Cache modification detected");
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                RefreshCachedFilesList();
            });
        }

        // Refresh locally cached files list
        private void RefreshCachedFilesList()
        {
            CachedFilesList.Items.Clear();
            DirectoryInfo d = new DirectoryInfo(ValCacheProxyLib.FileServerProxyService.CACHE_DIR);
            FileInfo[] files = d.GetFiles("*.*");
            foreach (FileInfo file in files)
            {
                CachedFilesList.Items.Add(file.Name);
            }
        }

        // Clear cache button handler
        private void ClearCacheBtn_Click(object sender, RoutedEventArgs e)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(ValCacheProxyLib.FileServerProxyService.CACHE_DIR);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

            Trace.TraceInformation("Cache cleared");
        }
    }
}
