using iPlant.Components.Framework.DataAccess.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectingSystemBall.Model.Oracle
{
    [EntityMapping(TableName = "USER_USERS")]

    internal class Oracle_USER_USERS
    {
        public string UserName { get; set; }
        public decimal USER_ID { get; set; }
        public string ACCOUNT_STATUS { get; set; }
        public DateTime LOCK_DATE { get; set; }  
        public DateTime EXPIRY_DATE { get; set; }
        public string DEFAULT_TABLESPACE { get; set; }
        public string TEMPORARY_TABLESPACE { get; set; }
        public DateTime CREATED { get; set; }
        public string INITIAL_RSRC_CONSUMER_GROUP { get; set; }
        public string EXTERNAL_NAME { get; set; }

    }
}
