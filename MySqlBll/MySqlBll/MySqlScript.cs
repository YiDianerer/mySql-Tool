using System;
using System.Collections.Generic;
using System.Text;

namespace MySqlBll
{
    public class MySqlScript
    {
        private SCHEMA m_Source;

        private SCHEMA m_Target;

        public MySqlScript(SCHEMA source, SCHEMA target)
        {
            this.m_Source = source;
            this.m_Target = target;
        }

        public List<string> MakeScript()
        {
            List<string> list = new List<string>();
            foreach (var sourceTable in m_Source.TABLES)
            {
                TABLE targetTable = this.m_Target.TABLES.Find((TABLE p) => p.TABLE_NAME == sourceTable.TABLE_NAME);
                if (targetTable == null)
                {
                    list.Add(this.MakeNewTable(sourceTable));
                }
                else
                {
                    string engine = this.MakeENGINE(sourceTable, targetTable);
                    if (engine != "")
                    {
                        list.Add(engine);
                    }
                    string rowformat = this.MakeRowFormat(sourceTable, targetTable);
                    if (rowformat != "")
                    {
                        list.Add(rowformat);
                    }
                    string temp = this.MakeChange(sourceTable, targetTable, list);
                    if (temp != "")
                    {
                        list.Add(temp);
                    }
                    string index = this.MakeIndex(sourceTable, targetTable);
                    if (index != "")
                    {
                        list.Add(index);
                    }
                }
            }
            return list;
        }

        private string MakeNewTable(TABLE sourceTable)
        {
            string sql = "CREATE  TABLE `{$TableName}` (\r\n                                {$Fields}\r\n                                {$PRIMARY}  {$INDEX})ENGINE = {$ENGINE} ROW_FORMAT = {$ROW_FORMAT};\r\n                            ";
            string fields = "";
            string primary = "";
            string index = "";
            foreach (COLUMN item in sourceTable.COLUMNS)
            {
                fields = fields + this.GetColumnInfo(item) + " ,";
                if (item.COLUMN_KEY == COLUMN_KEY.PRI)
                {
                    primary = primary + "`" + item.COLUMN_NAME + "`,";
                }
            }
            if (fields.Length > 0)
            {
                fields = fields.Remove(fields.Length - 1);
            }
            if (primary.Length > 0)
            {
                primary = primary.Remove(primary.Length - 1);
                primary = ",PRIMARY KEY (" + primary + ")";
            }
            foreach (INDEX item2 in sourceTable.INDEXS)
            {
                index += string.Format(",{0} INDEX `{1}` ({2})", (item2.NON_UNIQUE == 0) ? " UNIQUE" : "", item2.INDEX_NAME, item2.COLUMN_NAME);
            }
            return sql.Replace("{$TableName}", sourceTable.TABLE_NAME).Replace("{$Fields}", fields).Replace("{$PRIMARY}", primary).Replace("{$ENGINE}", sourceTable.ENGINE.ToString()).Replace("{$ROW_FORMAT}", sourceTable.ROW_FORMAT).Replace("{$INDEX}", index);
        }

        private string MakeENGINE(TABLE sourceTable, TABLE targetTable)
        {
            string result;
            if (sourceTable.ENGINE != targetTable.ENGINE)
            {
                result = string.Concat(new object[]
                {
                    "ALTER TABLE `",
                    targetTable.TABLE_NAME,
                    "` ENGINE = ",
                    sourceTable.ENGINE,
                    " ;"
                });
            }
            else
            {
                result = "";
            }
            return result;
        }

        private string MakeRowFormat(TABLE sourceTable, TABLE targetTable)
        {
            string result;
            if (sourceTable.ROW_FORMAT != targetTable.ROW_FORMAT)
            {
                result = string.Concat(new string[]
                {
                    "ALTER TABLE `",
                    targetTable.TABLE_NAME,
                    "` ROW_FORMAT = ",
                    sourceTable.ROW_FORMAT,
                    " ;"
                });
            }
            else
            {
                result = "";
            }
            return result;
        }

        private string MakeChange(TABLE sourceTable, TABLE targetTable, List<string> extendSqls = null)
        {
            if (extendSqls == null) extendSqls = new List<string>();
            string change = "";
            string changeprimary = "";
            string prev = "";
            bool isChangeKey = false;
            bool isExsitKey = false;
            string primary = "";
            foreach (var sourceColumn in sourceTable.COLUMNS)
            {
                bool updatenull = false;
                COLUMN targetColumn2 = targetTable.COLUMNS.Find((COLUMN p) => p.COLUMN_NAME == sourceColumn.COLUMN_NAME);
                if (targetColumn2 == null)
                {
                    if (sourceColumn.COLUMN_KEY == COLUMN_KEY.PRI)
                    {
                        isChangeKey = true;
                    }
                    string text = change;
                    change = string.Concat(new string[]
                    {
                            text,
                            "ADD COLUMN ",
                            this.GetColumnInfo(sourceColumn),
                            " ",
                            (prev == "") ? " FIRST" : ("AFTER `" + prev + "`"),
                            " ,"
                    });
                }
                else if (!this.EqualsColumn(sourceColumn, targetColumn2, ref updatenull))
                {
                    string text = change;
                    change = string.Concat(new string[]
                    {
                            text,
                            "CHANGE COLUMN `",
                            sourceColumn.COLUMN_NAME,
                            "` ",
                            this.GetColumnInfo(sourceColumn),
                            " ,"
                    });
                    if (updatenull)
                    {
                        string update = "update {0} set {1} = {2} where {1} is null;";
                        string upvalue = (sourceColumn.COLUMN_DEFAULT == DBNull.Value) ? sourceColumn.DATA_TYPE.GetDefault() : sourceColumn.COLUMN_DEFAULT.ToString();
                        update = string.Format(update, sourceColumn.TABLE_NAME, sourceColumn.COLUMN_NAME, upvalue);
                        extendSqls.Add(update);
                    }
                }
                if (targetColumn2 != null && sourceColumn.COLUMN_KEY == COLUMN_KEY.PRI && sourceColumn.COLUMN_KEY != targetColumn2.COLUMN_KEY)
                {
                    isChangeKey = true;
                }
                if (sourceColumn.COLUMN_KEY == COLUMN_KEY.PRI)
                {
                    primary = primary + "`" + sourceColumn.COLUMN_NAME + "`,";
                }
                prev = sourceColumn.COLUMN_NAME;
            }
            foreach (var targetColumn in targetTable.COLUMNS)
            {
                if (targetColumn.COLUMN_KEY == COLUMN_KEY.PRI)
                {
                    isExsitKey = true;
                }
                COLUMN sourceColumn2 = sourceTable.COLUMNS.Find((COLUMN p) => p.COLUMN_NAME == targetColumn.COLUMN_NAME);
                if (sourceColumn2 == null)
                {
                    change = change + " DROP COLUMN `" + targetColumn.COLUMN_NAME + "`,";
                }
            }
            if (change.Length > 0)
            {
                change = change.Remove(change.Length - 1);
            }
            if (primary.Length > 0)
            {
                primary = primary.Remove(primary.Length - 1);
            }
            if (isChangeKey)
            {
                if (isExsitKey)
                {
                    changeprimary = "  DROP PRIMARY KEY , ADD PRIMARY KEY (" + primary + ") ;";
                }
                else
                {
                    changeprimary = "   ADD PRIMARY KEY (" + primary + ") ;";
                }
                if (change.Length > 0)
                {
                    changeprimary = "," + changeprimary;
                }
            }
            string sql = "ALTER TABLE {$TableName} {$Change} {$Primary}";
            string result;
            if (change == "" && changeprimary == "")
            {
                result = "";
            }
            else
            {
                result = sql.Replace("{$TableName}", sourceTable.TABLE_NAME).Replace("{$Change}", change).Replace("{$Primary}", changeprimary);
            }
            return result;
        }

        private string MakeIndex(TABLE sourceTable, TABLE targetTable)
        {
            StringBuilder sb = new StringBuilder();
            foreach (INDEX t in targetTable.INDEXS)
            {
                bool isexsit = false;
                foreach (INDEX s in sourceTable.INDEXS)
                {
                    if (t.INDEX_NAME == s.INDEX_NAME && t.NON_UNIQUE == s.NON_UNIQUE && t.COLUMN_NAME == s.COLUMN_NAME)
                    {
                        isexsit = true;
                        break;
                    }
                }
                if (!isexsit)
                {
                    sb.Append("DROP INDEX " + t.INDEX_NAME + " ,");
                }
            }
            foreach (INDEX s in sourceTable.INDEXS)
            {
                bool isexsit = false;
                foreach (INDEX t in targetTable.INDEXS)
                {
                    if (t.INDEX_NAME == s.INDEX_NAME && t.NON_UNIQUE == s.NON_UNIQUE && t.COLUMN_NAME == s.COLUMN_NAME)
                    {
                        isexsit = true;
                        break;
                    }
                }
                if (!isexsit)
                {
                    sb.AppendFormat(string.Concat(new string[]
                    {
                        "ADD {0} INDEX ",
                        s.INDEX_NAME,
                        "(",
                        s.COLUMN_NAME,
                        ") ,"
                    }), (s.NON_UNIQUE == 0) ? "  UNIQUE" : "");
                }
            }
            if (sb.ToString().EndsWith(","))
            {
                sb.Remove(sb.Length - 1, 1);
            }
            string result;
            if (sb.Length == 0)
            {
                result = "";
            }
            else
            {
                result = string.Concat(new string[]
                {
                    "ALTER TABLE ",
                    sourceTable.TABLE_NAME,
                    " \r\n",
                    sb.ToString(),
                    ";"
                });
            }
            return result;
        }

        private string GetColumnInfo(COLUMN col)
        {
            string fields = "";
            string text = fields;
            fields = string.Concat(new string[]
            {
                text,
                "`",
                col.COLUMN_NAME,
                "` ",
                col.COLUMN_TYPE,
                " ",
                (col.IS_NULLABLE == IS_NULLABLE.NO) ? "NOT NULL" : "NULL"
            });
            if (col.COLUMN_DEFAULT != DBNull.Value)
            {

                fields = fields + " DEFAULT " + ((col.COLUMN_DEFAULT.ToString() == "") ? "''" : col.DATA_TYPE.IsChar() ? "'" + col.COLUMN_DEFAULT.ToString() + "'" : col.COLUMN_DEFAULT.ToString());
            }
            if (col.EXTRA != "")
            {
                fields += " AUTO_INCREMENT";
            }
            return fields;
        }

        private bool EqualsColumn(COLUMN source, COLUMN target, ref bool needUpdateNull)
        {
            needUpdateNull = false;
            bool result;
            if (source.COLUMN_TYPE != target.COLUMN_TYPE)
            {
                result = false;
            }
            else if (source.IS_NULLABLE != target.IS_NULLABLE)
            {
                result = false;
                if (source.IS_NULLABLE == IS_NULLABLE.NO) needUpdateNull = true;
            }
            else if (source.EXTRA != target.EXTRA)
            {
                result = false;
            }
            else
            {
                if (source.COLUMN_DEFAULT != DBNull.Value && target.COLUMN_DEFAULT != DBNull.Value)
                {
                    if (source.COLUMN_DEFAULT.ToString() != target.COLUMN_DEFAULT.ToString())
                    {
                        result = false;
                        return result;
                    }
                }
                result = ((source.COLUMN_DEFAULT != DBNull.Value || target.COLUMN_DEFAULT == DBNull.Value) && (source.COLUMN_DEFAULT == DBNull.Value || target.COLUMN_DEFAULT != DBNull.Value));
            }
            return result;
        }
    }
}
