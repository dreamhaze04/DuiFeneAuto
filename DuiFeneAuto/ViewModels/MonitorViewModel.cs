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
        private string? _textBlockText;
        public ICommand SignCommand { get; }
        private string? _code;
        public  string? Code {
            get => _code;
            set {
                if (_code != value) {
                    _code = value;
                    (SignCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }
        private static bool _signStatus = false;
        public string TextBlockText {
            get => _textBlockText!;
            set {
                _textBlockText = value;
                OnPropertyChanged(nameof(TextBlockText));
            }
        }
        
        private NavigationService _navigationService;
        public MonitorViewModel(NavigationService navigationService) {
            _navigationService = navigationService;
            SignCommand = new RelayCommand(ExecuteSign, () => { return Code != null; });
            _textBlockText = "测试";
        }
        public async Task StartListeningAsync() {
            if (await MainModel.Monitor()) {
                do {
                    Code = await MainModel.GetCode();
                    if (Code != null) {
                        TextBlockText = "获取到签到码\n" + Code;
                        Debug.WriteLine(Code);
                    } else {
                        _textBlockText = "监听中";
                        Debug.WriteLine("code is null");
                    }
                    await Task.Delay(1000);
                } while (Code == null);
                while (!_signStatus) {
                    _signStatus = await MainModel.AutoSign();
                    await Task.Delay(1000);
                }
            }
        }
        private async void ExecuteSign(object obj) {
            _signStatus = await MainModel.Sign(Code!);
            if (_signStatus) {
                MessageBox.Show("签到成功");
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
