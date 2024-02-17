using GalaSoft.MvvmLight.Threading;
using System;
using System.Windows;
using DetectingSystemBall.View;

namespace DetectingSystemBall
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DispatcherHelper.Initialize();
        }

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            try
            {
                FloatBallWindow floatBallWindow = new FloatBallWindow();
                floatBallWindow.Show();
            }
            catch (Exception ex) {  }
        }
    }
}
