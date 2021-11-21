using Microsoft.Win32;
using QuipuTest.Infrastructure;
using QuipuTest.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QuipuTest.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Аvailable tags property
        public static List<string> Tags
        {
            get
            {
                return UrlRepository.tags;
            }
        }
        #endregion

        #region Button content property
        private string _buttonContent;

        public string ButtonContent
        {
            get => _buttonContent;
            set
            {
                _buttonContent = value;
                OnPropertyChanged("ButtonContent");
            }
        }
        #endregion

        #region Path to file property
        private string _pathToFile;

        public string PathToFile
        {
            get
            {
                return _pathToFile;
            }
            set
            {
                _pathToFile = value;
                OnPropertyChanged(nameof(PathToFile));
            }
        }
        #endregion

        #region Current tag property
        private string _tag;
        public string Tag
        {
            get
            {
                if (string.IsNullOrEmpty(_tag))
                    _tag = Tags.First();
                return _tag;
            }
            set
            {
                _tag = value;
            }
        }
        #endregion

        #region Collection URLs property
        private ObservableCollection<Url> _urls;
        public ObservableCollection<Url> Urls
        {
            get
            {
                if (_urls == null)
                    _urls = new ObservableCollection<Url>();
                return _urls;
            }
        }

        #endregion

        #region Commands

        #region Calculation command

        #region Running calculation of tags property
        private bool _IsRunning;

        public bool IsRunning
        {
            get => _IsRunning;
            set
            {
                _IsRunning = value;
            }
        }
        #endregion

        private RelayCommand _calculationTagsCommand;

        private CancellationTokenSource _tokenSource;
        public ICommand CalculationTagsCommand
        {
            get
            {
                if (_calculationTagsCommand == null)
                    _calculationTagsCommand = new RelayCommand(ExecuteCalculationTagsCommand, CanExecuteCalculationTagsCommand);
                return _calculationTagsCommand;
            }
        }

        public async void ExecuteCalculationTagsCommand(object parameter)
        {
            if (IsRunning)
            {
                _tokenSource.Cancel();
                IsRunning = false;
                return;
            }
            IsRunning = true;

            _tokenSource = new CancellationTokenSource();
            CancellationToken canselToken = _tokenSource.Token;
            try
            {
                _urls = await Task.Run(() => UrlRepository.CalculationTags(canselToken, Tag, PathToFile));
                Urls.OrderBy(e => e.NumberTags).AsEnumerable();
                OnPropertyChanged(nameof(Urls));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Возникла ошибка:\n{ex.Message}");
            }
            finally
            {
                IsRunning = false;
                PathToFile = string.Empty;
                _tokenSource.Dispose();
            }
        }

        public bool CanExecuteCalculationTagsCommand(object parameter)
        {
            if (!string.IsNullOrEmpty(_pathToFile) && !string.IsNullOrEmpty(_tag))
            {
                if (IsRunning)
                    ButtonContent = "Остановить";
                return true;
            }
            else
            {
                ButtonContent = "Вычислить";
                return false;
            }
        }
        #endregion

        #region Call OFD command
        RelayCommand _openFileDialogCommand;

        public ICommand OpenFileDialogCommand
        {
            get
            {
                if (_openFileDialogCommand == null)
                    _openFileDialogCommand = new RelayCommand(ExecuteOpenFdCommand);
                return _openFileDialogCommand;
            }
        }

        public void ExecuteOpenFdCommand(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                PathToFile = openFileDialog.FileName;
            }
            else
                MessageBox.Show("Файл не выбран.");
        }
        #endregion

        #endregion

        protected override void OnDispose()
        {
            this.Urls.Clear();
        }
    }
}
