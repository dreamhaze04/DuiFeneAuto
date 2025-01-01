using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using DuiFeneAuto.Commands;
using DuiFeneAuto.Models;
namespace DuiFeneAuto.ViewModels
{
    public class MonitorViewModel : INotifyPropertyChanged {
        private string? _textBlockText = "test";
        public ICommand SignCommand { get; }
        private string? _checktype;
        private static bool _signStatus = false;
        public string TextBlockText {
            get => _textBlockText!;
            set {
                _textBlockText = value;
                OnPropertyChanged(nameof(TextBlockText));
            }
        }
        private bool _canSign = false;
        public bool CanSign {
            get => _canSign;
            set {
                if (_canSign != value) {
                    _canSign = value;
                    (SignCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }
        private NavigationService _navigationService;
        public MonitorViewModel(NavigationService navigationService) {
            _navigationService = navigationService;
            SignCommand = new RelayCommand(ExecuteSign, () => { return _canSign; });
            TextBlockText = "测试";
        }
        public async Task StartListeningAsync() {
            if (await MainModel.Monitor()) {
                do {
                    _checktype = await MainModel.GetChecktype();
                    if (_checktype != null) {
                        CanSign = true;
                    } else {
                        _textBlockText = "监听中";
                        Debug.WriteLine("checktype is null");
                    }
                    await Task.Delay(1000);
                } while (_checktype == null);
                //while (!_signStatus) {
                //    _signStatus = await MainModel.AutoSign();
                //    await Task.Delay(1000);
                //}
            }
        }
        private async void ExecuteSign(object obj) {
            await MainModel.Sign(_checktype!);
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
