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
using System.Text.RegularExpressions;

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
        /// 窗口拖动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private bool AltDown = false;
        /// <summary>
        /// 防止Ait+F4关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt)
            {
                AltDown = true;
            }
            else if (e.SystemKey == Key.F4 && AltDown)
            {
                e.Handled = true;
            }
        }
        /// <summary>
        /// 同上
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt)
            {
                AltDown = false;
            }
        }

        /// <summary>
        /// 读写配置文件
        /// </summary>
        public static class WinAPI
        {
            [DllImport("kernel32")] // 写入配置文件的接口
            private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
            [DllImport("kernel32")] // 读取配置文件的接口
            private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
            // 向配置文件写入值
            public static void ProfileWriteValue(string section, string key, string value, string path)
            {
                WritePrivateProfileString(section, key, value, path);
            }
            // 读取配置文件的值
            public static string ProfileReadValue(string section, string key, string path)
            {
                StringBuilder sb = new StringBuilder(255);
                GetPrivateProfileString(section, key, "", sb, 255, path);
                return sb.ToString().Trim();
            }
        }

        /// <summary>
        /// 热键消息
        /// </summary>
        public const int WM_HOTKEY = 0x0312;
        /// <summary>
        /// 点击计数
        /// </summary>
        public int ClickCount = 0;
        /// <summary>
        /// 开发者模式标识
        /// </summary>
        public bool DeveloperMode = false;
        /// <summary>
        /// 输入中标识
        /// </summary>
        public bool inputflag = false;
        /// <summary>
        /// 插件文件目录
        /// </summary>
        public string PluginPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\弹幕姬\plugins\发送弹幕\";

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
        /// <param name="fsModifiers">标识热键是否在按Alt、Ctrl、Shift、Windows等键时才会生效。Alt的值为1，Ctrl的值为2，Ctrl+Alt的值为3、Shift的值为4，Shift+Alt组合键为5，Shift+Alt+Ctrl组合键为7，Windows键的值为8</param>
        /// <param name="vk">定义热键的内容</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint KeyModifiers, EKey vk);
        /// <summary>
        /// 如果函数执行成功，返回值不为0。
        /// 如果函数执行失败，返回值为0。要得到扩展错误信息，调用GetLastError。
        /// </summary>
        /// <param name="hWnd">要定义热键的窗口的句柄</param>
        /// <param name="id">定义热键ID（不能与其它ID重复）</param>
        /// <param name="fsModifiers">标识热键是否在按Alt、Ctrl、Shift、Windows等键时才会生效。Alt的值为1，Ctrl的值为2，Ctrl+Alt的值为3、Shift的值为4，Shift+Alt组合键为5，Shift+Alt+Ctrl组合键为7，Windows键的值为8</param>
        /// <param name="vk">定义热键的内容</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint KeyModifiers, int key);

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
                    MessageBox.Show("热键注册失败！错误代码：" + errorCode);
                }
            }
            else
            {
                SendDanmakuMain.log("热键注册成功");
            }
        }

        /// <summary>
        /// 注册热键
        /// </summary>
        /// <param name="hwnd">窗口句柄</param>
        /// <param name="hotKey_id">热键ID</param>
        /// <param name="keyModifiers">组合键</param>
        /// <param name="key">热键</param>
        public static void RegHotKey(IntPtr hwnd, int hotKeyId, uint KeyModifiers, int key)
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
                    MessageBox.Show("热键注册失败！错误代码：" + errorCode);
                }
            }
            else
            {
                SendDanmakuMain.log("热键注册成功");
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

        /// <summary>
        /// 在窗口关闭时注销热键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendTool_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //用来取消注册的热键
            UnregisterHotKey(new WindowInteropHelper(this).Handle, 37844);
        }

        /// <summary>
        /// 窗口加载完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendTool_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DeveloperMode = Convert.ToBoolean(WinAPI.ProfileReadValue("SendDanmaku", "DeveloperMode", PluginPath + "Config.ini"));
                SendTool.Opacity = Convert.ToDouble(WinAPI.ProfileReadValue("SendDanmaku", "Opacity", PluginPath + "Config.ini"));
                SendTool.Topmost = Convert.ToBoolean(WinAPI.ProfileReadValue("SendDanmaku", "Topmost", PluginPath + "Config.ini"));
                string[] sArray = Regex.Split(WinAPI.ProfileReadValue("SendDanmaku", "Hotkey", PluginPath + "Config.ini"), " ", RegexOptions.IgnoreCase);
                RegHotKey(new WindowInteropHelper(this).Handle, 37844, Convert.ToUInt16(sArray[0]), Convert.ToUInt16(sArray[1]));
                sArray = Regex.Split(WinAPI.ProfileReadValue("SendDanmaku", "Size", PluginPath + "Config.ini"), " ", RegexOptions.IgnoreCase);
                SendTool.Width = Convert.ToDouble(sArray[0]);
                SendTool.Height = Convert.ToDouble(sArray[1]);
                text_count_max.Text = Convert.ToString(WinAPI.ProfileReadValue("SendDanmaku", "Danmucountmax", PluginPath + "Config.ini"));
                input.MaxLength = Convert.ToInt16(text_count_max.Text);
            }
            catch (Exception)
            {
                //配置文件读取失败或不存在
                try
                {
                    if (false == System.IO.Directory.Exists(PluginPath))
                    {
                        //创建文件夹
                        Directory.CreateDirectory(PluginPath);

                        //创建配置文件
                        StreamWriter t = new StreamWriter(PluginPath + "Config.ini", false, System.Text.Encoding.ASCII);
                        t.Close();
                    }
                    else
                    {
                        //创建配置文件
                        StreamWriter t = new StreamWriter(PluginPath + "Config.ini", false, System.Text.Encoding.ASCII);
                        t.Close();
                    }

                    try
                    {
                        WinAPI.ProfileWriteValue("SendDanmaku", "DeveloperMode", DeveloperMode.ToString(), PluginPath + "Config.ini");
                        WinAPI.ProfileWriteValue("SendDanmaku", "Opacity", SendTool.Opacity.ToString(), PluginPath + "Config.ini");
                        WinAPI.ProfileWriteValue("SendDanmaku", "Topmost", SendTool.Topmost.ToString(), PluginPath + "Config.ini");
                        WinAPI.ProfileWriteValue("SendDanmaku", "Hotkey", 3 + " " + 87, PluginPath + "Config.ini");
                        WinAPI.ProfileWriteValue("SendDanmaku", "Size", 768 + " " + 32, PluginPath + "Config.ini");
                        WinAPI.ProfileWriteValue("SendDanmaku", "Danmucountmax", "30", PluginPath + "Config.ini");
                        //开始插件初始化
                        DeveloperMode = Convert.ToBoolean(WinAPI.ProfileReadValue("SendDanmaku", "DeveloperMode", PluginPath + "Config.ini"));

                        SendTool.Opacity = Convert.ToDouble(WinAPI.ProfileReadValue("SendDanmaku", "Opacity", PluginPath + "Config.ini"));
                        SendDanmakuMain.log("已调整透明度为" + SendTool.Opacity.ToString());

                        SendTool.Topmost = Convert.ToBoolean(WinAPI.ProfileReadValue("SendDanmaku", "Topmost", PluginPath + "Config.ini"));
                        SendDanmakuMain.log("已调整窗口置顶为" + SendTool.Topmost.ToString());

                        string[] sArray = Regex.Split(WinAPI.ProfileReadValue("SendDanmaku", "Hotkey", PluginPath + "Config.ini"), " ", RegexOptions.IgnoreCase);
                        RegHotKey(new WindowInteropHelper(this).Handle, 37844, Convert.ToUInt16(sArray[0]), Convert.ToUInt16(sArray[1]));
                        SendDanmakuMain.log("已调整热键为 " + sArray[0] + " " + sArray[1]);

                        sArray = Regex.Split(WinAPI.ProfileReadValue("SendDanmaku", "Size", PluginPath + "Config.ini"), " ", RegexOptions.IgnoreCase);
                        SendTool.Width = Convert.ToDouble(sArray[0]);
                        SendTool.Height = Convert.ToDouble(sArray[1]);
                        SendDanmakuMain.log("已调整窗口大小为" + sArray[0] +"*"+ sArray[1]);

                        text_count_max.Text = Convert.ToString(WinAPI.ProfileReadValue("SendDanmaku", "Danmucountmax", PluginPath + "Config.ini"));
                        input.MaxLength = Convert.ToInt16(text_count_max.Text);
                        SendDanmakuMain.log("已设置弹幕字数上线为" + text_count_max.Text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("配置文件写入失败，请检查！\n" + ex.ToString(), "开发者模式", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("配置文件创建失败，请检查！\n" + ex.ToString(), "开发者模式", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }

        /// <summary>
        /// WPFWndProc支持
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        /// <summary>
        /// 监听热键事件
        /// </summary>
        /// <param name="hwnd">窗口句柄</param>
        /// <param name="msg">消息</param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="handled">处理结果</param>
        /// <returns></returns>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                if (wParam.ToInt32() == 37844)   //对比热键ID
                {
                    if (SendDanmakuMain.Toolwindows.IsVisible)
                    {
                        SendDanmakuMain.Toolwindows.Visibility = Visibility.Hidden;
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

        /// <summary>
        /// 申请授权
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var result = await SendDanmakuMain.api.doAuth(SendDanmakuMain.self);
            switch (result)
            {
                case AuthorizationResult.Success:
                    SendDanmakuMain.log("插件已经被允许使用B站账号！");
                    input.IsEnabled = true;
                    SendDanmakuMain.Toolwindows.GridA.Children.Remove(Button);
                    if (!DeveloperMode) { help_Text.Text = "[回车键发送弹幕] 按“Ctrl+Alt+W”隐藏/唤出发送窗"; } else { help_Text.Text = "[回车键发送弹幕] 请使用您自定义的键位隐藏/唤出发送窗"; }
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

        /// <summary>
        /// 开发者模式入口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!DeveloperMode)
            {
                ClickCount++;
                if (ClickCount == 5)
                {
                    ClickCount = 0;
                    DeveloperMode = true;
                    try
                    {
                        WinAPI.ProfileWriteValue("SendDanmaku", "DeveloperMode", DeveloperMode.ToString(), PluginPath + "Config.ini");
                        SendDanmakuMain.log("您已进入开发者模式！");
                        help_Text.Text = "您已进入开发者模式！";
                        MessageBox.Show("\n辅助键：\nAlt=1\nCtrl=2\nCtrl+Alt=3\nShift=4\nShift+Alt=5\nShift+Alt+Ctrl=7\nWindows键=8\n\n主键：\nSpace=32\nLeft=37\nUp=38\nRight=39\nDown=40\nA=65\nB=66\nC=67\nD=68\nE=69\nF=70\nG=71\nH=72\nI=73\nJ=74\nK=75\nL=76\nM=77\nN=78\nO=79\nP=80\nQ=81\nR=82\nS=83\nT=84\nU=85\nV=86\nW=87\nX=88\nY=89\nZ=90\nF1=112\nF2=113\nF3=114\nF4=115\nF5=116\nF6=117\nF7=118\nF8=119\nF9=120\nF10=121\nF11=122\nF12=123\n", "开发者模式", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("配置文件写入失败，请检查！\n" + ex.ToString(), "开发者模式", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else if (ClickCount < 5)
                {
                    SendDanmakuMain.log("再点击 " + (5 - ClickCount) + " 次将会进入开发者模式");
                    help_Text.Text = "再点击 " + (5 - ClickCount) + " 次将会进入开发者模式";
                    return;
                }
                else if (ClickCount > 5)
                {
                    SendDanmakuMain.log("您已进入开发者模式！");
                    help_Text.Text = "您已进入开发者模式！";
                    return;
                }
            } else {
                SendDanmakuMain.log("您已处于开发者模式！");
                help_Text.Text = "您已处于开发者模式！";
                return;
            }
            return;
        }

        /// <summary>
        /// 文本框记录查找
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 开发者模式主程序
        /// </summary>
        /// <param name="data">指令和参数</param>
        /// <returns></returns>
        private int DeveloperModeSet(string[] data)
        {
            switch (data[1])
            {
                case "Opacity":
                    try
                    {
                        SendTool.Opacity = Convert.ToDouble(data[2]);
                        SendDanmakuMain.log("已调整透明度为" + SendTool.Opacity);
                        WinAPI.ProfileWriteValue("SendDanmaku", "Opacity", SendTool.Opacity.ToString(), PluginPath + "Config.ini");
                        return 0;
                    }
                    catch (Exception)
                    {
                        return 1;
                    }
                case "Topmost":
                    try
                    {
                        SendTool.Topmost = Convert.ToBoolean(data[2]);
                        SendDanmakuMain.log("已调整窗口置顶为" + SendTool.Topmost.ToString());
                        WinAPI.ProfileWriteValue("SendDanmaku", "Topmost", SendTool.Topmost.ToString(), PluginPath + "Config.ini");
                        return 0;
                    }
                    catch (Exception)
                    {
                        return 1;
                    }
                case "Hotkey":
                    try
                    {
                        //注销热键
                        UnregisterHotKey(new WindowInteropHelper(this).Handle, 37844);
                        //注册热键
                        RegHotKey(new WindowInteropHelper(this).Handle, 37844, Convert.ToUInt16(data[2]), Convert.ToUInt16(data[3]));
                        SendDanmakuMain.log("已调整热键为" + data[2] + data[3]);
                        MessageBox.Show("已调整热键为" + data[2] + data[3], "开发者模式", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                        WinAPI.ProfileWriteValue("SendDanmaku", "Hotkey", data[2] + " " + data[3], PluginPath + "Config.ini");
                        return 0;
                    }
                    catch (Exception)
                    {
                        return 1;
                    }
                case "Size":
                    try
                    {
                        SendTool.Width = Convert.ToDouble(data[2]);
                        SendTool.Height = Convert.ToDouble(data[3]);
                        SendDanmakuMain.log("已调整窗口大小为 " + data[2] +" * "+ data[3]);
                        WinAPI.ProfileWriteValue("SendDanmaku", "Size", data[2] + " " + data[3], PluginPath + "Config.ini");
                        return 0;
                    }
                    catch (Exception)
                    {
                        return 1;
                    }
                case "Danmucountmax":
                    try
                    {
                        if (!(Convert.ToInt16(data[2]) <= 21))
                        {
                            input.MaxLength = Convert.ToInt16(data[2]);
                            text_count_max.Text = input.MaxLength.ToString();
                            SendDanmakuMain.log("已弹幕字数上限为 " + data[2]);
                            WinAPI.ProfileWriteValue("SendDanmaku", "Danmucountmax", data[2], PluginPath + "Config.ini");
                            return 0;
                        } else {
                            switch (MessageBox.Show("注意，字数上限小于或等于21可能会导致您无法修改该数值。\n是否确认修改？", "开发者模式", MessageBoxButton.YesNo, MessageBoxImage.Asterisk))
                            {
                                case MessageBoxResult.Yes:
                                    input.MaxLength = Convert.ToInt16(data[2]);
                                    text_count_max.Text = input.MaxLength.ToString();
                                    SendDanmakuMain.log("已弹幕字数上限为 " + data[2]);
                                    WinAPI.ProfileWriteValue("SendDanmaku", "Danmucountmax", data[2], PluginPath + "Config.ini");
                                    return 0;
                                case MessageBoxResult.No:
                                    break;
                            }
                        }
                        return 0;
                    }
                    catch (Exception)
                    {
                        return 1;
                    }
                default:
                    MessageBox.Show("没有这个指令！", "开发者模式", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    break;
            }
            return 1;
        }

        /// <summary>
        /// 用户输入监听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void input_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (input.Text.Contains("\n"))
                {
                    if (input.Text.Remove(1, input.Text.Length - 1) == "/") {
                        if (DeveloperMode)
                        {
                            string[] sArray = Regex.Split(input.Text.Remove(0, 1).Replace("\r", string.Empty).Replace("\n", string.Empty), " ", RegexOptions.IgnoreCase);
                            switch (sArray[0])
                            {
                                case "set":
                                    if (DeveloperModeSet(sArray) == 1)
                                    {
                                        help_Text.Text = "错误的指令或数值！";
                                        break;
                                    }
                                    else
                                    {
                                        help_Text.Text = "指令 " + sArray[1] + " 执行成功！";
                                        add2List(input.Text);
                                        return;
                                    }
                                case "reset":
                                case "initialize":
                                    switch (MessageBox.Show("注意，该操作将会初始化配置文件。\n是否确认初始化？", "开发者模式", MessageBoxButton.YesNo, MessageBoxImage.Asterisk))
                                    {
                                        case MessageBoxResult.Yes:
                                            WinAPI.ProfileWriteValue("SendDanmaku", "DeveloperMode", DeveloperMode.ToString(), PluginPath + "Config.ini");
                                            WinAPI.ProfileWriteValue("SendDanmaku", "Opacity", SendTool.Opacity.ToString(), PluginPath + "Config.ini");
                                            WinAPI.ProfileWriteValue("SendDanmaku", "Topmost", SendTool.Topmost.ToString(), PluginPath + "Config.ini");
                                            WinAPI.ProfileWriteValue("SendDanmaku", "Hotkey", 3 + " " + 87, PluginPath + "Config.ini");
                                            WinAPI.ProfileWriteValue("SendDanmaku", "Size", 768 + " " + 32, PluginPath + "Config.ini");
                                            WinAPI.ProfileWriteValue("SendDanmaku", "Danmucountmax", "30", PluginPath + "Config.ini");
                                            //开始插件初始化
                                            DeveloperMode = Convert.ToBoolean(WinAPI.ProfileReadValue("SendDanmaku", "DeveloperMode", PluginPath + "Config.ini"));

                                            SendTool.Opacity = Convert.ToDouble(WinAPI.ProfileReadValue("SendDanmaku", "Opacity", PluginPath + "Config.ini"));
                                            SendDanmakuMain.log("已调整透明度为" + SendTool.Opacity.ToString());

                                            SendTool.Topmost = Convert.ToBoolean(WinAPI.ProfileReadValue("SendDanmaku", "Topmost", PluginPath + "Config.ini"));
                                            SendDanmakuMain.log("已调整窗口置顶为" + SendTool.Topmost.ToString());

                                            sArray = Regex.Split(WinAPI.ProfileReadValue("SendDanmaku", "Hotkey", PluginPath + "Config.ini"), " ", RegexOptions.IgnoreCase);
                                            UnregisterHotKey(new WindowInteropHelper(this).Handle, 37844);
                                            RegHotKey(new WindowInteropHelper(this).Handle, 37844, Convert.ToUInt16(sArray[0]), Convert.ToUInt16(sArray[1]));
                                            SendDanmakuMain.log("已调整热键为 " + sArray[0] + " " + sArray[1]);

                                            sArray = Regex.Split(WinAPI.ProfileReadValue("SendDanmaku", "Size", PluginPath + "Config.ini"), " ", RegexOptions.IgnoreCase);
                                            SendTool.Width = Convert.ToDouble(sArray[0]);
                                            SendTool.Height = Convert.ToDouble(sArray[1]);
                                            SendDanmakuMain.log("已调整窗口大小为" + sArray[0] + "*" + sArray[1]);

                                            text_count_max.Text = Convert.ToString(WinAPI.ProfileReadValue("SendDanmaku", "Danmucountmax", PluginPath + "Config.ini"));
                                            input.MaxLength = Convert.ToInt16(text_count_max.Text);
                                            SendDanmakuMain.log("已设置弹幕字数上限为" + text_count_max.Text);
                                            help_Text.Text = "初始化成功！";
                                            break;
                                        case MessageBoxResult.No:
                                            break;
                                    }
                                    break;
                                default:
                                    help_Text.Text = "缺少参数！";
                                    return;
                            }
                            input.Text = string.Empty;
                            return;
                        }
                    }
                    help_Text.Text = "发送中...";
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
                if (input.Text.Length >= input.MaxLength)
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
