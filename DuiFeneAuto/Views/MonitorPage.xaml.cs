using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using DuiFeneAuto.ViewModels;
namespace DuiFeneAuto.Views {
    /// <summary>
    /// MonitorPage.xaml 的交互逻辑
    /// </summary>
    public partial class MonitorPage : Page {
        public MonitorViewModel ViewModel { get; set; }
        public MonitorPage(NavigationService navigationService) {
            InitializeComponent();
            ViewModel = new MonitorViewModel(navigationService);
            this.DataContext = ViewModel;
            this.Loaded += MonitorPage_Loaded;
        }

        private async void MonitorPage_Loaded(object sender, RoutedEventArgs e) {
            await ViewModel.StartListeningAsync();
        }
    }
}
