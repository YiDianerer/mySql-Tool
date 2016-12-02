using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace MySqlBll
{
    public class MySqlCore
    {
        private static Dictionary<string, int> _dtTypes = null;
        public static SCHEMA GetSCHEMA(string conn, string dbname)
        {
            TableMySQL sqltable = new TableMySQL(conn);
            SCHEMA schema = new SCHEMA(dbname);
            DataTable dt = sqltable.Get("select * from information_schema.tables where Table_SCHEMA ='" + dbname + "'");
            foreach (DataRow dr in dt.Rows)
            {
                TABLE table = new TABLE();
                table.TABLE_SCHEMA = ((dr["TABLE_SCHEMA"] == DBNull.Value) ? "" : dr["TABLE_SCHEMA"].ToString());
                table.TABLE_NAME = ((dr["TABLE_NAME"] == DBNull.Value) ? "" : dr["TABLE_NAME"].ToString());
                table.TABLE_TYPE = MySqlCore.GetTABLE_TYPE(dr["TABLE_TYPE"].ToString());
                table.ENGINE = MySqlCore.GetENGINE(dr["ENGINE"].ToString());
                table.TABLE_ROWS = ((dr["TABLE_ROWS"] == DBNull.Value) ? 0uL : ((ulong)dr["TABLE_ROWS"]));
                table.DATA_LENGTH = ((dr["DATA_LENGTH"] == DBNull.Value) ? 0uL : ((ulong)dr["DATA_LENGTH"]));
                table.MAX_DATA_LENGTH = ((dr["MAX_DATA_LENGTH"] == DBNull.Value) ? 0uL : ((ulong)dr["MAX_DATA_LENGTH"]));
                table.INDEX_LENGTH = ((dr["INDEX_LENGTH"] == DBNull.Value) ? 0uL : ((ulong)dr["INDEX_LENGTH"]));
                table.CREATE_TIME = ((dr["CREATE_TIME"] == DBNull.Value) ? DateTime.MinValue : ((DateTime)dr["CREATE_TIME"]));
                table.UPDATE_TIME = ((dr["UPDATE_TIME"] == DBNull.Value) ? DateTime.MinValue : ((DateTime)dr["UPDATE_TIME"]));
                table.TABLE_COMMENT = ((dr["TABLE_COMMENT"] == DBNull.Value) ? "" : dr["TABLE_COMMENT"].ToString());
                table.ROW_FORMAT = ((dr["ROW_FORMAT"] == DBNull.Value) ? "" : dr["ROW_FORMAT"].ToString());
                schema.TABLES.Add(table);
                DataTable dtcol = sqltable.Get(string.Concat(new string[]
                {
                    "select * from COLUMNs where table_schema='",
                    table.TABLE_SCHEMA,
                    "' and table_name='",
                    table.TABLE_NAME,
                    "';"
                }));
                foreach (DataRow drcol in dtcol.Rows)
                {
                    COLUMN column = new COLUMN();
                    column.TABLE_SCHEMA = dbname;
                    column.TABLE_NAME = table.TABLE_NAME;
                    column.COLUMN_NAME = drcol["COLUMN_NAME"].ToString();
                    column.ORDINAL_POSITION = ((drcol["ORDINAL_POSITION"] == DBNull.Value) ? 0uL : ((ulong)drcol["ORDINAL_POSITION"]));
                    column.COLUMN_DEFAULT = drcol["COLUMN_DEFAULT"];
                    column.IS_NULLABLE = MySqlCore.GetIS_NULLABLE(drcol["IS_NULLABLE"].ToString());
                    column.DATA_TYPE = MySqlCore.GetDATA_TYPE(drcol["DATA_TYPE"].ToString());
                    column.CHARACTER_MAXIMUM_LENGTH = ((drcol["CHARACTER_MAXIMUM_LENGTH"] == DBNull.Value) ? 0uL : ((ulong)drcol["CHARACTER_MAXIMUM_LENGTH"]));
                    column.CHARACTER_OCTET_LENGTH = ((drcol["CHARACTER_OCTET_LENGTH"] == DBNull.Value) ? 0uL : ((ulong)drcol["CHARACTER_OCTET_LENGTH"]));
                    column.NUMERIC_PRECISION = ((drcol["NUMERIC_PRECISION"] == DBNull.Value) ? 0uL : ((ulong)drcol["NUMERIC_PRECISION"]));
                    column.NUMERIC_SCALE = ((drcol["NUMERIC_SCALE"] == DBNull.Value) ? 0uL : ((ulong)drcol["NUMERIC_SCALE"]));
                    column.COLUMN_TYPE = ((drcol["COLUMN_TYPE"] == DBNull.Value) ? "" : drcol["COLUMN_TYPE"].ToString());
                    column.COLUMN_KEY = MySqlCore.GetCOLUMN_KEY(drcol["COLUMN_KEY"].ToString());
                    column.COLUMN_COMMENT = drcol["COLUMN_COMMENT"].ToString();
                    column.EXTRA = ((drcol["EXTRA"] == DBNull.Value) ? "" : drcol["EXTRA"].ToString());
                    table.COLUMNS.Add(column);
                }
                DataTable dtindex = sqltable.Get(string.Concat(new string[]
                {
                    "select *  from information_schema.statistics where table_schema='",
                    table.TABLE_SCHEMA,
                    "' and table_name='",
                    table.TABLE_NAME,
                    "';"
                }));
                foreach (DataRow dri in dtindex.Rows)
                {
                    string indexname = dri["INDEX_NAME"].ToString();
                    if (!(indexname == "PRIMARY"))
                    {
                        INDEX model = table.INDEXS.Find((INDEX p) => p.INDEX_NAME == indexname);
                        if (model == null)
                        {
                            model = new INDEX();
                            model.INDEX_NAME = indexname;
                            model.NON_UNIQUE = Convert.ToInt32(dri["NON_UNIQUE"]);
                            model.COLUMN_NAME = string.Format("`{0}` ASC", dri["COLUMN_NAME"]);
                            table.INDEXS.Add(model);
                        }
                        else model.COLUMN_NAME += string.Format(",`{0}` ASC", dri["COLUMN_NAME"]);
                    }
                }
            }
            schema.PROCEDURE = GetPROCEDURE(conn, dbname);
            return schema;
        }

        public static List<string> GetPROCEDURE(string conn, string dbname)
        {
            TableMySQL sqltable = new TableMySQL(conn);
            List<string> list = new List<string>();
            DataTable dt = sqltable.Get("select type,name,param_list,body,returns  from mysql.proc where db='" + dbname + "'");
            foreach (DataRow dr in dt.Rows)
            {
                string name = dr["name"].ToString();
                string type = dr["type"].ToString();
                string returns = Encoding.UTF8.GetString((dr["returns"] == DBNull.Value) ? new byte[0] : ((byte[])dr["returns"]));
                string paralist = Encoding.UTF8.GetString((dr["param_list"] == DBNull.Value) ? new byte[0] : ((byte[])dr["param_list"]));
                string body = Encoding.UTF8.GetString((dr["body"] == DBNull.Value) ? new byte[0] : ((byte[])dr["body"]));
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("DROP " + type + " IF EXISTS `" + name + "`;");
                sb.AppendLine(string.Concat(new string[]
                {
                            "CREATE  "+type+" `",
                            name,
                            "`(",
                            paralist,
                            ")"
                }));
                if (type.ToLower() == "function")
                {
                    sb.AppendLine("RETURNS " + returns);
                }
                sb.Append(body);
                list.Add(sb.ToString());
            }
            return list;
        }

        private static TABLE_TYPE GetTABLE_TYPE(string value)
        {
            TABLE_TYPE result;
            if (value != null)
            {
                if (value == "SYSTEM VIEW")
                {
                    result = TABLE_TYPE.SYSTEMVIEW;
                    return result;
                }
                if (value == "BASE TABLE")
                {
                    result = TABLE_TYPE.BASETABLE;
                    return result;
                }
                if (value == "VIEW")
                {
                    result = TABLE_TYPE.VIEW;
                    return result;
                }
            }
            result = TABLE_TYPE.NONE;
            return result;
        }

        private static ENGINE GetENGINE(string value)
        {
            ENGINE result;
            if (value != null)
            {
                if (value == "CSV")
                {
                    result = ENGINE.CSV;
                    return result;
                }
                if (value == "InnoDB")
                {
                    result = ENGINE.InnoDB;
                    return result;
                }
                if (value == "MEMORY")
                {
                    result = ENGINE.MEMORY;
                    return result;
                }
                if (value == "MyISAM")
                {
                    result = ENGINE.MyISAM;
                    return result;
                }
                if (value == "PERFORMANCE_SCHEMA")
                {
                    result = ENGINE.PERFORMANCE_SCHEMA;
                    return result;
                }
            }
            result = ENGINE.NONE;
            return result;
        }

        private static IS_NULLABLE GetIS_NULLABLE(string value)
        {
            IS_NULLABLE result;
            if (value != null)
            {
                if (value == "NO")
                {
                    result = IS_NULLABLE.NO;
                    return result;
                }
                if (value == "YES")
                {
                    result = IS_NULLABLE.YES;
                    return result;
                }
            }
            result = IS_NULLABLE.YES;
            return result;
        }

        private static DATA_TYPE GetDATA_TYPE(string value)
        {
            if (value != null)
            {
                if (_dtTypes == null)
                {
                    _dtTypes = new Dictionary<string, int>(23)
                    {
                        {  "bigint", 0 },  {  "blob",   1 },  { "mediumblob", 2 }, { "tinyblob", 3 },
                        { "char", 4  },  { "datetime",  5 },  { "decimal", 6 },  { "double", 7 },
                        { "enum", 8 }, { "float",  9 }, { "int",  10 },  { "longblob", 11  },
                        { "longtext", 12  }, {  "mediumint", 13 },  {  "mediumtext", 14 },
                        { "set",  15 }, { "smallint",  16 },  { "text",  17  }, { "time", 18 },
                        { "timestamp", 19 }, { "tinyint", 20 },  { "varchar", 21 }, { "year",  22  }, { "bit",  23 }, { "date", 5 }
                    };
                }
                int num;
                if (_dtTypes.TryGetValue(value, out num))
                {
                    DATA_TYPE result = (DATA_TYPE)num;
                    if (!Enum.IsDefined(typeof(DATA_TYPE), result))
                        throw new Exception("类型错误");
                    return result;
                }
                else
                    throw new Exception("类型错误");
            }
            else
                throw new Exception("类型错误");
        }

        public static COLUMN_KEY GetCOLUMN_KEY(string value)
        {
            COLUMN_KEY result;
            if (value != null)
            {
                if (value == "")
                {
                    result = COLUMN_KEY.NONE;
                    return result;
                }
                if (value == "MUL")
                {
                    result = COLUMN_KEY.MUL;
                    return result;
                }
                if (value == "PRI")
                {
                    result = COLUMN_KEY.PRI;
                    return result;
                }
                if (value == "UNI")
                {
                    result = COLUMN_KEY.UNI;
                    return result;
                }
            }
            result = COLUMN_KEY.NONE;
            return result;
        }

        public static DataTable GetDataBase(string conn)
        {
            TableMySQL sqltable = new TableMySQL(conn);
            DataTable dt = sqltable.Get("show databases;");
            int i = dt.Rows.Count - 1;
            while (i >= 0)
            {
                DataRow dr = dt.Rows[i];
                string dbname = dr["DataBase"].ToString();
                string text = dbname;
                if (text != null)
                {
                    if (!(text == "information_schema"))
                    {
                        if (!(text == "performance_schema"))
                        {
                            if (text == "mysql")
                            {
                                dt.Rows.Remove(dr);
                            }
                        }
                        else
                        {
                            dt.Rows.Remove(dr);
                        }
                    }
                    else
                    {
                        dt.Rows.Remove(dr);
                    }
                }
                i--;
            }
            return dt;
        }

        public static string[] GetDbInfo(string conn)
        {
            TableMySQL sqltable = new TableMySQL(conn);
            string sql = "CREATE  TABLE if not exists `db_info` (\n  `tag` VARCHAR(50) NOT NULL DEFAULT '' ,\n  `version` INT NULL DEFAULT 0 ,\n  `outtag` VARCHAR(50) NOT NULL DEFAULT '' ,\n  `outversion` INT NULL DEFAULT 0 ,\n  PRIMARY KEY (`tag`) );";
            sqltable.ExecuteNonQuery(sql);
            DataTable dt = sqltable.Get("select * from  db_info");
            string[] result = new string[]
            {
                "",
                "0",
                "",
                "0"
            };
            if (dt.Rows.Count > 0)
            {
                result[0] = ((dt.Rows[0]["tag"] == DBNull.Value) ? "0" : dt.Rows[0]["tag"].ToString());
                result[1] = ((dt.Rows[0]["version"] == DBNull.Value) ? "0" : dt.Rows[0]["version"].ToString());
                result[2] = ((dt.Rows[0]["outtag"] == DBNull.Value) ? "" : dt.Rows[0]["outtag"].ToString());
                result[3] = ((dt.Rows[0]["outversion"] == DBNull.Value) ? "0" : dt.Rows[0]["outversion"].ToString());
            }
            return result;
        }

        public static void SaveDbInfo(string conn, string dbtag, int dbversion, string outdbtag, int outdbversion)
        {
            TableMySQL sqltable = new TableMySQL(conn);
            string sql = string.Format("delete from db_info ;\r\n                                    insert into db_info values('{0}',{1},'{2}',{3})", new object[]
            {
                dbtag,
                dbversion,
                outdbtag,
                outdbversion
            });
            sqltable.ExecuteNonQuery(sql);
        }
    }
}
