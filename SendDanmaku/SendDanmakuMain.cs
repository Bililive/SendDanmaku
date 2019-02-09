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
            catch (Exception)
            {
                Log("需要安装“登录中心”才能使用");
                return;
            }

            hackGUI();
            Toolwindows = new SendToolwindows();
            Toolwindows.Hide();
        }

        public override void Start()
        {
            try
            {
                Toolwindows.Show();
            }
            catch (Exception)
            {
                MessageBox.Show("发送弹幕窗口出现异常，请尝试重启弹幕姬！", this.PluginName, MessageBoxButton.OK, MessageBoxImage.Error);
                base.Stop();
                return;
            }
            base.Start();

        }

        public override void Stop()
        {
            try
            {
                Toolwindows.Hide();
            }
            catch (Exception)
            {
                MessageBox.Show("发送弹幕窗口出现异常，请尝试重启弹幕姬！", this.PluginName, MessageBoxButton.OK, MessageBoxImage.Error);
                base.Stop();
                return;
            }
            base.Stop();
        }

        public override void DeInit()
        {
            Toolwindows.Close();
        }

        public override void Admin()
        {
            try
            {
                Toolwindows.Visibility = Visibility.Visible;
            }
            catch (Exception)
            {
                MessageBox.Show("发送弹幕窗口出现异常，请尝试重启弹幕姬！", this.PluginName, MessageBoxButton.OK, MessageBoxImage.Error);
                base.Stop();
                return;
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
