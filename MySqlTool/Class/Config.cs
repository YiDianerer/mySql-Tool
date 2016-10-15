using System;
using System.Configuration;

namespace MySqlTool.Class
{
    public class Config
    {
        private static string _BackPath;

        public static string BackPath
        {
            get
            {
                if (Config._BackPath == null)
                {

                    Config._BackPath = ConfigurationManager.AppSettings["BackPath"];
                    if (Config._BackPath == null)
                    {
                        Config._BackPath = AppDomain.CurrentDomain.BaseDirectory;
                    }
                    if (!Config._BackPath.EndsWith("\\"))
                    {
                        Config._BackPath += "\\";
                    }
                }
                return Config._BackPath;
            }
        }
    }
}
