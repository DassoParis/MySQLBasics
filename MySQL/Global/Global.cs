using System;

namespace MySQL
{
    public enum Status { Failed, Successful};    

    public static class Database
    {
        public static string ConnectionString = "datasource=localhost; port=3306; username=dasso; password="; 

        public enum ReadingType { DataAdapter, DataReader };

        public enum SearchingType { WithDatabase, NoDatabase };
    }

    public static class ParsingExtensions
    {
        public static int ParseInt (this object value, int defaultValue = -1)
        {
            if (value == null)
                return defaultValue;

            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static string ParseString (this object value, string defaultValue = "Invalid")
        {
            if (value == null)
                return defaultValue;

            try
            {
                return value.ToString();
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
