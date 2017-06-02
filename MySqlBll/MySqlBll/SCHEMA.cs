using System;
using System.Collections.Generic;

namespace MySqlBll
{
    [Serializable]
    public class SCHEMA
    {
        public string SCHEMA_NAME
        {
            get;
            set;
        }

        public List<TABLE> TABLES
        {
            get;
            set;
        }

        public string Tag
        {
            get;
            set;
        }

        public int Version
        {
            get;
            set;
        }

        public List<string> PROCEDURE
        {
            get;
            set;
        }

        public SCHEMA(string name)
        {
            this.SCHEMA_NAME = name;
            this.TABLES = new List<TABLE>();
        }
    }
}
