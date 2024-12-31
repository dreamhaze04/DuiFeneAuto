using DuiFeneAuto.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Configuration;
using System.Windows;
namespace DuiFeneAuto.Models {
    public class MainModel {
        private const string UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3";
        private const string Host = "https://www.duifene.com";
        public static List<Dictionary<string, string>>? CourseData;
        private static int seconds = 10;
        private static string? _stuId, _classId;
        private static string? _courseId;
        public static string? CourseId {
            get => _courseId;
            set => _courseId = value;
        }
        public static readonly HttpClient Client = new() {
            DefaultRequestHeaders = {
                { "User-Agent", UA }
            }
        };
        public static async Task<bool> ExecuteLogin(object parameter) {
            string? userLink = parameter as string;
            string pattern = "code=.*?&state=";
            string code = Regex.Match(userLink!, pattern).Value.Replace("code=", "").Replace("&state=", "");
            var response = await Client.GetAsync($"https://www.duifene.com/P.aspx?authtype=1&code={code}&state=1");
            return true;
        }
        public static async Task GetClassJson() {
            var content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("action", "getstudentcourse"),
            new KeyValuePair<string, string>("classtypeid", "2")
        });

            var response = await Client.PostAsync($"https://www.duifene.com/_UserCenter/CourseInfo.ashx", content);
            if (response.IsSuccessStatusCode) {
                var jsonString = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(jsonString);
                CourseData = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(jsonString);
            }
        }
        public static async Task<bool> Monitor() {
            Client.DefaultRequestHeaders.Remove("Referer");
            Client.DefaultRequestHeaders.Add("Referer", "https://www.duifene.com/_UserCenter/MB/index.aspx");
            if (_courseId != null) {
                Debug.WriteLine($"{_courseId}");
            } else {
                Debug.WriteLine("_courseId is null");
                return false;
            }
            var response = await Client.GetAsync($"https://www.duifene.com/_UserCenter/MB/Module.aspx?data={_courseId}");      
            Debug.WriteLine(await response.Content.ReadAsStringAsync());
            if (response.IsSuccessStatusCode) {
                var html = await response.Content.ReadAsStringAsync();
                if (html.Contains(_courseId!)) {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);
                    var courseName = doc.GetElementbyId("CourseName")?.InnerText;
                    return true;
                }
            }
            return false;
        }
        public static async Task<string?> GetCode() {
            var response = await Client.GetAsync($"https://www.duifene.com/_CheckIn/MB/TeachCheckIn.aspx?classid={_classId}&temps=0&checktype=1&isrefresh=0&timeinterval=0&roomid=0&match=");
            Debug.WriteLine("监听中");
            if (response.IsSuccessStatusCode) {
                var html = await response.Content.ReadAsStringAsync();
                if (html.Contains("HFCheckCodeKey")) {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);
                    var code = doc.GetElementbyId("HFCheckCodeKey")?.GetAttributeValue("value", null);
                    var hfSeconds = doc.GetElementbyId("HFSeconds")?.GetAttributeValue("value", null);
                    if (code != null) {
                        return code;
                    }
                }
            }
            return null;
        }
        public static async Task<bool> AutoSign() {
            var response = await Client.GetAsync($"https://www.duifene.com/_CheckIn/MB/TeachCheckIn.aspx?classid={_classId}&temps=0&checktype=1&isrefresh=0&timeinterval=0&roomid=0&match=");
            if (response.IsSuccessStatusCode) {
                var html = await response.Content.ReadAsStringAsync();
                if (html.Contains("HFCheckCodeKey")) {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);
                    var code = doc.GetElementbyId("HFCheckCodeKey")?.GetAttributeValue("value", null);
                    var hfSeconds = doc.GetElementbyId("HFSeconds")?.GetAttributeValue("value", null);

                    if (int.Parse(hfSeconds) <= seconds) {
                        Debug.WriteLine($"开始签到 签到码: {code}");
                        
                        return await Sign(code);
                    }
                }
            }
            return false;
        }
        private static async Task GetUserId() {
            var response = await Client.GetAsync($"https://www.duifene.com/_UserCenter/MB/index.aspx");
            if (response.IsSuccessStatusCode) {
                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                _stuId = doc.GetElementbyId("hidUID")?.GetAttributeValue("value", null);
            }
        }
        public static async Task<bool> Sign(string code) {
            await GetUserId();
            var content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("action", "studentcheckin"),
            new KeyValuePair<string, string>("studentid", _stuId),
            new KeyValuePair<string, string>("checkincode", code)
        });

            var response = await Client.PostAsync($"https://www.duifene.com/_CheckIn/CheckIn.ashx", content);
            if (response.IsSuccessStatusCode) {
                var jsonString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;
                string msgbox = root.GetProperty("msgbox").GetString();
                Debug.WriteLine(msgbox);
                MessageBox.Show(msgbox);
                return msgbox == "签到成功！";
            }
            return false;
        }
    }

}
