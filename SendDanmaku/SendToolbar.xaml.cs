using LoginCenter.API;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SendDanmaku
{
    /// <summary>
    /// SendToolbar.xaml 的交互逻辑
    /// </summary>
    internal partial class SendToolbar : UserControl
    {
        internal SendToolbar()
        {
            InitializeComponent();
            list.Add(string.Empty);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {// 打开设置窗
            var result = await SendDanmakuMain.api.doAuth(SendDanmakuMain.self);
            switch (result)
            {
                case AuthorizationResult.Success:
                    SendDanmakuMain.log("插件已经被允许使用B站账号！");
                    break;
                case AuthorizationResult.Failure:
                    SendDanmakuMain.log("授权失败，用户拒绝");
                    break;
                case AuthorizationResult.Disabled:
                    SendDanmakuMain.log("权限被禁用，请到“登录中心”插件启用授权");
                    break;
                case AuthorizationResult.Timeout:
                    SendDanmakuMain.log("授权失败，用户未操作超时");
                    break;
                case AuthorizationResult.Illegal:
                case AuthorizationResult.Duplicate:
                default:
                    break;
            }
        }

        private void input_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {// 翻更老的一条记录
                history += 1;
                if (history > list.Count)
                    history = list.Count;
                input.Text = list[list.Count - history];
            }
            else if (e.Key == Key.Down)
            {
                history -= 1;
                if (history < 0)
                    history = 0;
                if (history == 0)
                    input.Text = string.Empty;
                else
                    input.Text = list[list.Count - history];
            }
        }

        // 0 的意思是当前正在打字的这个
        // 1 的意思是刚刚发出去的
        // 2 的意思是刚发出去的前一个

        // list数组序号：     0  1  2  3  4  5  6  7
        // 发送弹幕顺序： 老  1  2  3  4  5  6  7  8      新
        // hist变量内容：     8  7  6  5  4  3  2  1  0

        //                  list[list.Count - history]

        private int history = 0;
        private List<string> list = new List<string>();
        private void add2List(string text)
        {
            while (list.Count >= 10)
                list.RemoveAt(0);
            list.Add(text);
        }

        private async void input_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (input.Text.Contains("\n"))
                {
                    string text = input.Text.Replace("\r", string.Empty).Replace("\n", string.Empty);
                    input.Text = string.Empty;
                    history = 0;
                    add2List(text);
                    string result = null;
                    try
                    {
                        if (SendDanmakuMain.self.RoomId.HasValue)
                        {
                            result = await SendDanmakuAsync(SendDanmakuMain.self.RoomId.Value, text, LoginCenterAPI.getCookies());
                            //string.Join("; ", LoginCenterAPI.getCookies().GetCookies(new Uri("http://live.bilibili.com/")).OfType<Cookie>().Select(p => $"{p.Name}={p.Value}"))
                            //result = await SendDanmakuMain.api.send(SendDanmakuMain.self.RoomId.Value, text);
                        }
                        else
                        {
                            SendDanmakuMain.log("还未连接直播间！");
                            return;
                        }
                    }
                    catch (PluginNotAuthorizedException)
                    {
                        SendDanmakuMain.log("插件未被授权使用B站账号");
                        return;
                    }
                    if (result == null)
                    {
                        SendDanmakuMain.log("网络错误，请重试");
                    }
                    else
                    {
                        var j = JObject.Parse(result);
                        if (j["msg"].ToString() != string.Empty)
                        {
                            SendDanmakuMain.log("服务器返回：" + j["msg"].ToString());
                        }
                    }
                }
            }
            finally
            {// 统计弹幕字数
                text_count.Text = input.Text.Length.ToString();
            }
        }

        /// <summary>
        /// 异步发送弹幕
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="NotImplementedException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <param name="roomId">原房间号</param>
        /// <param name="danmaku">弹幕</param>
        /// <param name="cookie">发送账号的Cookie</param>
        /// <param name="color">弹幕颜色</param>
        public static async Task<string> SendDanmakuAsync(int roomId, string danmaku, CookieContainer cookie, int color = 16777215)
        {
            //IDictionary<string, string> Headers = new Dictionary<string, string>
            //{
            //    { "Origin", "https://live.bilibili.com" },
            //    { "Referer", $"https://live.bilibili.com/{GetShortRoomId(roomId)}" }
            //};
            int UnixTimeStamp = (int)((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            while (true)
            {
                IDictionary<string, object> Postdata = new Dictionary<string, object>
                {
                    { "color", color },
                    { "fontsize", 25 },
                    { "mode", 1 },
                    { "msg", WebUtility.UrlEncode(danmaku) },
                    { "rnd", UnixTimeStamp },
                    { "roomid", roomId },
                    { "csrf_token", cookie.GetCookies(new Uri("http://live.bilibili.com/")).OfType<Cookie>().FirstOrDefault(p => p.Name == "bili_jct")?.Value }
                };
                try
                {
                    return await HttpPostAsync("https://api.live.bilibili.com/msg/send", Postdata, 15, cookie: cookie/*, headers: Headers*/); ;
                }
                catch 
                {
                    return null;
                }
            }
        }
        public static async Task<string> HttpPostAsync(string url, string formdata, int timeout = 0, string userAgent = null, CookieContainer cookie = null, IDictionary<string, string> headers = null)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Accept = "*/*";
            request.Method = "POST";
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = userAgent ?? $"SendDanmaku/{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}";
            if (timeout != 0) { request.Timeout = timeout * 1000; request.ReadWriteTimeout = timeout * 1000; }
            else request.ReadWriteTimeout = 10000;
            request.CookieContainer = cookie;
            if (headers != null)
            {
                foreach (string key in headers.Keys)
                {
                    if (key.ToLower() == "accept")
                        request.Accept = headers[key];
                    else if (key.ToLower() == "host")
                        request.Host = headers[key];
                    else if (key.ToLower() == "referer")
                        request.Referer = headers[key];
                    else if (key.ToLower() == "content-type")
                        request.ContentType = headers[key];
                    else
                        request.Headers.Add(key, headers[key]);
                }
            }
            if (!string.IsNullOrEmpty(formdata))
            {
                byte[] data = Encoding.UTF8.GetBytes(formdata);
                using (Stream stream = await request.GetRequestStreamAsync())
                {
                    await stream.WriteAsync(data, 0, data.Length);
                }
            }
            using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                return await reader.ReadToEndAsync();
        }
        public static async Task<string> HttpPostAsync(string url, IDictionary<string, object> parameters = null, int timeout = 0, string userAgent = null, CookieContainer cookie = null, IDictionary<string, string> headers = null)
        {
            string formdata = string.Join("&", parameters.Select(p => $"{p.Key}={p.Value}"));
            return await HttpPostAsync(url, formdata, timeout, userAgent, cookie, headers);
        }
    }
}