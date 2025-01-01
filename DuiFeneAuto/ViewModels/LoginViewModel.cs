using DuiFeneAuto.Commands;
using DuiFeneAuto.Models;
using DuiFeneAuto.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
namespace DuiFeneAuto.ViewModels
{
    class LoginViewModel : INotifyPropertyChanged
    {
        private readonly NavigationService _navigationService;
        private const string LINK_TEXT = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx1b5650884f657981&redirect_uri=https://www.duifene.com/&response_type=code&scope=snsapi_userinfo&connect_redirect=1#wechat_redirect";
        public ICommand LoginCommand { get; }
        public ICommand GetLinkCommand { get; }
        public LoginViewModel(NavigationService navigationService) {
            _navigationService = navigationService; 
            LoginCommand = new RelayCommand(ExecuteLogin);
            GetLinkCommand = new RelayCommand(ExecuteGetLink);
        }
        private async void ExecuteLogin(object? obj) {
            if (await MainModel.ExecuteLogin(Clipboard.GetText())) {
                await MainModel.GetClassJson();
                _navigationService.Navigate(new CoursePage(_navigationService));
            } else {
                MessageBox.Show("登录失败，请检查链接！");
            }
        }
        private void ExecuteGetLink(object? obj) {
            Clipboard.SetText(LINK_TEXT);
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
