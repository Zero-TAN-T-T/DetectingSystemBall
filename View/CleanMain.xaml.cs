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
using System.Windows.Controls;

namespace DetectingSystemBall.View
{
    /// <summary>
    /// CleanMain.xaml 的交互逻辑
    /// </summary>
    public partial class CleanMain : Window
    {
        private DatabaseManager dbManager;

        private decimal _dataBaseMargin;
        public decimal DataBaseMarginClean
        {
            get { return _dataBaseMargin; }
            set
            {
                if (_dataBaseMargin != value)
                {
                    _dataBaseMargin = value;
                }
            }
        }

        //根据警戒值显示对应的圆球颜色
        //public decimal percentage;
        private Dictionary<string, string[]> imagePaths;

        //日志对象
        protected MyLog logger;

        //日志对象
        protected MyLog loggerErr;

        //获取当前时间,用于检测当天的数据是否有报错
        public string todayDateString;

        //保存要收缩的前五张表
        private List<string> tablesToShrink;

        private Timer progressTimer;

        public CleanMain()
        {
            InitializeComponent();
            DataContext = this;
            InitializeLogger();
            //InitializeDatabaseManager();

            //Task<decimal> getDatabaseSpaceTask = dbManager.GetDatabaseSpaceAsync();
            //getDatabaseSpaceTask.ContinueWith(task =>
            //{
            

            tablesToShrink = new List<string>();

                imagePaths = new Dictionary<string, string[]>
            {
                { "red", new string[] { "red.png", "red1.png", "red2.png", "red3.png", "red4.png" } },
                { "yellow", new string[] { "yellow.png", "yellow1.png", "yellow2.png", "yellow3.png", "yellow4.png" } },
                { "orange", new string[] { "Center0.png", "orange1.png", "orange2.png", "orange3.png", "orange4.png" } },
                { "blue", new string[] { "blue.png" , "blue1.png", "blue2.png", "blue3.png", "blue4.png" } },
                { "green", new string[] { "green.png", "Out1.png", "Out2.png", "Out3.png", "Out4.png" } }
            };



                
                // 设置窗口的初始位置
                var workingArea = System.Windows.SystemParameters.WorkArea;
                Left = workingArea.Right - Width;
                Top = (workingArea.Height - Height) / 2;
                CheckAppRunningAndUpdateButton();
                UpdateBackgroundImage();
            //}, TaskScheduler.FromCurrentSynchronizationContext());
        }

        

        /// <summary>
        /// 生成日志文件
        /// </summary>
        private void InitializeLogger()
        {
            string loggerName = GetType().ToString();
            LogFactory.StartLog(loggerName, $@"\{loggerName}.log");
            logger = LogFactory.GetLog(loggerName, loggerName);

            string loggerErrName = $@"\{loggerName}_err";
            LogFactory.StartLog(loggerErrName, $@"\{loggerName}_err.log");
            loggerErr = LogFactory.GetLog(loggerErrName, loggerErrName);
        }

        /// <summary>
        ///修改圆球的颜色 
        /// </summary>
        private void UpdateBackgroundImage()
        {
            string colorKey;
            FloatBallWindow floatBallWindow = new FloatBallWindow();
            DataBaseMarginClean = floatBallWindow.percentage;

            if (DataBaseMarginClean >= 80)
            {
                colorKey = "green";
            }
            else if (DataBaseMarginClean >= 60)
            {
                colorKey = "blue";
            }
            else if (DataBaseMarginClean >= 40)
            {
                colorKey = "orange";
            }
            else if (DataBaseMarginClean >= 20)
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

        string connectionString = ConfigurationManager.AppSettings["Conne_DB"];

        private async void Margin_Click(object sender, RoutedEventArgs e)
        {
            //获取数据库的连接类型，以此来判断该执行哪个数据库的收缩语句
            DBType databaseType = ConnAppConfig.Conne_Type;
            string databaseTypes = databaseType.ToString().Trim();
            try
            {
                //如果数据库类型不为空就获取数据库用户名的id,查询数据库中大于100MB的前五张表
                if (!databaseType.Equals(""))
                {
                    string databaseName = dbManager.GetDatabaseNameFromConnectionString(connectionString);
                    List<string> tablesToShrink = dbManager.GetTablesToShrinkAbove100MB(databaseName); // 初始化 tablesToShrink

                    if (tablesToShrink.Count > 0)
                    {
                        // 根据数据库类型调用相应的表收缩方法
                        if (databaseTypes.Equals("ORACLE", StringComparison.OrdinalIgnoreCase))
                        {
                            // 获取要收缩的前五张表名
                            if (tablesToShrink.Count > 0)
                            {
                                // 显示前五张表名弹框
                                ShowTablesToShrinkDialog(tablesToShrink);

                                // 显示进度条
                                shrinkProgressBar.Visibility = Visibility.Visible;

                                var progress = new Progress<int>(value =>
                                {
                                    // 更新进度条
                                    shrinkProgressBar.Value += value;
                                });

                                await Task.Run(() =>
                                {
                                    dbManager.ShrinkOracleTables(tablesToShrink, progress);
                                });

                                // 收缩完成之后隐藏进度条
                                shrinkProgressBar.Visibility = Visibility.Hidden;
                                MessageBox.Show("数据库表收缩成功。");
                            }
                            else
                            {
                                MessageBox.Show("没有找到大于100MB的可收缩的表。");
                            }
                        }
                        if (databaseTypes.Equals("SQLSERVER", StringComparison.OrdinalIgnoreCase))
                        {
                            await Task.Run(() =>
                            {
                                dbManager.ShrinkSqlServerTables(tablesToShrink);
                            });
                        }
                        else
                        {
                            MessageBox.Show("不支持的数据库类型！");
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
            catch (Exception ex)
            {
                loggerErr.Debug(ex.Message);
            }
        }

        //弹框显示前五张表名
        private void ShowTablesToShrinkDialog(List<string> tablesToShrink)
        {
            // 创建一个 TextBlock 用于显示收缩的前五张表名
            TextBlock textBlock = new TextBlock
            {
                Text = "收缩的前五张表名：\n" + string.Join("\n", tablesToShrink.GetRange(0, Math.Min(5, tablesToShrink.Count))),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(20),
                FontSize = 16,
                FontWeight = FontWeights.Bold
            };

            // 创建一个 MessageBox 显示前五张表名
            MessageBox.Show(new Window { Content = textBlock, SizeToContent = SizeToContent.WidthAndHeight }, "收缩前五张表名");
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
