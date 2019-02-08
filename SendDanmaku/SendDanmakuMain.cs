using BilibiliDM_PluginFramework;
using Bililive_dm;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SendDanmaku
{
    public class SendDanmakuMain : DMPlugin
    {
        internal static SendDanmakuMain self;

        internal static SafeAPI api = null;
        internal static SendToolbar bar;
        internal static SendToolwindows Toolwindows;
        public Boolean Toolwindows_Visibility = false;

        public SendDanmakuMain()
        {
            if (self == null)
                self = this;
            else
                throw new InvalidOperationException();

            this.PluginName = "弹幕发送";
            this.PluginDesc = "在弹幕姬中快速发送弹幕";
            this.PluginAuth = "宅急送队长";
            this.PluginCont = "私信15253直播间主播或弹幕姬群内私聊";
            this.PluginVer = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

            VersionChecker.Check(this);
            
        }

        public override void Inited()
        {
            try
            {
                api = new SafeAPI();
            }
            catch (Exception ex)
            {
                Log("需要安装“登录中心”才能使用");
                return;
            }

            hackGUI();
            Start();
        }

        public override void Start()
        {
            Toolwindows = new SendToolwindows();
            Toolwindows.Show();
            Toolwindows_Visibility = true;
            base.Start();

        }

        public override void Stop()
        {
            Toolwindows_Visibility = false;
            Toolwindows.Close();
            base.Stop();
        }

        public override void Admin()
        {
            if (Toolwindows_Visibility)
            {
                Toolwindows.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("请先启用插件！");
            }
        }

        private void hackGUI()
        {
            MainWindow mw = (MainWindow)Application.Current.MainWindow;
            ItemsControl log = (ItemsControl)mw.FindName("Log");
            TabItem tab = (TabItem)log.Parent;
            Grid grid = new Grid();
            RowDefinition c1 = new RowDefinition();
            c1.Height = new GridLength(1, GridUnitType.Star);
            RowDefinition c2 = new RowDefinition();
            c2.Height = new GridLength(1, GridUnitType.Auto);
            grid.RowDefinitions.Add(c1);
            grid.RowDefinitions.Add(c2);
            bar = new SendToolbar();
            Grid.SetRow(bar, 1);
            tab.Content = grid;
            grid.Children.Add(log);
            var i = grid.Children.Add(bar);
        }

        internal static void log(string text)
        {
            self.Log(text);
        }

    }
}
