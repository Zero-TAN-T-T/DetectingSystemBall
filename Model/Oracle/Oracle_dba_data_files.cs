using iPlant.Components.Framework.DataAccess.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectingSystemBall.Model.Oracle
{
    [EntityMapping(TableName = "dba_data_files")]

    internal class Oracle_dba_data_files
    {
        public string FILE_NAME { get; set; }
        public decimal FILE_ID { get; set; }
        public string TABLESPACE_NAME { get; set; }
        public decimal BYTES { get; set; }
        public decimal BLOCKS { get; set; }
        public string STATUS { get; set; }
        public decimal AUTOEXTENSIBLE { get; set; }
        public decimal MAXBYTES { get; set; }
        public decimal MAXBLOCKS { get; set; }
        public decimal INCREMENT_BY { get; set; }
        public decimal USER_BYTES { get; set; }
        public decimal USER_BLOCKS { get; set; }
        public string ONLINE_STATUS { get; set; }

    }
}
