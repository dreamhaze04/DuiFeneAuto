using DuiFeneAuto.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Windows.Navigation;
using DuiFeneAuto.Models;
using DuiFeneAuto.Views;
using System.Diagnostics;
namespace DuiFeneAuto.ViewModels {
    public class CourseViewModel {
        public ObservableCollection<Course> CourseList { get; set; }
        public Course SelectedCourse { get; set; }
        public ICommand StartSignInCommand { get; }
        private NavigationService _navigationService;
        public CourseViewModel(NavigationService navigationService) {
            // 初始化课程列表
            CourseList = new ObservableCollection<Course> { };
            foreach (var item in MainModel.CourseData!) {
                CourseList.Add(new Course { CourseName = item["CourseName"]});
            }
            _navigationService = navigationService;
            // 初始化签到命令
            StartSignInCommand = new RelayCommand(ExecuteStartSignIn);
        }

        // 签到操作
        private void ExecuteStartSignIn(object obj) {
            if (SelectedCourse != null) {
                MainModel.CourseId = MainModel.CourseData!.First(x => x["CourseName"] == SelectedCourse.CourseName)["CourseID"];
                _navigationService.Navigate(new MonitorPage(_navigationService));
            } else {
                MessageBox.Show("请选择一个课程进行签到！");
            }
        }
    }

    public class Course {
        public string CourseName { get; set; }
    }
}
