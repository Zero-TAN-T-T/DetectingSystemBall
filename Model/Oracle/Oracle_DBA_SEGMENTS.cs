using iPlant.Components.Framework.DataAccess.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectingSystemBall.Model.Oracle
{
    [EntityMapping(TableName = "Oracle_DBA_SEGMENTS")]
    public class Oracle_DBA_SEGMENTS
    {
        //段所有者的用户名
        public string OWNER { get; set; }

        //段的名称
        public string SEGMENT_NAME { get; set; }

        //对象分区名称（NULL对于未分区的对象，设置为）
        public string PARTITION_NAME { get; set; }

        public string SEGMENT_TYPE { get ; set; }

        public string TABLESPACE_NAME { get ; set; }

        public decimal HEADER_FILE { get; set; }
        public decimal  HEADER_BLOCK { get; set; }
        public decimal  BYTES { get; set; }
        public decimal  BLOCKS { get; set; }
        public decimal  EXTENTS { get; set; }
        public decimal  INITIAL_EXTENT { get; set; }
        public decimal  NEXT_EXTENT { get; set; }
        public decimal  MIN_EXTENTS { get; set; }
        public decimal  MAX_EXTENTS { get; set; }
        public decimal  PCT_INCREASE { get; set; }
        public decimal  FREELISTS { get; set; }
        public decimal  FREELIST_GROUPS { get; set; }
        public decimal  RELATIVE_FNO { get; set; }
        public string BUFFER_POOL { get; set; }

    }
}
