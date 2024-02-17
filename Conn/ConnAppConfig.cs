using iPlant.Components.Framework.Util.Config;
using iPlant.Components.Framework.Util.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectingSystemBall.Conn
{
    public class ConnAppConfig
    {
        //获取数据库连接
        public readonly static string DB_Conn = AppSettingsHelper.GetString("Conne_DB");

        //获取数据库连接类型
        public readonly static DBType Conne_Type = (DBType)Enum.Parse(typeof(DBType), AppSettingsHelper.GetString("Conne_Type"));

        //获取数据库实例名称
        public readonly static string DIRECTORY = AppSettingsHelper.GetString("DIRECTORY");

        //获取备份文件存放路径
        public readonly static string DUMPFile = AppSettingsHelper.GetString("DUMPFile");
    }
}
