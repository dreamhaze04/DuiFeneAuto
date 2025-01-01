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
using System.Windows.Controls;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices.ActiveDirectory;
namespace DuiFeneAuto.Models {
    public class MainModel {
        private const string UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3";
        private const string Host = "https://www.duifene.com";
        public static List<Dictionary<string, string>>? CourseData;
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
            if (parameter == null) {
                return false;
            }
            string userLink = (string)parameter;
            string pattern = "code=.*?&state=";
            var match = Regex.Match(userLink, pattern);
            if (!match.Success) {
                return false;
            }
            string code = Regex.Match(userLink!, pattern).Value.Replace("code=", "").Replace("&state=", "");
            var response = await Client.GetAsync($"https://www.duifene.com/P.aspx?authtype=1&code={code}&state=1");
            string html = await response.Content.ReadAsStringAsync();
            if (html.Contains("window.location='/_UserCenter/MB/LostInfo.aspx'")) {
                return false;
            }
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
        public static async Task<string?> GetChecktype() {
            var response = await Client.GetAsync($"https://www.duifene.com/_CheckIn/MB/TeachCheckIn.aspx?classid={_classId}&temps=0&checktype=1&isrefresh=0&timeinterval=0&roomid=0&match=");
            if (response.IsSuccessStatusCode) {
                var html = await response.Content.ReadAsStringAsync();
                if (html.Contains("HFCheck")) {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);
                    var hfChecktype = doc.GetElementbyId("HFChecktype")?.GetAttributeValue("value", null);
                    var hfCheckInId = doc.GetElementbyId("HFCheckInId")?.GetAttributeValue("value", null);
                    return hfChecktype;
                }
            }
            return null;
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
        public static async Task<bool> Sign(string checktype) {
            var response = await Client.GetAsync($"https://www.duifene.com/_CheckIn/MB/TeachCheckIn.aspx?classid={_classId}&temps=0&checktype=1&isrefresh=0&timeinterval=0&roomid=0&match=");
            if (response.IsSuccessStatusCode) {
                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                if (checktype == "1") {
                    var code = doc.GetElementbyId("HFCheckCodeKey")?.GetAttributeValue("value", null);
                    return await SignByCode(code);
                } else if (checktype == "2") {
                    var hfCheckInID = doc.GetElementbyId("HFCheckInID")?.GetAttributeValue("value", null);
                    return await SignByQr(hfCheckInID);
                } else {
                    var hfRoomLongitude = doc.GetElementbyId("HFRoomLongitude")?.GetAttributeValue("value", null);
                    var hfRoomLatitude = doc.GetElementbyId("HFRoomLatitude")?.GetAttributeValue("value", null);
                    return await SignByLocation(hfRoomLongitude!, hfRoomLatitude!);
                }
            }
            return false;

        }
        public static async Task<bool> SignByCode(string code) {
            await GetUserId();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("action", "studentcheckin"),
                new KeyValuePair<string, string>("studentid", _stuId!),
                new KeyValuePair<string, string>("checkincode", code)
            });

            var response = await Client.PostAsync($"https://www.duifene.com/_CheckIn/CheckIn.ashx", content);
            if (response.IsSuccessStatusCode) {
                var jsonString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;
                string? msgbox = root.GetProperty("msgbox").GetString();
                //return msgbox == "签到成功！";
                return true;
            }
            return false;
        }
        public static async Task<bool> SignByLocation(string hfRoomLongitude, string hfRoomLatitude) {
            await GetUserId();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("action", "signin"),
                new KeyValuePair<string, string>("cid", _classId),
                new KeyValuePair<string, string>("sid", _stuId),
                new KeyValuePair<string, string>("longitude", hfRoomLongitude),
                new KeyValuePair<string, string>("latitude", hfRoomLatitude)
            });
            var response = await Client.PostAsync($"https://www.duifene.com/_CheckIn/CheckInRoomHandler.ashx", content);
            if (response.IsSuccessStatusCode) {
                var jsonString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;
                string? msgbox = root.GetProperty("msgbox").GetString();
                Debug.WriteLine(msgbox);
                MessageBox.Show(msgbox);
                //return msgbox == "签到成功！";
                return true;
            }
            return false;
        }
        public static async Task<bool> SignByQr(string hfCheckInID) {
            await GetUserId();
            var response = await Client.GetAsync("https://www.duifene.com/_CheckIn/MB/QrCodeCheckOK.aspx?state=" + hfCheckInID);
            if (response.IsSuccessStatusCode) {
                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var msg = doc.GetElementbyId("DivOK").InnerText;
                return true;
            }
            return false;
        }
    }
}