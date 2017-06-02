using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySqlBll
{
    public static class GetDefaultValue
    {
        public static string GetDefault(this DATA_TYPE type)
        {
            switch (type)
            {
                case DATA_TYPE.MYSQL_blob:
                case DATA_TYPE.MYSQL_char:
                case DATA_TYPE.MYSQL_longblob:
                case DATA_TYPE.MYSQL_longtext:
                case DATA_TYPE.MYSQL_mediumblob:
                case DATA_TYPE.MYSQL_mediumtext:
                case DATA_TYPE.MYSQL_text:
                case DATA_TYPE.MYSQL_varchar:
                    return "''";
                case DATA_TYPE.MYSQL_datetime:
                case DATA_TYPE.MYSQL_time:
                case DATA_TYPE.MYSQL_timestamp:
                    return "now()";
                case DATA_TYPE.MYSQL_year:
                    return DateTime.Now.Year.ToString();
                default:
                    return "0";
            }
        }
        public static bool IsChar(this DATA_TYPE type)
        {
            switch (type)
            {
                case DATA_TYPE.MYSQL_blob:
                case DATA_TYPE.MYSQL_char:
                case DATA_TYPE.MYSQL_longblob:
                case DATA_TYPE.MYSQL_longtext:
                case DATA_TYPE.MYSQL_mediumblob:
                case DATA_TYPE.MYSQL_mediumtext:
                case DATA_TYPE.MYSQL_text:
                case DATA_TYPE.MYSQL_varchar:
                    return true;
                default:
                    return false;
            }
        }
    }
}
