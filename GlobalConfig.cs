using System.Configuration;
using System.Collections.Specialized;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Polynomial_merge
{
    public static class GlobalConfig
    {
        public static int PageSize { get; private set; }
        public static int MaxRecordLength { get; private set; }
        public static void LoadConfig()
        {
            int loadedPageSize = 0;
            if (int.TryParse(ConfigurationManager.AppSettings.Get("PageSize"),out loadedPageSize))
            {
                PageSize = loadedPageSize;
            }
            else
            {
                throw new Exception("Can't read page size from config file");
            }


            int loadedMinRecordLength = 0;
            if (int.TryParse(ConfigurationManager.AppSettings.Get("MinRecordLength"), out loadedMinRecordLength))
            {
                TapeHandler.minRecordLength = loadedMinRecordLength;
            }
            else
            {
                throw new Exception("Can't read loadedMaxRecordLength from config file");
            }

            int loadedMaxRecordLength = 0;
            if (int.TryParse(ConfigurationManager.AppSettings.Get("MaxRecordLength"), out loadedMaxRecordLength))
            {
                MaxRecordLength = loadedMaxRecordLength;
                TapeHandler.maxRecordLength = loadedMaxRecordLength;
            }
            else
            {
                throw new Exception("Can't read loadedMaxRecordLength from config file");
            }


            int loadedRecordMaxCharValue = '\0';
            if (int.TryParse(ConfigurationManager.AppSettings.Get("RecordMaxCharValue"), out loadedRecordMaxCharValue))
            {
                TapeHandler.maxCharValue = (char)loadedRecordMaxCharValue;
            }
            else
            {
                throw new Exception("Can't read loadedRecordMaxCharValue from config file");
            }

            int loadedRecordMinCharValue = '\0';
            if (int.TryParse(ConfigurationManager.AppSettings.Get("RecordMinCharValue"), out loadedRecordMinCharValue))
            {
                TapeHandler.minCharValue = (char)loadedRecordMinCharValue;
            }
            else
            {
                throw new Exception("Can't read loadedRecordMinCharValue from config file");
            }


        }
    }
}
