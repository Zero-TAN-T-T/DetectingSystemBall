using System;
using System.Windows.Forms;
using HandyControl.Controls;
using DetectingSystemBall.View;

namespace DetectingSystemBall
{
    internal class Notice
    {
        private Timer NotificationTimer;
        private int notificationCount = 0;

        private CleanMain cleanMain;

        public Notice(CleanMain cleanMain)
        {
            this.cleanMain = cleanMain;

            NotificationTimer = new System.Windows.Forms.Timer();
            NotificationTimer.Interval = 10 * 60 * 1000; // 10分钟，以毫秒为单位
            NotificationTimer.Tick += NotificationTimer_Tick;
            NotificationTimer.Start();
        }

        private void ShowNotification(string title, string message)
        {
        //hc: Growl.Warning(title, message);
            Growl.Warning(title, message);
        }

        private void NotificationTimer_Tick(object sender, EventArgs e)
        {
            if (cleanMain.DataBaseMarginClean < 20)
            {
                // 弹出通知
                ShowNotification("警告", "数据库空间严重不足！");
                notificationCount++;

                // 如果通知次数达到3次，停止定时器
                if (notificationCount >= 3)
                {
                    NotificationTimer.Stop();
                }
            }
        }
    }
}