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
using System.Windows.Shapes;
using System.Windows.Interop;
using LoginCenter.API;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;

namespace SendDanmaku
{
    /// <summary>
    /// SendToolwindows.xaml 的交互逻辑
    /// </summary>
    public partial class SendToolwindows : Window
    {
        public SendToolwindows()
        {
            InitializeComponent();
            list.Add(string.Empty);
        }

        /// <summary>
        /// 热键消息
        /// </summary>
        public const int WM_HOTKEY = 0x0312;

        /// <summary>
        /// 自定义按键枚举
        /// </summary>
        public enum EKey
        {
            Space = 32,
            Left = 37,
            Up = 38,
            Right = 39,
            Down = 40,
            A = 65,
            B = 66,
            C = 67,
            D = 68,
            E = 69,
            F = 70,
            G = 71,
            H = 72,
            I = 73,
            J = 74,
            K = 75,
            L = 76,
            M = 77,
            N = 78,
            O = 79,
            P = 80,
            Q = 81,
            R = 82,
            S = 83,
            T = 84,
            U = 85,
            V = 86,
            W = 87,
            X = 88,
            Y = 89,
            Z = 90,
            F1 = 112,
            F2 = 113,
            F3 = 114,
            F4 = 115,
            F5 = 116,
            F6 = 117,
            F7 = 118,
            F8 = 119,
            F9 = 120,
            F10 = 121,
            F11 = 122,
            F12 = 123,
        }


        /// <summary>
        /// 如果函数执行成功，返回值不为0。
        /// 如果函数执行失败，返回值为0。要得到扩展错误信息，调用GetLastError。
        /// </summary>
        /// <param name="hWnd">要定义热键的窗口的句柄</param>
        /// <param name="id">定义热键ID（不能与其它ID重复）</param>
        /// <param name="fsModifiers">标识热键是否在按Alt、Ctrl、Shift、Windows等键时才会生效</param>
        /// <param name="vk">定义热键的内容</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint KeyModifiers, EKey vk);


        /// <summary>
        /// 注册热键
        /// </summary>
        /// <param name="hwnd">窗口句柄</param>
        /// <param name="hotKey_id">热键ID</param>
        /// <param name="keyModifiers">组合键</param>
        /// <param name="key">热键</param>
        public static void RegHotKey(IntPtr hwnd, int hotKeyId, uint KeyModifiers, EKey key)
        {
            if (!RegisterHotKey(hwnd, hotKeyId, KeyModifiers, key))
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode == 1409)
                {
                    MessageBox.Show("热键被占用 ！");
                }
                else
                {
                    MessageBox.Show("注册热键失败！错误代码：" + errorCode);
                }
            }
            else
            {
                SendDanmakuMain.log("注册热键成功");
            }
        }

        /// <summary>
        /// 取消注册的热键
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="id">注册的热键id</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private void SendTool_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //用来取消注册的热键
            UnregisterHotKey(new WindowInteropHelper(this).Handle, 37844);
        }

        private void SendTool_Loaded(object sender, RoutedEventArgs e)
        {
            //注册热键c+a+W，这里的37844就是一个ID识别
            RegHotKey(new WindowInteropHelper(this).Handle, 37844, 3, EKey.W);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        // 响应热键
        private IntPtr WndProc(IntPtr hwnd,int msg,IntPtr wParam,IntPtr lParam, ref bool handled)
        {
            if(msg == WM_HOTKEY)
                {       
                    if (wParam.ToInt32() == 37844)   //如果是注册的那个热键     
                    {
                        if (SendDanmakuMain.Toolwindows.IsVisible)
                        {
                            SendDanmakuMain.Toolwindows.Visibility= Visibility.Hidden;
                        }
                        else
                        {
                            SendDanmakuMain.Toolwindows.Visibility = Visibility.Visible;
                        }
                    }
                    handled = true;
            }
            return IntPtr.Zero;
        }

        

        private async void Button_Click(object sender, RoutedEventArgs e)
        {// 打开设置窗
            var result = await SendDanmakuMain.api.doAuth(SendDanmakuMain.self);
            switch (result)
            {
                case AuthorizationResult.Success:
                    SendDanmakuMain.log("插件已经被允许使用B站账号！");
                    input.IsEnabled = true;
                    SendDanmakuMain.Toolwindows.GridA.Children.Remove(Button);
                    help_Text.Text = "[回车键发送弹幕]按“Ctrl+Alt+W”隐藏/唤出发送窗";
                    break;
                case AuthorizationResult.Failure:
                    SendDanmakuMain.log("授权失败，用户拒绝");
                    help_Text.Text = "授权失败，用户拒绝";
                    break;
                case AuthorizationResult.Disabled:
                    SendDanmakuMain.log("权限被禁用，请到“登录中心”插件启用授权");
                    help_Text.Text = "权限被禁用，请到“登录中心”插件启用授权";
                    break;
                case AuthorizationResult.Timeout:
                    SendDanmakuMain.log("授权失败，用户未操作超时");
                    help_Text.Text = "授权失败，用户未操作超时";
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
                    help_Text.Text = "发送中。。。";
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
                            help_Text.Text = "还未连接至直播间！";
                            return;
                        }
                    }
                    catch (PluginNotAuthorizedException)
                    {
                        SendDanmakuMain.log("插件未被授权使用B站账号");
                        help_Text.Text = "插件未被授权使用B站账号";
                        return;
                    }
                    if (result == null)
                    {
                        SendDanmakuMain.log("网络错误，请重试");
                        help_Text.Text = "网络错误，请重试";
                    }
                    else
                    {
                        var j = JObject.Parse(result);
                        string msg = (j["msg"] ?? j["message"]).ToString();
                        if (msg != string.Empty)
                        {
                            SendDanmakuMain.log("服务器返回：" + msg);
                            help_Text.Text = "错误：" + msg;
                        }
                    }
                    help_Text.Text = "已发送！";
                }
            }
            finally
            {// 统计弹幕字数
                text_count.Text = input.Text.Length.ToString();
                if (input.Text.Length >= 20)
                {
                    text_count.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                }
                else
                {
                    text_count.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                }
            }
        }

        /// <summary>
        /// 异步发送弹幕
        /// </summary>
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
                string csrf = cookie.GetCookies(new Uri("http://live.bilibili.com/")).OfType<Cookie>().FirstOrDefault(p => p.Name == "bili_jct")?.Value;
                IDictionary<string, object> Postdata = new Dictionary<string, object>
                {
                    { "color", color },
                    { "fontsize", 25 },
                    { "mode", 1 },
                    { "msg", WebUtility.UrlEncode(danmaku) },
                    { "rnd", UnixTimeStamp },
                    { "roomid", roomId },
                    { "csrf_token", csrf },
                    { "csrf", csrf }
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
            request.UserAgent = userAgent ?? $"SendDanmaku/{SendDanmakuMain.self.PluginVer}";
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
