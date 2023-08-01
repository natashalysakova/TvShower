using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;



namespace TVShower
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<string> PreviewFilenames
        {
            get;
        }

        private const string directory = @"D:\Tv Shows";
        Dictionary<string, string> RenameList = new Dictionary<string, string>();


        private string _folderPath;
        public string FolderPath
        {
            get { return _folderPath; }
            set { _folderPath = value; NotifyPropertyChanged(nameof(FolderPath)); }
        }

        private string _pattern = "{title} - s{season}e{episode}{ext}";
        public string Pattern
        {
            get { return _pattern; }
            set { _pattern = value; NotifyPropertyChanged(nameof(Pattern)); }
        }

        private string _subtitlePattern = "{title} - s{season}e{episode}.{lang}{ext}";
        public string SubtitlePattern
        {
            get { return _subtitlePattern; }
            set { _subtitlePattern = value; NotifyPropertyChanged(nameof(SubtitlePattern)); }
        }

        private string _seasonNumber;
        public string Season
        {
            get { return _seasonNumber; }
            set { _seasonNumber = value; NotifyPropertyChanged(nameof(Season)); }
        }

        private string _subtitleLanguage;
        public string SubtitleLanguage
        {
            get { return _subtitleLanguage; }
            set { _subtitleLanguage = value; NotifyPropertyChanged(nameof(SubtitleLanguage)); }
        }

        private string _showId;
        public string ShowId
        {
            get { return _showId; }
            set { _showId = value; NotifyPropertyChanged(nameof(ShowId)); }
        }

        private string _showTitle;
        public string ShowTitle
        {
            get { return _showTitle; }
            set { _showTitle = value; NotifyPropertyChanged(nameof(ShowTitle)); }
        }

        private bool _isFullShow;

        public bool IsFullShow
        {
            get { return _isFullShow; }
            set { _isFullShow = value; NotifyPropertyChanged(nameof(IsFullShow)); NotifyPropertyChanged(nameof(IsSingleSeason)); }
        }

        public bool IsSingleSeason
        {
            get { return !_isFullShow; }
            set { _isFullShow = !value; NotifyPropertyChanged(nameof(IsSingleSeason)); }
        }

        private bool _addShowTitle;
        public bool AddShowTitle
        {
            get { return _addShowTitle; }
            set { _addShowTitle = value; NotifyPropertyChanged(nameof(AddShowTitle)); }
        }

        public MainWindow()
        {
            PreviewFilenames = new ObservableCollection<string>();

            InitializeComponent();
            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.DefaultDirectory = directory;
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if(result == CommonFileDialogResult.Ok)
            {
                FolderPath = dialog.FileName;

                AnalyzeFolder();
            }
        }

        private void PrepareAnalysis()
        {
            RenameList.Clear();
            PreviewFilenames.Clear();

        }

        private void FastAnalyzeFolder()
        {
            PrepareAnalysis();

            DirectoryInfo info = new DirectoryInfo(FolderPath);
            var subdirectories = info.GetDirectories();
            if (subdirectories.Length > 1)
            {
                PreviewFilenames.Add(info.Name);
                RenameFullShow(subdirectories);
            }
            else
            {
                int seasonInt;
                if (!int.TryParse(Season, out seasonInt))
                {
                    seasonInt = 1;
                }

                RenameSeason(seasonInt, info);
            }
        }

        private void AnalyzeFolder()
        {
            PrepareAnalysis();

            DirectoryInfo info = new DirectoryInfo(FolderPath);
            var subdirectories = info.GetDirectories();
            if (subdirectories.Length > 0)
            {
                IsFullShow = true;
                ShowTitle = info.Name;
                AddShowTitle = false;

                PreviewFilenames.Add(info.Name);
                RenameFullShow(subdirectories);

            }
            else
            {
                IsSingleSeason = true;
                AddShowTitle = true;

                var splittedPath = info.FullName.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                ShowTitle = splittedPath[splittedPath.Length - 1];


                Season = Regex.Match(info.Name, @"\d+").Value;
                int seasonInt;
                if (!int.TryParse(Season, out seasonInt))
                {
                    seasonInt = 1;
                }

                RenameSeason(seasonInt, info);
            }

        }

        private void RenameFullShow(DirectoryInfo[] subdirectories)
        {
            var season = 1;
            foreach (var item in subdirectories.OrderBy(x => x.Name))
            {
                RenameSeason(season, item);
                season++;
            }
        }

        private void RenameSeason(int season, DirectoryInfo item)
        {
            var folder = RenameFolder(item.Name, season);
            RenameList.Add(item.FullName, Path.Combine(item.Parent.FullName, folder));

            var files = item.GetFiles();

            var videos = files.OrderBy(x => x.Name).Where(x => IsVideo(x.Extension));
            var subtitles = files.OrderBy(x => x.Name).Where(x => IsSubtitles(x.Extension));
            var episode = 1;
            foreach (var file in videos)
            {
                var newName = RenameFile(ShowTitle, file.Name, file.Extension, season, episode);
                RenameList.Add(file.FullName, Path.Combine(item.FullName, newName));

                episode++;
            }

            episode = 1;
            foreach (var file in subtitles)
            {
                var newName = RenameSubtitle(ShowTitle, file.Name, file.Extension, SubtitleLanguage, season, episode);
                RenameList.Add(file.FullName, Path.Combine(item.FullName, newName));
                episode++;
            }

        }
        private bool IsSubtitles(string extension)
        {
            var extLow = extension.ToLower();
            var exlList = new List<string>()
            {
                ".srt",
                ".ass"
            };

            return exlList.Contains(extLow);
        }

        private bool IsVideo(string extension)
        {
            var extLow = extension.ToLower();
            var exlList = new List<string>()
            {
                ".avi",
                ".mxf",
                ".mkv",
                ".mp4"
            };

            return exlList.Contains(extLow);
        }

        private string RenameFolder(string name, int season)
        {
            string newFolderName = $"Season {season.ToString("00")}";

            if (AddShowTitle)
            {
                newFolderName = $"{ShowTitle} - {newFolderName}";
            }

            PreviewFilenames.Add($"{name} -> {newFolderName}");
            return newFolderName;
        }

        private string RenameFile(string name, string oldName, string extention, int season, int episode)
        {
            var newFileName = Pattern.Replace("{title}", name)
                .Replace("{season}", season.ToString("00"))
                .Replace("{episode}", episode.ToString("00"))
                .Replace("{ext}", extention);
            PreviewFilenames.Add($"{oldName} -> {newFileName}");

            return newFileName;
        }

        private string RenameSubtitle(string name, string oldName, string extention, string language, int season, int episode)
        {
            var newFileName = SubtitlePattern.Replace("{title}", name)
                .Replace("{season}", season.ToString("00"))
                .Replace("{episode}", episode.ToString("00"))
                .Replace("{lang}", language)
                .Replace("{ext}", extention);
            PreviewFilenames.Add($"{oldName} -> {newFileName}");

            return newFileName;
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var path in RenameList)
            {
                FileAttributes attr = File.GetAttributes(path.Key);

                if (attr.HasFlag(FileAttributes.Directory))
                    continue;
                else
                    File.Move(path.Key, path.Value);
            }

            foreach (var path in RenameList)
            {
                if (path.Key == path.Value)
                    continue;

                if (Directory.Exists(path.Key) || File.Exists(path.Key))
                {
                    FileAttributes attr = File.GetAttributes(path.Key);

                    if (attr.HasFlag(FileAttributes.Directory))
                        Directory.Move(path.Key, path.Value);

                }


            }

            PreviewFilenames.Clear();
            PreviewFilenames.Add("Done");
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            MovieDbApi.MovieDbProvider provider = new MovieDbApi.MovieDbProvider();

            if (!string.IsNullOrEmpty(ShowId))
            {
                int showIdInt;
                if (int.TryParse(ShowId, out showIdInt))
                {
                    ShowTitle = provider.GetTvShow(showIdInt);
                }
            }
            else if (!string.IsNullOrEmpty(ShowTitle))
            {
                ShowId = provider.SearchTvShow(ShowTitle).ToString();
            }
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            FastAnalyzeFolder();
        }
    }
}
