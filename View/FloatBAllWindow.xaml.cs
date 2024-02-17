using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Collections.Generic;
using iPlant.Components.Framework.Util.Log;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

namespace DetectingSystemBall.View
{
    /// <summary>
    /// FloatAlarmWIndow.xaml 的交互逻辑
    /// </summary>
    public partial class FloatBallWindow : Window, INotifyPropertyChanged
    {
        //用于保存数据库余量的值
        //public String PercentageString { get; private set; }
        //public String FloatBallPercentageString { get; private set; }

        public decimal _percentage;
        public string percentage;

        // 定时器用于每隔10分钟更新数据库剩余空间
        private DispatcherTimer _updateTimer;

        //日志对象
        protected MyLog logger;

        //日志对象
        protected MyLog loggerErr;

        private DatabaseManager dbManager;

        //是否停留在此窗口上的时间
        private bool isMouseOver;
        //停留在窗口上的时间
        private DispatcherTimer timer;

        private bool shouldCloseOnMouseLeave = true;
        //显示数据库当前的剩余空间
        //private decimal percentage;

        private Dictionary<string, string[]> imagePaths;
        public FloatBallWindow()
        {
            InitializeComponent();

            //每十分钟更新一次数据库余量
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromMinutes(10); // 设置定时器间隔为10分钟
            _updateTimer.Tick += async (sender, e) => await DatabaseSpace();
            _updateTimer.Start();

            string loggerName = GetType().ToString();
            LogFactory.StartLog(loggerName, string.Format("\\{0}.log", loggerName));
            logger = LogFactory.GetLog(loggerName, loggerName);
            string loggerErrName = string.Format("\\{0}_err", loggerName);
            LogFactory.StartLog(loggerErrName, string.Format("\\{0}_err.log", loggerName));
            loggerErr = LogFactory.GetLog(loggerErrName, loggerErrName);

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            DataBaseMargin.Text = "计算中";
            await StartProgressUpdateTimer();
            await DatabaseSpace();
        }

        //private async Task SetPercentageString()
        //{
        //    await DatabaseSpace(); // 等待 DatabaseSpace 方法完成
        //    FloatBallPercentageString = PercentageString; // 将 PercentageString 的值赋给 FloatBallPercentageString
        //}

        private async Task DatabaseSpace()
        {
            dbManager = new DatabaseManager();
            decimal DatabaseSpaceValue = await dbManager.GetDatabaseSpaceAsync();


            imagePaths = new Dictionary<string, string[]>
            {
                { "red", new string[] { "red.png", "red1.png", "red2.png", "red3.png", "red4.png" } },
                { "yellow", new string[] { "yellow.png", "yellow1.png", "yellow2.png", "yellow3.png", "yellow4.png" } },
                { "orange", new string[] { "Center0.png", "orange1.png", "orange2.png", "orange3.png", "orange4.png" } },
                { "blue", new string[] { "blue.png" , "blue1.png", "blue2.png", "blue3.png", "blue4.png" } },
                { "green", new string[] { "green.png", "Out1.png", "Out2.png", "Out3.png", "Out4.png" } }
            };

            if (DatabaseSpaceValue > 0)
            {
                _percentage = 100 - (DatabaseSpaceValue / 11264) * 100;
                percentage = _percentage.ToString("0.00") + "%";
                DataBaseMargin.Text = percentage;
                UpdateBackgroundImage();
            }
            else
            {
                // 处理 totalSpace 为 0 的情况，以避免除以零错误
                DataBaseMargin.Text = "N/A";
            }

            // 初始化计时器
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3); // 设置定时器间隔为5秒
            timer.Tick += Timer_Tick;
        }

        

        private async Task StartProgressUpdateTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += async (sender, e) =>
            {
                await DatabaseSpace(); 
            };
            timer.Start();
        }
        private void UpdateBackgroundImage()
        {
            string colorKey;

            if (_percentage <= 20)
            {
                colorKey = "green";
            }
            else if (_percentage <= 40)
            {
                colorKey = "blue";
            }
            else if (_percentage <= 60)
            {
                colorKey = "orange";
            }
            else if (_percentage <= 80)
            {
                colorKey = "yellow";
            }
            else
            {
                colorKey = "red";
            }

            // 检查字典中是否包含键
            if (imagePaths.ContainsKey(colorKey))
            {
                // 获取指定颜色的图像路径
                string[] imagePathsForColor = imagePaths[colorKey];

                // 为ColorImage1 Grid中的每个Image元素设置图像源
                ColorImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resource/Image/" + imagePathsForColor[0]));
                ColorImage1.Source = new BitmapImage(new Uri("pack://application:,,,/Resource/Image/" + imagePathsForColor[1]));
                ColorImage2.Source = new BitmapImage(new Uri("pack://application:,,,/Resource/Image/" + imagePathsForColor[2]));
                ColorImage3.Source = new BitmapImage(new Uri("pack://application:,,,/Resource/Image/" + imagePathsForColor[3]));
                ColorImage4.Source = new BitmapImage(new Uri("pack://application:,,,/Resource/Image/" + imagePathsForColor[4]));
            }
            else
            {
                Console.WriteLine("找不到键: " + colorKey);
            }
        }

        //跟踪CleanMain页面的位置
        private CleanMain cleanMainWindow;
        //记录CleanMain页面是否打开
        private bool isCleanMainOpen = false;

        public event PropertyChangedEventHandler PropertyChanged;

        //鼠标悬停页面处理事件
        private void OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isCleanMainOpen)
            {
                timer.Stop();
            }
            else
            {
                isMouseOver = true;
                timer.Start();
                if (cleanMainWindow != null && !cleanMainWindow.IsVisible && isCleanMainOpen)
                {
                    cleanMainWindow = null; // 释放之前的窗口引用
                }
            }

        }

        //鼠标离开时处理事件
        private void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (shouldCloseOnMouseLeave)
            {
                isMouseOver = false;

                if (isCleanMainOpen && cleanMainWindow != null)
                {
                    isCleanMainOpen = false;
                    cleanMainWindow.Close();
                }
            }
            //else
            //{
            //    shouldCloseOnMouseLeave = true; // 重置标志以便下一次正常关闭
            //}

        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop(); // 停止计时器

            if (!isCleanMainOpen && isMouseOver)
            {
                if (cleanMainWindow == null || !cleanMainWindow.IsVisible)
                {
                    // 创建 CleanMain 窗口
                    cleanMainWindow = new CleanMain();

                    // 计算 CleanMain 窗口的预期位置，确保不超出屏幕边界
                    double cleanMainLeft = this.Left + this.Width;
                    double cleanMainTop = this.Top;

                    // 获取屏幕的工作区尺寸
                    var workingArea = System.Windows.SystemParameters.WorkArea;

                    // 调整 CleanMain 窗口的位置，确保不超出屏幕右边和底部
                    if (cleanMainLeft + cleanMainWindow.Width > workingArea.Right)
                    {
                        cleanMainLeft = workingArea.Right - cleanMainWindow.Width;
                    }
                    if (cleanMainTop + cleanMainWindow.Height > workingArea.Bottom)
                    {
                        cleanMainTop = workingArea.Bottom - cleanMainWindow.Height;
                    }

                    // 设置 CleanMain 窗口的位置
                    cleanMainWindow.Left = cleanMainLeft;
                    cleanMainWindow.Top = cleanMainTop;

                    // 更新状态
                    isCleanMainOpen = true;
                    shouldCloseOnMouseLeave = true;
                    // 显示 CleanMain 窗口
                    cleanMainWindow.Show();
                }
            }
        }

        //鼠标双击处理事件
        private void MBall_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            shouldCloseOnMouseLeave = false;
            timer.Stop(); // 停止计时器

            try
            {
                if (e.ChangedButton == MouseButton.Left && !isCleanMainOpen)
                {
                    cleanMainWindow = new CleanMain();
                    cleanMainWindow.Left = this.Left + (this.Width - cleanMainWindow.Width) / 2;
                    cleanMainWindow.Top = this.Top + (this.Height - cleanMainWindow.Height) / 2;
                    isCleanMainOpen = true;
                    // 订阅 CleanMain 窗口的关闭事件
                    cleanMainWindow.Closed += CleanMain_Closed;
                    shouldCloseOnMouseLeave = false;
                    cleanMainWindow.Show();
                }
                else
                {
                    MessageBox.Show("窗口已打开");
                }
            }
            catch (Exception ex)
            {
                loggerErr.Debug(ex.Message);
            }
        }
        //MBall窗口加载处理事件
        private void MBall_Loaded(object sender, RoutedEventArgs e)
        {
            // 获取屏幕的工作区尺寸
            var workingArea = System.Windows.SystemParameters.WorkArea;

            // 设置窗口的初始位置为屏幕右侧并居中
            Left = workingArea.Right - Width;
            Top = (workingArea.Height - Height) / 2;
        }

        private void MBall_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                // 创建一个弹出菜单
                ContextMenu menu = new ContextMenu();

                // 添加退出菜单项
                MenuItem exitMenuItem = new MenuItem();
                exitMenuItem.Header = "退出";
                exitMenuItem.Width = 100;
                exitMenuItem.FontSize = 12;
                exitMenuItem.Click += (s, args) =>
                {
                    Application.Current.Shutdown();
                };

                menu.Items.Add(exitMenuItem);

                // 显示菜单
                menu.IsOpen = true;
            }
        }

        //无边随意框拖动小球
        private void MBall_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CleanMain_Closed(object sender, EventArgs e)
        {
            isCleanMainOpen = false;
        }
    }
}
