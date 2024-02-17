using iPlant.Components.Framework.Util.Enums;
using iPlant.Components.Framework.Util.Log;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DetectingSystemBall.Conn;
using Path = System.IO.Path;

namespace DetectingSystemBall.View
{
    /// <summary>
    /// CleanMain.xaml 的交互逻辑
    /// </summary>
    public partial class CleanMain : Window
    {
        private DatabaseManager dbManager;

        //根据警戒值显示对应的圆球颜色
        public decimal percentage;

        //日志对象
        protected MyLog logger;

        //日志对象
        protected MyLog loggerErr;

        //获取当前时间,用于检测当天的数据是否有报错
        public string todayDateString;

        

        //private bool MarginBtnEnter = false;

        //private bool MQBtnEnter = false;

        //private bool DatacleanBtnEnter = false;

        //private bool BackupDataBaseBtnEnter = false;

        //private bool UndeterminedBtnEnter = false;

        //private bool MarginBtnLeave = false;

        //private bool MQBtnLeave = false;

        //private bool DatacleanBtnLeave = false;

        //private bool BackupDataBaseBtnLeave = false;

        //private bool UndeterminedBtnLeave = false;


        private Dictionary<string, string[]> imagePaths;

        public CleanMain()
        {
            InitializeComponent();
            string loggerName = GetType().ToString();
            LogFactory.StartLog(loggerName, string.Format("\\{0}.log", loggerName));
            logger = LogFactory.GetLog(loggerName, loggerName);
            string loggerErrName = string.Format("\\{0}_err", loggerName);
            LogFactory.StartLog(loggerErrName, string.Format("\\{0}_err.log", loggerName));
            loggerErr = LogFactory.GetLog(loggerErrName, loggerErrName);
            dbManager = new DatabaseManager();
            //获取数据库剩余空间
            //decimal freeSpace = dbManager.GetDatabaseFreeSpace();
            //获取数据库的总空间
            //decimal totalSpace = dbManager.GetDatabaseTotalSpace();
            decimal DatabaseSpace=dbManager.GetDatabaseSpace();
            



            imagePaths = new Dictionary<string, string[]>
            {
                { "red", new string[] { "red.png", "red1.png", "red2.png", "red3.png", "red4.png" } },
                { "yellow", new string[] { "yellow.png", "yellow1.png", "yellow2.png", "yellow3.png", "yellow4.png" } },
                { "orange", new string[] { "Center0.png", "orange1.png", "orange2.png", "orange3.png", "orange4.png" } },
                { "blue", new string[] { "blue.png" , "blue1.png", "blue2.png", "blue3.png", "blue4.png" } },
                { "green", new string[] { "green.png", "Out1.png", "Out2.png", "Out3.png", "Out4.png" } }
            };


            if (DatabaseSpace > 0)
            {
                percentage = 100 - (DatabaseSpace / 11264) * 100;
                string PercentageString = percentage.ToString("0.00") + "%";
                DataBaseMargin.Text = PercentageString;
                //UpdateBackgroundImage();
                //UpdateColorLoad();
            }
            else
            {
                // 处理 totalSpace 为 0 的情况，以避免除以零错误
                DataBaseMargin.Text = "N/A";
            }
            // 设置窗口的初始位置
            var workingArea = System.Windows.SystemParameters.WorkArea;
            Left = workingArea.Right - Width;
            Top = (workingArea.Height - Height) / 2;
            CheckAppRunningAndUpdateButton();
            StartProgressUpdateTimer();
            UpdateBackgroundImage();
        }

        /// <summary>
        ///修改圆球的颜色 
        /// </summary>
        private void UpdateBackgroundImage()
        {
            string colorKey;

            if (percentage < 20)
            {
                colorKey = "red";
            }
            else if (percentage < 40)
            {
                colorKey = "yellow";
            }
            else if (percentage < 60)
            {
                colorKey = "orange";
            }
            else if (percentage < 80)
            {
                colorKey = "blue";
            }
            else
            {
                colorKey = "green";
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

        ////修改圈外的颜色
        //private void UpdateBackgroundImage()
        //{
        //    string colorKey;

        //    if (percentage < 20)
        //    {
        //        colorKey = "red";
        //    }
        //    else if (percentage < 40)
        //    {
        //        colorKey = "yellow";
        //    }
        //    else if (percentage < 60)
        //    {
        //        colorKey = "orange";
        //    }
        //    else if (percentage < 80)
        //    {
        //        colorKey = "blue";
        //    }
        //    else
        //    {
        //        colorKey = "green";
        //    }

        //    // 检查字典中是否包含键
        //    if (imagePaths.ContainsKey(colorKey))
        //    {
        //        // 获取指定颜色的图像路径
        //        string[] imagePathsForColor = imagePaths[colorKey];

        //        // 为ColorImage1 Grid中的每个Image元素设置图像源
        //        ColorImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resource/Image/" + imagePathsForColor[0]));
        //        ColorImage1.Source = new BitmapImage(new Uri("pack://application:,,,/Resource/Image/" + imagePathsForColor[1]));
        //        ColorImage2.Source = new BitmapImage(new Uri("pack://application:,,,/Resource/Image/" + imagePathsForColor[2]));
        //        ColorImage3.Source = new BitmapImage(new Uri("pack://application:,,,/Resource/Image/" + imagePathsForColor[3]));
        //        ColorImage4.Source = new BitmapImage(new Uri("pack://application:,,,/Resource/Image/" + imagePathsForColor[4]));
        //    }
        //    else
        //    {
        //        Console.WriteLine("找不到键: " + colorKey);
        //    }
        //}

        ///// <summary>
        ///// 页面加载时根据余量变换样式
        ///// </summary>
        //private void UpdateColorLoad()
        //{
        //    if (percentage < 20)
        //    {
        //        MarginBtn.Background = Brushes.Transparent;
        //        Margin.Foreground = new SolidColorBrush(Color.FromArgb(234, 230, 36, 36));
        //        //MarginBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(234, 230, 36, 36));

        //        Messege_MQ.Background = Brushes.Transparent;
        //        MQ.Foreground = new SolidColorBrush(Color.FromArgb(234, 230, 36, 36));
        //        //Messege_MQ.BorderBrush = new SolidColorBrush(Color.FromArgb(234, 230, 36, 36));

        //        Messege_DataClean.Background = Brushes.Transparent;
        //        DataClean.Foreground = new SolidColorBrush(Color.FromArgb(234, 230, 36, 36));
        //        //Messege_DataClean.BorderBrush = new SolidColorBrush(Color.FromArgb(234, 230, 36, 36));

        //        BackupDataBase.Background = Brushes.Transparent;
        //        BackupDataBase.Foreground = new SolidColorBrush(Color.FromArgb(234, 230, 36, 36));
        //        //BackupDataBaseBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(234, 230, 36, 36));

        //        Undetermined.Background = Brushes.Transparent;
        //        Undetermined.Foreground = new SolidColorBrush(Color.FromArgb(234, 230, 36, 36));
        //        //UndeterminedBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(234, 230, 36, 36));
        //    }
        //    else if (percentage < 40)
        //    {
        //        MarginBtn.Background = Brushes.Transparent;
        //        Margin.Foreground = new SolidColorBrush(Color.FromArgb(255, 238, 228, 37));
        //        //MarginBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 238, 228, 37));

        //        Messege_MQ.Background = Brushes.Transparent;
        //        MQ.Foreground = new SolidColorBrush(Color.FromArgb(255, 238, 228, 37));
        //        //Messege_MQ.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 238, 228, 37));

        //        Messege_DataClean.Background = Brushes.Transparent;
        //        DataClean.Foreground = new SolidColorBrush(Color.FromArgb(255, 238, 228, 37));
        //       //Messege_DataClean.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 238, 228, 37));

        //        BackupDataBase.Background = Brushes.Transparent;
        //        BackupDataBase.Foreground = new SolidColorBrush(Color.FromArgb(255, 238, 228, 37));
        //        //BackupDataBaseBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 238, 228, 37));

        //        Undetermined.Background = Brushes.Transparent;
        //        Undetermined.Foreground = new SolidColorBrush(Color.FromArgb(255, 238, 228, 37));
        //        //UndeterminedBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 238, 228, 37));
        //    }
        //    else if (percentage < 60)
        //    {
        //        MarginBtn.Background = Brushes.Transparent;
        //        Margin.Foreground = new SolidColorBrush(Color.FromArgb(255, 241, 171, 38));
        //        //MarginBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 241, 171, 38));

        //        Messege_MQ.Background = Brushes.Transparent;
        //        MQ.Foreground = new SolidColorBrush(Color.FromArgb(255, 241, 171, 38));
        //        //Messege_MQ.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 241, 171, 38));

        //        Messege_DataClean.Background = Brushes.Transparent;
        //        DataClean.Foreground = new SolidColorBrush(Color.FromArgb(255, 241, 171, 38));
        //        //Messege_DataClean.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 241, 171, 38));

        //        BackupDataBase.Background = Brushes.Transparent;
        //        BackupDataBase.Foreground = new SolidColorBrush(Color.FromArgb(255, 241, 171, 38));
        //        //BackupDataBaseBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 241, 171, 38));

        //        Undetermined.Background = Brushes.Transparent;
        //        Undetermined.Foreground = new SolidColorBrush(Color.FromArgb(255, 241, 171, 38));
        //        //UndeterminedBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 241, 171, 38));
        //    }
        //    else if (percentage < 80)
        //    {
        //        MarginBtn.Background = Brushes.Transparent;
        //        Margin.Foreground = new SolidColorBrush(Color.FromArgb(255, 48, 106, 207));
        //        //MarginBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 48, 106, 207));

        //        Messege_MQ.Background = Brushes.Transparent;
        //        MQ.Foreground = new SolidColorBrush(Color.FromArgb(255, 48, 106, 207));
        //        //Messege_MQ.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 48, 106, 207));

        //        Messege_DataClean.Background = Brushes.Transparent;
        //        DataClean.Foreground = new SolidColorBrush(Color.FromArgb(255, 48, 106, 207));
        //        //Messege_DataClean.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 48, 106, 207));

        //        BackupDataBase.Background = Brushes.Transparent;
        //        BackupDataBase.Foreground = new SolidColorBrush(Color.FromArgb(255, 48, 106, 207));
        //        //BackupDataBaseBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 48, 106, 207));

        //        Undetermined.Background = Brushes.Transparent;
        //        Undetermined.Foreground = new SolidColorBrush(Color.FromArgb(255, 48, 106, 207));
        //        //UndeterminedBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 48, 106, 207));
        //    }
        //    else
        //    {
        //        MarginBtn.Background = Brushes.Transparent;
        //        Margin.Foreground = new SolidColorBrush(Color.FromArgb(255, 64, 224, 64));
        //        //MarginBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 64, 224, 64));

        //        Messege_MQ.Background = Brushes.Transparent;
        //        MQ.Foreground = new SolidColorBrush(Color.FromArgb(255, 64, 224, 64));
        //       //Messege_MQ.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 64, 224, 64));

        //        Messege_DataClean.Background = Brushes.Transparent;
        //        DataClean.Foreground = new SolidColorBrush(Color.FromArgb(255, 64, 224, 64));
        //        //Messege_DataClean.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 64, 224, 64));

        //        BackupDataBase.Background = Brushes.Transparent;
        //        BackupDataBase.Foreground = new SolidColorBrush(Color.FromArgb(255, 64, 224, 64));
        //        //BackupDataBaseBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 64, 224, 64));

        //        Undetermined.Background = Brushes.Transparent;
        //        Undetermined.Foreground = new SolidColorBrush(Color.FromArgb(255, 64, 224, 64));
        //        //UndeterminedBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 64, 224, 64));
        //    }
        //}
        ///// <summary>
        ///// 当鼠标悬停时样式
        ///// </summary>
        //private void UpdateColorEnter()
        //{

        //    if (percentage < 20)
        //    {
        //        if (MarginBtnEnter)
        //        {
        //            MarginBtn.Background = new SolidColorBrush(Color.FromArgb(234, 230, 36, 36));
        //            Margin.Foreground = Brushes.White;
        //            MarginBtnEnter = false;
        //        }
        //        else if (MQBtnEnter)
        //        {
        //            Messege_MQ.Background = new SolidColorBrush(Color.FromArgb(234, 230, 36, 36));
        //            MQ.Foreground = Brushes.White;
        //            MQBtnEnter = false;
        //        }
        //        else if (DatacleanBtnEnter)
        //        {
        //            Messege_DataClean.Background = new SolidColorBrush(Color.FromArgb(234, 230, 36, 36));
        //            DataClean.Foreground = Brushes.White;
        //            DatacleanBtnEnter = false;
        //        }
        //        else if (BackupDataBaseBtnEnter)
        //        {
        //            BackupDataBaseBtn.Background = new SolidColorBrush(Color.FromArgb(234, 230, 36, 36));
        //            BackupDataBase.Foreground = Brushes.White;
        //            BackupDataBaseBtnEnter = false;
        //        }
        //        else if (UndeterminedBtnEnter)
        //        {
        //            UndeterminedBtn.Background = new SolidColorBrush(Color.FromArgb(234, 230, 36, 36));
        //            Undetermined.Foreground = Brushes.White;
        //            UndeterminedBtnEnter = false;
        //        }

        //    }
        //    else if (percentage < 40)
        //    {
        //        if (MarginBtnEnter)
        //        {
        //            MarginBtn.Background = new SolidColorBrush(Color.FromArgb(255, 238, 228, 37));
        //            Margin.Foreground = Brushes.White;
        //            MarginBtnEnter = false;
        //        }
        //        else if (MQBtnEnter)
        //        {
        //            Messege_MQ.Background = new SolidColorBrush(Color.FromArgb(255, 238, 228, 37));
        //            MQ.Foreground = Brushes.White;
        //            MQBtnEnter = false;
        //        }
        //        else if (DatacleanBtnEnter)
        //        {
        //            Messege_DataClean.Background = new SolidColorBrush(Color.FromArgb(255, 238, 228, 37));
        //            DataClean.Foreground = Brushes.White;
        //            DatacleanBtnEnter = false;
        //        }
        //        else if (BackupDataBaseBtnEnter)
        //        {
        //            BackupDataBaseBtn.Background = new SolidColorBrush(Color.FromArgb(234, 238, 228, 37));
        //            BackupDataBase.Foreground = Brushes.White;
        //            BackupDataBaseBtnEnter = false;
        //        }
        //        else if (UndeterminedBtnEnter)
        //        {
        //            UndeterminedBtn.Background = new SolidColorBrush(Color.FromArgb(255, 238, 228, 37));
        //            Undetermined.Foreground = Brushes.White;
        //            UndeterminedBtnEnter = false;
        //        }
        //    }
        //    else if (percentage < 60)
        //    {
        //        if (MarginBtnEnter)
        //        {
        //            MarginBtn.Background = new SolidColorBrush(Color.FromArgb(255, 241, 171, 38));
        //            Margin.Foreground = Brushes.White;
        //            MarginBtnEnter = false;
        //        }
        //        else if (MQBtnEnter)
        //        {
        //            Messege_MQ.Background = new SolidColorBrush(Color.FromArgb(255, 241, 171, 38));
        //            MQ.Foreground = Brushes.White;
        //            MQBtnEnter = false;
        //        }
        //        else if (DatacleanBtnEnter)
        //        {
        //            Messege_DataClean.Background = new SolidColorBrush(Color.FromArgb(255, 241, 171, 38));
        //            DataClean.Foreground = Brushes.White;
        //            DatacleanBtnEnter = false;
        //        }
        //        else if (BackupDataBaseBtnEnter)
        //        {
        //            BackupDataBaseBtn.Background = new SolidColorBrush(Color.FromArgb(255, 241, 171, 38));
        //            BackupDataBase.Foreground = Brushes.White;
        //            BackupDataBaseBtnEnter = false;
        //        }
        //        else if (UndeterminedBtnEnter)
        //        {
        //            UndeterminedBtn.Background = new SolidColorBrush(Color.FromArgb(255, 241, 171, 38));
        //            Undetermined.Foreground = Brushes.White;
        //            UndeterminedBtnEnter = false;
        //        }
        //    }
        //    else if (percentage < 80)
        //    {
        //        if (MarginBtnEnter)
        //        {
        //            MarginBtn.Background = new SolidColorBrush(Color.FromArgb(255, 48, 106, 207));
        //            Margin.Foreground = Brushes.White;
        //            MarginBtnEnter = false;
        //        }
        //        else if (MQBtnEnter)
        //        {
        //            Messege_MQ.Background = new SolidColorBrush(Color.FromArgb(255, 48, 106, 207));
        //            MQ.Foreground = Brushes.White;
        //            MQBtnEnter = false;
        //        }
        //        else if (DatacleanBtnEnter)
        //        {
        //            Messege_DataClean.Background = new SolidColorBrush(Color.FromArgb(255, 48, 106, 207));
        //            DataClean.Foreground = Brushes.White;
        //            DatacleanBtnEnter = false;
        //        }
        //        else if (BackupDataBaseBtnEnter)
        //        {
        //            BackupDataBaseBtn.Background = new SolidColorBrush(Color.FromArgb(255, 48, 106, 207));
        //            BackupDataBase.Foreground = Brushes.White;
        //            BackupDataBaseBtnEnter = false;
        //        }
        //        else if (UndeterminedBtnEnter)
        //        {
        //            UndeterminedBtn.Background = new SolidColorBrush(Color.FromArgb(255, 48, 106, 207));
        //            Undetermined.Foreground = Brushes.White;
        //            UndeterminedBtnEnter = false;
        //        }
        //    }
        //    else
        //    {
        //        if (MarginBtnEnter)
        //        {
        //            MarginBtn.Background = new SolidColorBrush(Color.FromArgb(255, 64, 224, 64));
        //            Margin.Foreground = Brushes.White;
        //            MarginBtnEnter = false;
        //        }
        //        else if (MQBtnEnter)
        //        {
        //            Messege_MQ.Background = new SolidColorBrush(Color.FromArgb(255, 64, 224, 64));
        //            MQ.Foreground = Brushes.White;
        //            MQBtnEnter = false;
        //        }
        //        else if (DatacleanBtnEnter)
        //        {
        //            Messege_DataClean.Background = new SolidColorBrush(Color.FromArgb(255, 64, 224, 64));
        //            DataClean.Foreground = Brushes.White;
        //            DatacleanBtnEnter = false;
        //        }
        //        else if (BackupDataBaseBtnEnter)
        //        {
        //            BackupDataBaseBtn.Background = new SolidColorBrush(Color.FromArgb(255, 64, 224, 64));
        //            BackupDataBase.Foreground = Brushes.White;
        //            BackupDataBaseBtnEnter = false;
        //        }
        //        else if (UndeterminedBtnEnter)
        //        {
        //            UndeterminedBtn.Background = new SolidColorBrush(Color.FromArgb(255, 64, 224, 64));
        //            Undetermined.Foreground = Brushes.White;
        //            UndeterminedBtnEnter = false;
        //        }
        //    }
        //}
        ///// <summary>
        ///// 鼠标离开时样式
        ///// </summary>
        //private void UpdateColorLeave()
        //{

        //    if (percentage < 20)
        //    {
        //        if (MarginBtnLeave)
        //        {
        //            MarginBtn.Background = Brushes.Transparent;
        //            Margin.Foreground = new SolidColorBrush(Color.FromArgb(234, 230, 36, 36));
        //            //MarginBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(234, 230, 36, 36));
        //            MarginBtnLeave = false;
        //        }
        //        else if (MQBtnLeave)
        //        {
        //            Messege_MQ.Background = Brushes.Transparent;
        //            MQ.Foreground = new SolidColorBrush(Color.FromArgb(234, 230, 36, 36));
        //            //Messege_MQ.BorderBrush = new SolidColorBrush(Color.FromArgb(234, 230, 36, 36));
        //            MQBtnLeave = false;
        //        }
        //        else if (DatacleanBtnLeave)
        //        {
        //            Messege_DataClean.Background = Brushes.Transparent;
        //            DataClean.Foreground = new SolidColorBrush(Color.FromArgb(234, 230, 36, 36));
        //            //Messege_DataClean.BorderBrush = new SolidColorBrush(Color.FromArgb(234, 230, 36, 36));
        //            DatacleanBtnLeave = false;
        //        }
        //        else if (BackupDataBaseBtnLeave)
        //        {
        //            BackupDataBaseBtn.Background = Brushes.Transparent;
        //            BackupDataBase.Foreground = new SolidColorBrush(Color.FromArgb(234, 230, 36, 36));
        //            //BackupDataBaseBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(234, 230, 36, 36));
        //            BackupDataBaseBtnLeave = false;
        //        }
        //        else if (UndeterminedBtnLeave)
        //        {
        //            UndeterminedBtn.Background = Brushes.Transparent;
        //            Undetermined.Foreground = new SolidColorBrush(Color.FromArgb(234, 230, 36, 36));
        //            //UndeterminedBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(234, 230, 36, 36));
        //            UndeterminedBtnLeave = false;
        //        }
        //    }
        //    else if (percentage < 40)
        //    {
        //        if (MarginBtnLeave)
        //        {
        //            MarginBtn.Background = Brushes.Transparent;
        //            Margin.Foreground = new SolidColorBrush(Color.FromArgb(255, 238, 228, 37));
        //            //MarginBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 238, 228, 37));
        //            MarginBtnLeave = false;
        //        }
        //        else if (MQBtnLeave)
        //        {
        //            Messege_MQ.Background = Brushes.Transparent;
        //            MQ.Foreground = new SolidColorBrush(Color.FromArgb(255, 238, 228, 37));
        //            //Messege_MQ.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 238, 228, 37));
        //            MQBtnLeave = false;
        //        }
        //        else if (DatacleanBtnLeave)
        //        {
        //            Messege_DataClean.Background = Brushes.Transparent;
        //            DataClean.Foreground = new SolidColorBrush(Color.FromArgb(255, 238, 228, 37));
        //            //Messege_DataClean.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 238, 228, 37));
        //            DatacleanBtnLeave = false;
        //        }
        //        else if (BackupDataBaseBtnLeave)
        //        {
        //            BackupDataBaseBtn.Background = Brushes.Transparent;
        //            BackupDataBase.Foreground = new SolidColorBrush(Color.FromArgb(255, 238, 228, 37));
        //            //BackupDataBaseBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 238, 228, 37));
        //            BackupDataBaseBtnLeave = false;
        //        }
        //        else if (UndeterminedBtnLeave)
        //        {
        //            UndeterminedBtn.Background = Brushes.Transparent;
        //            Undetermined.Foreground = new SolidColorBrush(Color.FromArgb(255, 238, 228, 37));
        //           //UndeterminedBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 238, 228, 37));
        //            UndeterminedBtnLeave = false;
        //        }
        //    }
        //    else if (percentage < 60)
        //    {
        //        if (MarginBtnLeave)
        //        {
        //            MarginBtn.Background = Brushes.Transparent;
        //            Margin.Foreground = new SolidColorBrush(Color.FromArgb(255, 241, 171, 38));
        //            //MarginBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 241, 171, 38));
        //            MarginBtnLeave = false;
        //        }
        //        else if (MQBtnLeave)
        //        {
        //            Messege_MQ.Background = Brushes.Transparent;
        //            MQ.Foreground = new SolidColorBrush(Color.FromArgb(255, 241, 171, 38));
        //            //Messege_MQ.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 241, 171, 38));
        //            MQBtnLeave = false;
        //        }
        //        else if (DatacleanBtnLeave)
        //        {
        //            Messege_DataClean.Background = Brushes.Transparent;
        //            DataClean.Foreground = new SolidColorBrush(Color.FromArgb(255, 241, 171, 38));
        //           //Messege_DataClean.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 241, 171, 38));
        //            DatacleanBtnLeave = false;
        //        }
        //        else if (BackupDataBaseBtnLeave)
        //        {
        //            BackupDataBaseBtn.Background = Brushes.Transparent;
        //            BackupDataBase.Foreground = new SolidColorBrush(Color.FromArgb(255, 241, 171, 38));
        //            //BackupDataBaseBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 241, 171, 38));
        //            BackupDataBaseBtnLeave = false;
        //        }
        //        else if (UndeterminedBtnLeave)
        //        {
        //            UndeterminedBtn.Background = Brushes.Transparent;
        //            Undetermined.Foreground = new SolidColorBrush(Color.FromArgb(255, 241, 171, 38));
        //            //UndeterminedBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 241, 171, 38));
        //            UndeterminedBtnLeave = false;
        //        }
        //    }
        //    else if (percentage < 80)
        //    {
        //        if (MarginBtnLeave)
        //        {
        //            MarginBtn.Background = Brushes.Transparent;
        //            Margin.Foreground = new SolidColorBrush(Color.FromArgb(255, 48, 106, 207));
        //            //MarginBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 48, 106, 207));
        //            MarginBtnLeave = false;
        //        }
        //        else if (MQBtnLeave)
        //        {
        //            Messege_MQ.Background = Brushes.Transparent;
        //            MQ.Foreground = new SolidColorBrush(Color.FromArgb(255, 48, 106, 207));
        //            //Messege_MQ.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 48, 106, 207));
        //            MQBtnLeave = false;
        //        }
        //        else if (DatacleanBtnLeave)
        //        {
        //            Messege_DataClean.Background = Brushes.Transparent;
        //            DataClean.Foreground = new SolidColorBrush(Color.FromArgb(255, 48, 106, 207));
        //            //Messege_DataClean.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 48, 106, 207));
        //            DatacleanBtnLeave = false;
        //        }
        //        else if (BackupDataBaseBtnLeave)
        //        {
        //            BackupDataBaseBtn.Background = Brushes.Transparent;
        //            BackupDataBase.Foreground = new SolidColorBrush(Color.FromArgb(255, 48, 106, 207));
        //            //BackupDataBaseBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 48, 106, 207));
        //            BackupDataBaseBtnLeave = false;
        //        }
        //        else if (UndeterminedBtnLeave)
        //        {
        //            UndeterminedBtn.Background = Brushes.Transparent;
        //            Undetermined.Foreground = new SolidColorBrush(Color.FromArgb(255, 48, 106, 207));
        //            //UndeterminedBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 48, 106, 207));
        //            UndeterminedBtnLeave = false;
        //        }
        //    }
        //    else
        //    {
        //        if (MarginBtnLeave)
        //        {
        //            MarginBtn.Background = Brushes.Transparent;
        //            Margin.Foreground = new SolidColorBrush(Color.FromArgb(255, 64, 224, 64));
        //            //MarginBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 64, 224, 64));
        //            MarginBtnLeave = false;
        //        }
        //        else if (MQBtnLeave)
        //        {
        //            Messege_MQ.Background = Brushes.Transparent;
        //            MQ.Foreground = new SolidColorBrush(Color.FromArgb(255, 64, 224, 64));
        //            //Messege_MQ.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 64, 224, 64));
        //            MQBtnLeave = false;
        //        }
        //        else if (DatacleanBtnLeave)
        //        {
        //            Messege_DataClean.Background = Brushes.Transparent;
        //            DataClean.Foreground = new SolidColorBrush(Color.FromArgb(255, 64, 224, 64));
        //            //Messege_DataClean.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 64, 224, 64));
        //            DatacleanBtnLeave = false;
        //        }
        //        else if (BackupDataBaseBtnLeave)
        //        {
        //            BackupDataBaseBtn.Background = Brushes.Transparent;
        //            BackupDataBase.Foreground = new SolidColorBrush(Color.FromArgb(255, 64, 224, 64));
        //            //BackupDataBaseBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 64, 224, 64));
        //            BackupDataBaseBtnLeave = false;
        //        }
        //        else if (UndeterminedBtnLeave)
        //        {
        //            UndeterminedBtn.Background = Brushes.Transparent;
        //            Undetermined.Foreground = new SolidColorBrush(Color.FromArgb(255, 64, 224, 64));
        //            //UndeterminedBtn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 64, 224, 64));
        //            UndeterminedBtnLeave = false;
        //        }
        //    }
        //}
        //private string GetDatabaseTypeFromConfig()
        //{
        //    try
        //    {
        //        XmlDocument doc = new XmlDocument();
        //        doc.Load("DetectingSystemBall.exe.config");
        //        Console.WriteLine("Loaded from: " + doc.BaseURI);
        //        XmlNode node = doc.SelectSingleNode("//appSettings/add[@key='Conne_Type']");

        //        if (node != null)
        //        {
        //            // 获取数据库类型的值
        //            return node.Attributes["value"].Value.Trim();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        loggerErr.Debug("GetDatabaseTypeFromConfig -->" + ex.ToString());
        //    }

        //    return string.Empty;
        //}
        string connectionString = ConfigurationManager.AppSettings["Conne_DB"];

        //private void UpdateShrinkProgress()
        //{
        //    // 调用 DatabaseManager 中的方法获取收缩进度
        //    int shrinkProgress = dbManager.GetShrinkProgress();

        //    Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

        //    Dispatcher.Invoke(() =>
        //    {
        //        shrinkProgressBar.Value = shrinkProgress;
        //    });
        //    // 在UI中更新进度条
        //    //shrinkProgressBar.Value = shrinkProgress;
        //}

        // 使用定时器定期更新
        private void StartProgressUpdateTimer()
        {
            Timer progressTimer = new Timer(_ => dbManager.GetDatabaseSpace(), null, 0, 1000);
        }

        private async void Margin_Click(object sender, RoutedEventArgs e)
        {
            //StartProgressUpdateTimer();
            DBType databaseType = ConnAppConfig.Conne_Type;
            string databaseTypes = databaseType.ToString().Trim();
            if (!databaseType.Equals(""))
            {
                string databaseName = dbManager.GetDatabaseNameFromConnectionString(connectionString);

                List<string> tablesToShrink = dbManager.GetTablesToShrinkAbove100MB(databaseName);

                if (tablesToShrink.Count > 0)
                {
                    // 显示进度条
                    shrinkProgressBar.Visibility = Visibility.Visible; 

                    // 使用 Progress<int> 进行进度报告
                    var progress = new Progress<int>(value =>
                    {
                        // 更新进度条
                        shrinkProgressBar.Value += value; 
                    });

                    // 根据数据库类型调用相应的表收缩方法
                    if (databaseTypes.Equals("ORACLE", StringComparison.OrdinalIgnoreCase))
                    {
                        await Task.Run(() =>
                        {
                            dbManager.ShrinkOracleTables(tablesToShrink, progress);
                        });
                        // 收缩完成之后隐藏进度条
                        shrinkProgressBar.Visibility = Visibility.Hidden;
                        MessageBox.Show("数据库表收缩成功。");

                    }
                    else if (databaseTypes.Equals("SQLSERVER", StringComparison.OrdinalIgnoreCase))
                    {
                        await Task.Run(() =>
                        {
                            dbManager.ShrinkSqlServerTables(tablesToShrink);
                        });
                    }
                }
                else
                {
                    MessageBox.Show("没有找到大于100MB的可收缩的表。");
                }
            }
            else
            {
                MessageBox.Show("数据库表收缩失败。");
            }
        }


        private void MarginBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            //    MarginBtnEnter = true;
            //    UpdateColorEnter();

        }

        private void MarginBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            //    MarginBtnLeave = true;
            //    UpdateColorLeave();
        }

        private void MQ_MouseEnter(object sender, MouseEventArgs e)
        {
            //MQBtnEnter = true;
            //UpdateColorEnter();
        }

        private void MQ_MouseLeave(object sender, MouseEventArgs e)
        {
            //MQBtnLeave = true;
            //UpdateColorLeave();
        }

        private void DataClean_MouseEnter(object sender, MouseEventArgs e)
        {
            //DatacleanBtnEnter = true;
            //UpdateColorEnter();
        }

        private void DataClean_MouseLeave(object sender, MouseEventArgs e)
        {
            //DatacleanBtnLeave = true;
            //UpdateColorLeave();
        }

        private void BackupDataBase_MouseEnter(object sender, MouseEventArgs e)
        {
            //BackupDataBaseBtnEnter = true;
            //UpdateColorEnter();
        }

        private void BackupDataBase_MouseLeave(object sender, MouseEventArgs e)
        {
            //BackupDataBaseBtnLeave = true;
            //UpdateColorLeave();
        }

        private void UndeterminedBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            //UndeterminedBtnEnter = true;
            //UpdateColorEnter();
        }

        private void UndeterminedBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            //UndeterminedBtnLeave = true;
            //UpdateColorLeave();
        }

        //读取应用路径
        private string GetProcessExecutablePath(string processName)
        {
            string executablePath = string.Empty;
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length > 0)
            {
                executablePath = processes[0].MainModule.FileName;
            }
            return executablePath;
        }

        private string SanitizePath(string path)
        {
            char[] invalidChars = Path.GetInvalidPathChars();
            return new string(path.Where(c => !invalidChars.Contains(c)).ToArray());
        }

        private string GetLogFolderPath(string executablePath)
        {
            string sanitizedExecutablePath = SanitizePath(executablePath);

            string executableFolderPath = Path.GetDirectoryName(sanitizedExecutablePath);
            string logFolderPath = Path.Combine(executableFolderPath, "Log");
            return logFolderPath;
        }


        private string GetLogFilePath(string logFolderPath, string logFileName)
        {
            string logFilePath = Path.Combine(logFolderPath, logFileName);
            return logFilePath;
        }

        /// <summary>
        /// 查询服务是否启动
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        private bool IsServiceRunning(string serviceName)
        {
            ServiceController sc = new ServiceController(serviceName);

            try
            {
                return sc.Status == ServiceControllerStatus.Running;
            }
            catch (Exception ex)
            {
                loggerErr.Error(ex.ToString());
                System.Console.WriteLine(ex.Message);
                return false;
            }
        }

        private void CheckAppRunningAndUpdateButton()
        {
            bool isMQRunning = false;
            bool isDataCleanRunning = IsServiceRunning("iPlant.DataCleanService");
            string errorCodeMessage = "";

            string mqExecutablePath = GetProcessExecutablePath("ACC.MQ");

            if (!string.IsNullOrEmpty(mqExecutablePath))
            {
                isMQRunning = true;
                string mqLogFolderPath = GetLogFolderPath(mqExecutablePath);
                string mqLogFilePath = GetLogFilePath(mqLogFolderPath, "message.log");
                List<string> mqErrorMessages = ReadLogFileForErrors(mqLogFilePath);

                if (mqErrorMessages.Count > 0)
                {
                    errorCodeMessage = "MQ Error Messages:\n" + string.Join(Environment.NewLine, mqErrorMessages);
                }
            }

            if (isDataCleanRunning)
            {
                string dataCleanExecutablePath = GetServiceExecutablePath("iPlant.DataCleanService");
                string dataCleanLogFolderPath = GetLogFolderPath(dataCleanExecutablePath);
                string[] logFileNames = new string[] { "StartUpLog.log", "CleanLog.log" };

                foreach (string logFileName in logFileNames)
                {
                    string dataCleanLogFilePath = GetLogFilePath(dataCleanLogFolderPath, logFileName);

                    if (File.Exists(dataCleanLogFilePath))
                    {
                        List<string> dataCleanErrorMessages = ReadLogFileForErrors(dataCleanLogFilePath);

                        if (dataCleanErrorMessages.Count > 0)
                        {
                            errorCodeMessage = "DataClean Error Messages:\n" + string.Join(Environment.NewLine, dataCleanErrorMessages);
                            break;
                        }
                    }
                    else
                    {
                        MessageBox.Show(dataCleanLogFilePath + "不存在");
                    }
                }
            }

            UpdateButtonDisplay(isMQRunning, isDataCleanRunning, errorCodeMessage);
        }


        private string GetServiceExecutablePath(string serviceName)
        {
            string executablePath = string.Empty;
            string query = $"SELECT PathName FROM Win32_Service WHERE Name = '{serviceName}'";

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            using (ManagementObjectCollection services = searcher.Get())
            {
                foreach (ManagementObject service in services)
                {
                    if (service["PathName"] != null)
                    {
                        executablePath = service["PathName"].ToString();
                        string[] parts = executablePath.Split(' ');
                        if (parts.Length > 0)
                        {
                            executablePath = parts[0];
                        }
                        break;
                    }
                }
            }

            return executablePath;
        }

        /// <summary>
        /// 检测错误日志，只检测当天的错误日志
        /// </summary>
        /// <param name="logFilePath"></param>
        /// <returns></returns>
        private List<string> ReadLogFileForErrors(string logFilePath)
        {
            List<string> errorMessages = new List<string>();
            // 获取当天日期的字符串表示，格式为"yyyy-MM-dd"
            todayDateString = DateTime.Now.ToString("yyyy-MM-dd");

            try
            {
                using (FileStream fileStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader sr = new StreamReader(fileStream, System.Text.Encoding.Default))
                {
                    string[] errorKeywords = new string[] { "ORA-", "Erro", "Fail", "CheckError", "fail", "erro" };
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Contains(todayDateString))
                        {
                            foreach (string keyword in errorKeywords)
                            {
                                if (line.Contains(keyword))
                                {
                                    errorMessages.Add(line);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (IOException ie)
            {
                loggerErr.Error(ie.ToString());
                System.Console.WriteLine(ie.Message);
            }

            return errorMessages;
        }



        private void UpdateButtonDisplay(bool isMQRunning, bool isDataCleanRunning, string ErrorCodeMessage)
        {
            // 更新MQ按钮的显示状态
            if (isMQRunning)
            {
                MQ.IsEnabled = false;
                if (ErrorCodeMessage == "")
                {
                    MQ.Text = "MQ\nMQ is Running";
                }
                else
                {
                    MQ.Text = "MQ\nMQ状态异常，请联系ACC管理员处理！";
                }


            }
            else
            {
                MQ.Text = "MQ\nMQ no Start";
            }

            // 更新DataClean按钮的显示状态
            if (isDataCleanRunning)
            {

                if (ErrorCodeMessage == "")
                {
                    DataClean.Text = "DataClean\nDataClean is Running";
                }
                else
                {
                    DataClean.Text = "DataClean\nDataClean 状态异常，请联系ACC管理员处理！";
                }
            }
            else
            {
                DataClean.Text = "DataClean no Start";
            }
        }


        private void BackupDataBaseBtn_Click(object sender, RoutedEventArgs e)
        {
            string ShirkNameData = DateTime.Now.ToString("yyyyMMdd");
            try
            {
                string connectionString = ConnAppConfig.DB_Conn;
                string DataSource = dbManager.GetDataSourceFromConnectionString(connectionString);
                string databaseName = dbManager.GetDatabaseNameFromConnectionString(connectionString);
                string password = dbManager.GetPasswordFromConnectionString(connectionString);
                string backupFilePath = ConnAppConfig.DIRECTORY;
                string DUMPFile = ConnAppConfig.DUMPFile;

                // 检查是否已存在同名备份文件
                string existingBackupFilePath = Path.Combine(DUMPFile, $"{databaseName}" + ShirkNameData + ".dmp");
                if (File.Exists(existingBackupFilePath))
                {
                    // 提示用户确认是否替换
                    MessageBoxResult result = MessageBox.Show("同名备份文件已存在，是否替换？", "替换确认", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                    {
                        return;
                    }
                    else
                    {
                        // 如果用户同意替换，删除现有文件
                        File.Delete(existingBackupFilePath);
                    }
                }

                // 在新线程中执行导出操作
                var exportThread = new Thread(() =>
                {
                    string exportCommand = $"expdp {databaseName}/{password}@{DataSource} FULL=Y DIRECTORY={backupFilePath} DUMPFILE={databaseName}" + ShirkNameData + ".dmp LOGFILE=backup.log";
                    Console.WriteLine("Export Command: " + exportCommand);
                    Process process = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe", "/c " + exportCommand);
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();
                    if (process.ExitCode == 0)
                    {
                        this.Dispatcher.Invoke(new Action(delegate
                        {
                            MessageBox.Show("数据库备份成功!");

                        }));
                    }
                    process.Close();

                });
                exportThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据库备份失败：" + ex.Message);
            }
        }
    }
}
