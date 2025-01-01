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
            TextBlockText = "监听中";
        }
        public async Task StartListeningAsync() {
            if (await MainModel.Monitor()) {
                do {
                    _checktype = await MainModel.GetChecktype();
                    if (_checktype != null) {
                        CanSign = true;
                        if (_checktype == "1") {
                            TextBlockText = "监听到签到码签到\n" + await MainModel.GetCode();
                            Debug.WriteLine(TextBlockText);
                        } else if (_checktype == "2") {
                            TextBlockText = "监听到二维码签到";
                            Debug.WriteLine(TextBlockText);
                        } else if (_checktype == "3") {
                            TextBlockText = "监听到定位签到";
                            Debug.WriteLine(TextBlockText);
                        }
                    }
                    await Task.Delay(1000);
                } while (_checktype == null);
                
            }
        }
        private async void ExecuteSign(object obj) {
            _signStatus =  await MainModel.Sign(_checktype!);
            if (_signStatus) {
                MessageBox.Show("签到成功！");
            } else {
                MessageBox.Show("签到失败！");
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
