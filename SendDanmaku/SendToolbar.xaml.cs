using LoginCenter.API;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
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
            switch(result)
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
            try
            {
                if(e.Key == Key.Enter)
                {
                    string text = input.Text;
                    input.Text = string.Empty;
                    history = 0;
                    add2List(text);
                    string result = null;
                    try
                    {
                        if(SendDanmakuMain.self.RoomId.HasValue)
                        {
                            result = SendDanmakuMain.api.send(SendDanmakuMain.self.RoomId.Value, text);
                        }
                        else
                        {
                            SendDanmakuMain.log("还未连接直播间！");
                            return;
                        }
                    }
                    catch(PluginNotAuthorizedException)
                    {
                        SendDanmakuMain.log("插件未被授权使用B站账号");
                        return;
                    }
                    var j = JObject.Parse(result);
                    if(j["msg"].ToString() != string.Empty)
                    {
                        SendDanmakuMain.log("服务器返回：" + j["msg"].ToString());
                    }
                }
                else if(e.Key == Key.Up)
                {// 翻更老的一条记录
                    history += 1;
                    if(history > list.Count)
                        history = list.Count;
                    input.Text = list[list.Count - history];
                }
                else if(e.Key == Key.Down)
                {
                    history -= 1;
                    if(history < 0)
                        history = 0;
                    if(history == 0)
                        input.Text = string.Empty;
                    else
                        input.Text = list[list.Count - history];
                }
                else
                { }// do nothing
            }
            finally
            {// 统计弹幕字数
                text_count.Text = input.Text.Length.ToString();
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
            while(list.Count >= 10)
                list.RemoveAt(0);
            list.Add(text);
        }

    }
}
