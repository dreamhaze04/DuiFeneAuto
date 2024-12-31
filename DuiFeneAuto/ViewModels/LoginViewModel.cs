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
        private const string LINK_TEXT = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=wx1b5650884f657981&redirect_uri=https://www.duifene.com/_FileManage/PdfView.aspx?file=https%3A%2F%2Ffs.duifene.com%2Fres%2Fr2%2Fu6106199%2F%E5%AF%B9%E5%88%86%E6%98%93%E7%99%BB%E5%BD%95_876c9d439ca68ead389c.pdf&response_type=code&scope=snsapi_userinfo&connect_redirect=1#wechat_redirect";
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
