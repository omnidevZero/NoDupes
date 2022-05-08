using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using NoDupes.Utils;
using NoDupes.Data;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace NoDupes
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            dialog.RootFolder = System.Environment.SpecialFolder.Desktop;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CancelButton.IsEnabled = true;
                SelectedFolder.Text = dialog.SelectedPath;
                var files = (bool)RecursiveCheckbox.IsChecked ? Directory.EnumerateFiles(dialog.SelectedPath, "*.*", SearchOption.AllDirectories) : Directory.GetFiles(dialog.SelectedPath);
                //var duplicatesData = new List<ListData>();
                ObservableCollection<ListData> duplicatesData = new ObservableCollection<ListData>();
                FileList.ItemsSource = duplicatesData;

                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.Maximum = files.Count();
                var fullData = await GetFilesDataAsync(files);
                CheckForDuplicates(fullData, duplicatesData);
            }
        }

        private async Task<List<ListData>> GetFilesDataAsync(IEnumerable<string> files)
        {
            Threading.CancellationTokenSource = new CancellationTokenSource();
            Threading.CancellationToken = Threading.CancellationTokenSource.Token;
            var result = await Task.Run(() => GetFilesData(files), Threading.CancellationToken);
            return result;
        }

        private List<ListData> GetFilesData(IEnumerable<string> files)
        {
            var data = new List<ListData>();
            var loggingStringStorage = new string[5];

            foreach (var file in files)
            {
                if (Threading.CancellationToken.IsCancellationRequested)
                {
                    break;
                }

                data.Add(new ListData { MD5 = Helpers.GetMD5(file), Path = file });
                Dispatcher.Invoke(new System.Action(() =>
                {
                    ProgressBar.Value++;
                    System.Array.Copy(loggingStringStorage, 0, loggingStringStorage, 1, loggingStringStorage.Length - 1);
                    loggingStringStorage[0] = Logger.Log($"Found {file}");
                    StatusText.Text = string.Join("", loggingStringStorage).Trim();
                }));
            }

            Dispatcher.Invoke(new System.Action(() =>
            {
                ProgressBar.Value = 0;
                ProgressBar.Visibility = Visibility.Hidden;
                AddToStatusText("Finished scanning.");


            }));
            return data;
        }

        private void CheckForDuplicates(List<ListData> listToCheck, ObservableCollection<ListData> listToFillWithDuplicates)
        {
            for (var i = 0; i < listToCheck.Count; i++)
            {
                var hasMatchingData = false;
                var data = listToCheck[i];
                for (var j = 0; j < listToCheck.Count; j++)
                {
                    var item = listToCheck[j];

                    if (hasMatchingData)
                    {
                        break;
                    }
                    else if (i == j)
                    {
                        continue;
                    }
                    else if (data.MD5 == item.MD5)
                    {
                        hasMatchingData = true;
                        listToFillWithDuplicates.Add(new ListData { MD5 = data.MD5, Path = data.Path, MatchingPath = item.Path });
                    }
                }
            }

            CancelButton.IsEnabled = false;

            if (listToCheck.Count == 0)
            {
                AddToStatusText("No files found in current directory.");
            }
            else if (listToFillWithDuplicates.Count > 0)
            {
                AddToStatusText($"Found {listToFillWithDuplicates.Count} duplicates.");
                DeleteDuplicatesButton.IsEnabled = true;
            }
            else
            {
                AddToStatusText("No duplicates found.");
            }
        }

        private void AddToStatusText(string text)
        {
            StatusText.Text = StatusText.Text.Insert(0, Logger.Log(text)).Trim();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Threading.CancellationTokenSource.Cancel();
        }

        private void DeleteDuplicatesButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Delete action.");
        }
    }
}
