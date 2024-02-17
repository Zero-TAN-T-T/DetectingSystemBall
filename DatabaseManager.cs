using iPlant.Components.Framework.DataAccess;
using iPlant.Components.Framework.Util.Enums;
using iPlant.Components.Framework.Util.Log;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Xml;
using DetectingSystemBall.Conn;
using DetectingSystemBall.Model.Oracle;
using System.Threading.Tasks;
using System.Data;

namespace DetectingSystemBall
{
    public class DatabaseManager
    {
        //获取数据库连接
        private string ConnectionString;

        //日志对象
        protected MyLog logger;

        //日志对象
        protected MyLog loggerErr;

        public DBHelper DBHelper;

        public ConnAppConfig ConnAppConfig;

        public SQLHelper SQLHelper_oracle;
        public SQLHelper SQLHelper_SQLSERVER;

        public Oracle_DBA_SEGMENTS oracle_DBA_SEGMENTS;
        public DatabaseManager()
        {
            LoadConnectionStringFromConfig();
            InitLogger();
            Init();
        }
        private void InitLogger()
        {
            string loggerName = GetType().ToString();
            LogFactory.StartLog(loggerName, $"{loggerName}.log");
            logger = LogFactory.GetLog(loggerName, loggerName);
            string loggerErrName = $"{loggerName}_err";
            LogFactory.StartLog(loggerErrName, $"{loggerName}_err.log");
            loggerErr = LogFactory.GetLog(loggerErrName, loggerErrName);
        }

        public async Task<DataTable> GetAsync(string sql, int maxRetryAttempts, TimeSpan delayBetweenRetries)
        {
            int retryCount = 0;

            while (retryCount < maxRetryAttempts)
            {
                try
                {
                    return await Task.Run(() => SQLHelper_oracle.Get(sql));
                }
                catch (Exception ex) 
                {
                    retryCount++;
                    loggerErr.Debug($"Error executing SQL query asynchronously (Retry {retryCount}/{maxRetryAttempts}): {ex}");
                    await Task.Delay(delayBetweenRetries); // 延迟一段时间后重试
                }
            }

            
            throw new Exception($"Max retry attempts ({maxRetryAttempts}) exceeded.");
        }
        public virtual void Init()
        {
            SQLHelper_oracle = new SQLHelper(ConnAppConfig.Conne_Type, ConnAppConfig.DB_Conn, false, ConnAppConfig.Conne_Type, logger);
            SQLHelper_SQLSERVER = new SQLHelper(ConnAppConfig.Conne_Type, ConnAppConfig.DB_Conn, false, ConnAppConfig.Conne_Type, logger);

        }
        string connectionString = ConnAppConfig.DB_Conn;

        /// <summary>
        /// 获取用户登录密码
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public string GetPasswordFromConnectionString(string connectionString)
        {
            string password = string.Empty;
            if (connectionString.Contains("Password="))
            {
                int startIndex = connectionString.IndexOf("Password=") + "Password=".Length;
                int endIndex = connectionString.IndexOf(";", startIndex);
                password = connectionString.Substring(startIndex, endIndex - startIndex);
            }

            return password;
        }

        /// <summary>
        /// 获取登录用户ID
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public string GetDatabaseNameFromConnectionString(string connectionString)
        {
            string databaseName = string.Empty;
            if (connectionString.Contains("User ID="))
            {
                int startIndex = connectionString.IndexOf("User ID=") + "User ID=".Length;
                int endIndex = connectionString.IndexOf(";", startIndex);
                databaseName = connectionString.Substring(startIndex, endIndex - startIndex);
            }

            return databaseName.ToUpper();
        }

        /// <summary>
        /// 获取实例名
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public string GetDataSourceFromConnectionString(string connectionString)
        {
            string dataSource = string.Empty;
            if (connectionString.Contains("/"))
            {
                int startIndex = connectionString.IndexOf("/") + "/".Length;
                int endIndex = connectionString.IndexOf(";", startIndex);
                dataSource = connectionString.Substring(startIndex, endIndex - startIndex);
            }

            return dataSource;
        }
        private void LoadConnectionStringFromConfig()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("DetectingSystemBall.exe.config");
                Console.WriteLine("Loaded from: " + doc.BaseURI);
                DBType connectionType = ConnAppConfig.Conne_Type;
                string connectionString = ConnAppConfig.DB_Conn;

                if (connectionType.Equals("") && connectionString != null)
                {
                    // 获取连接类型和连接字符串的值
                    string connectionTypes = connectionType.ToString().Trim();
                    ConnectionString = connectionString.ToString().Trim();

                    // 根据连接类型执行相应的操作
                    if (connectionTypes.Equals("ORACLE", StringComparison.OrdinalIgnoreCase))
                    {

                    }
                    else if (connectionTypes.Equals("SQLSERVER", StringComparison.OrdinalIgnoreCase))
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                loggerErr.Debug("LoadConnectionStringFromConfig -->" + ex.ToString());
            }


        }

        /// <summary>
        /// <summary>
        /// 查询占用数据库已用空间最大的前五个表单,100MB以上
        /// </summary>
        /// <returns></returns>
        public virtual List<string> GetTablesToShrinkAbove100MB(string databaseName)
        {
            try
            {

                //databaseName = GetDatabaseNameFromConnectionString(databaseName);
                var db_segments = ObjectBuilder.ToList<Oracle_DBA_SEGMENTS>(SQLHelper_oracle.Get(string.Format("SELECT OWNER, SEGMENT_NAME, SEGMENT_TYPE, BYTES / (1024 * 1024) AS MEGABYTES FROM DBA_SEGMENTS WHERE OWNER = '{0}' AND BYTES / (1024 * 1024) > 100 ORDER BY BYTES DESC", databaseName)));
                List<string> tableNames = new List<string>();
                foreach (var segment in db_segments)
                {
                    tableNames.Add(segment.SEGMENT_NAME);
                }

                return tableNames;
            }
            catch (Exception ex)
            {
                loggerErr.Debug("GetTablesToShrinkAbove100MB -->" + ex.ToString());
                return new List<string>();
            }

        }

        /// <summary>
        /// 查看Oracle视图，来跟进当前收缩进程的进度
        /// </summary>
        /// <returns></returns>
        public int GetShrinkProgress()
        {
            try
            {
                string databaseName = GetDatabaseNameFromConnectionString(connectionString);
                string shrinkProgressQuery = string.Format("SELECT ROUND(sofar / totalwork * 100, 2) AS SHRINK_PROGRESS " +
                                                           "FROM v$session_longops " +
                                                           "WHERE opname LIKE 'Tablespace%Shrink%' AND USERNAME = '{0}'", databaseName);
                object result = SQLHelper_oracle.Get(shrinkProgressQuery);

                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                loggerErr.Debug("GetShrinkProgress -->" + ex.ToString());
            }

            return 0;
        }

        /// <summary>
        /// 收缩Oracle数据库语句
        /// </summary>
        /// <param name="tableNames"></param>
        public void ShrinkOracleTables(List<string> tableNames, IProgress<int> progress)
        {
            try
            {
                int totalTables = tableNames.Count;
                int tableProgressIncrement = 100 / totalTables;
                int queryProgressIncrement = tableProgressIncrement/ totalTables;

                foreach (string tableName in tableNames)
                {
                    string enableRowMovementQuery = string.Format("ALTER TABLE {0} ENABLE ROW MOVEMENT", tableName);
                    string shrinkSpaceCompactQuery = string.Format("ALTER TABLE {0} SHRINK SPACE COMPACT", tableName);
                    string shrinkSpaceQuery = string.Format("ALTER TABLE {0} SHRINK SPACE", tableName);
                    string shrinkSpaceCascadeQuery = string.Format("ALTER TABLE {0} SHRINK SPACE CASCADE", tableName);
                    string disableRowMovementQuery = string.Format("ALTER TABLE {0} DISABLE ROW MOVEMENT", tableName);

                    List<string> queries = new List<string>
                    {
                       enableRowMovementQuery, shrinkSpaceCompactQuery, shrinkSpaceQuery, shrinkSpaceCascadeQuery, disableRowMovementQuery
                    };

                    int queryCount = queries.Count;
                    int currentQuery = 0;

                    foreach (string query in queries)
                    {
                        try
                        {
                            SQLHelper_oracle.Get(query);
                            logger.Debug($"Table {tableName} - Query executed successfully: {query}");

                            // 更新查询进度
                            currentQuery++;
                            int queryProgress = currentQuery * queryProgressIncrement / queryCount;
                            progress.Report(queryProgress);
                        }
                        catch (Exception ex)
                        {
                            loggerErr.Debug($"Table {tableName} - Error executing query: {query}. {ex.Message}");
                            MessageBox.Show($"Table {tableName} - Error executing query: {query}. {ex.Message}");
                            return;
                        }
                    }

                    // 更新总体进度
                    int tableProgress = tableProgressIncrement * currentQuery;
                    progress.Report(tableProgress);
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                loggerErr.Debug("ShrinkOracleTables -->", ex.Message);
            }
        }


        public void ShrinkSqlServerTables(List<string> tableNames)
        {
            try
            {

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    foreach (string tableName in tableNames)
                    {
                        // SQL Server 表收缩 SQL 语句
                        string shrinkQuery = string.Format("DBCC SHRINKTABLE ('{0}')", tableName);

                        using (SqlCommand command = new SqlCommand(shrinkQuery, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                loggerErr.Debug("ShrinkSqlServerTables -->" + ex.ToString());
            }
        }

        ///// <summary>
        ///// Oracle数据库所有表空间总余量
        ///// </summary>
        ///// <returns></returns>
        //public async Task<decimal> GetDatabaseSpace()
        //{
        //    decimal databaseSpace = 0;
        //    try
        //    {
        //        string spaceQuery = "SELECT NVL(SUM(ResizeTo), 0) AS TotalResizeTo " +
        //            "FROM (SELECT ceil(HWM * a.block_size) / 1024 / 1024 AS ResizeTo " +
        //            "FROM v$datafile a " +
        //            "LEFT JOIN (SELECT file_id, MAX(block_id + blocks - 1) AS HWM " +
        //            "           FROM dba_extents " +
        //            "           GROUP BY file_id) b " +
        //            "ON a.file# = b.file_id " +
        //            "WHERE (a.bytes - NVL(HWM * a.block_size, 0)) > 0)";

        //        var spaceResult = await Task.Run(() => SQLHelper_oracle.Get(spaceQuery));
        //        if (spaceResult != null && spaceResult.Rows.Count > 0)
        //        {
        //            databaseSpace = Convert.ToDecimal(spaceResult.Rows[0]["TotalResizeTo"]);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        loggerErr.Debug("GetDatabaseSpace -->" + ex.ToString());
        //    }
        //    return databaseSpace;
        //}
        /// <summary>
        /// 查询当前表空间的剩余空间
        /// </summary>
        /// <returns></returns>
        public async Task<decimal> GetDatabaseSpaceAsync()
        {
            decimal databaseSpace = 0;
            TimeSpan delayBetweenRetries = TimeSpan.FromSeconds(1);
            try
            {
                // 获取异步操作开始时的时间戳
                long startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                // 查询数据库空间大小并等待结果
                string spaceQuery = "SELECT NVL(SUM(ResizeTo), 0) AS TotalResizeTo " +
                                    "FROM (SELECT ceil(HWM * a.block_size) / 1024 / 1024 AS ResizeTo " +
                                    "FROM v$datafile a " +
                                    "LEFT JOIN (SELECT file_id, MAX(block_id + blocks - 1) AS HWM " +
                                    "           FROM dba_extents " +
                                    "           GROUP BY file_id) b " +
                                    "ON a.file# = b.file_id " +
                                    "WHERE (a.bytes - NVL(HWM * a.block_size, 0)) > 0)";
                var spaceResult = await GetAsync(spaceQuery, 3, delayBetweenRetries);
                long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                if (spaceResult != null)
                {
                    databaseSpace = decimal.Parse(spaceResult.Rows[0]["TotalResizeTo"].ToString());
                    // 计算异步操作的执行时间（毫秒）
                    long executionTime = endTime - startTime;
                    logger.Debug($"GetDatabaseSpaceAsync execution time: {executionTime} ms");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                loggerErr.Debug("GetDatabaseSpaceAsync -->" + ex.ToString());
            }
            return databaseSpace;
        }


        /// <summary>
        /// 查询当前表空间的总空间
        /// </summary>
        /// <returns></returns>
        public decimal GetDatabaseTotalSpace()
        {
            decimal totalSpace = 0;
            //获取数据库名称
            string databaseName = GetDatabaseNameFromConnectionString(connectionString);
            try  
            {
                //查询当前用户使用表空间
                string sql = string.Format("select default_tablespace  from  dba_users   where  username= '{0}'", databaseName);
                var defaultTablespaceResult = SQLHelper_oracle.Get(sql);
                string defaultTablespace = defaultTablespaceResult.Rows[0]["default_tablespace"].ToString();

                // 使用当前登录用户表空间进行查询
                string spaceQuery = string.Format("SELECT tablespace_name, ROUND(SUM(maxbytes) / 1024 / 1024, 2) AS total_space_mb FROM dba_data_files WHERE tablespace_name = '{0}' GROUP BY tablespace_name", defaultTablespace);
                var dbaDataFileResult = SQLHelper_oracle.Get(spaceQuery);
                if (dbaDataFileResult.Rows.Count > 0)
                {
                    totalSpace = decimal.Parse(dbaDataFileResult.Rows[0]["total_space_mb"].ToString());

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                loggerErr.Debug("GetDatabaseTotalSpace -->" + ex.ToString());
                //Application.Current.Shutdown();
            }
            return totalSpace;
        }

    }
}
