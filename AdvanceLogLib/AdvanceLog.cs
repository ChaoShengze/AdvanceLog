using System;
using System.IO;
using System.Collections.Generic;

using BaseLogLib;
using System.Diagnostics;
using System.Data.SQLite;

namespace AdvanceLogLib
{
    /// <summary>
    /// 高级日志操作动态库
    /// </summary>
    public class AdvanceLog
    {
        #region 单例模式
        private static readonly object Locker = new object();
        private static AdvanceLog Instance;
        public static AdvanceLog GetInstance()
        {
            if (Instance == null)
                lock (Locker)
                    Instance = new AdvanceLog();

            return Instance;
        }
        private AdvanceLog() { }
        #endregion

        #region 常/变量定义
        /// <summary>
        /// SQLite的数据库文件路径
        /// </summary>
        private const string DBPATH = "LOG.V10";
        /// <summary>
        /// 数据库表类别的缓存，如果本次运行进行过初始化判断则不再尝试初始化，用内存换硬盘性能
        /// </summary>
        private readonly List<string> DBCategoryCache = new List<string>();
        /// <summary>
        /// 装载日志时，单模块的条目上限，默认1000条
        /// </summary>
        public int QueryCountLimit = 1000;
        #endregion

        #region 方法接口
        /// <summary>
        /// 写入Log
        /// </summary>
        /// <param name="logType">日志级别，详见枚举注释</param>
        /// <param name="module">模块名，建议以命名空间区分</param>
        /// <param name="func">函数名</param>
        /// <param name="desc">错误信息</param>
        /// <returns></returns>
        public bool WriteLog(LogType logType, string module, string func, string desc)
        {
            module = ReplaceSpecialSymbol(module);
            func = ReplaceSpecialSymbol(func);
            desc = ReplaceSpecialSymbol(desc);

            var dt = DateTime.Now;
#if DEBUG
            var _desc = $"****************************************" + Environment.NewLine +
                        $"* TIME: {dt:yyyy-MM-dd HH:mm:ss.fff}" + Environment.NewLine +
                        $"* LEVEL: {logType}" + Environment.NewLine +
                        $"* MODU: {module}" + Environment.NewLine +
                        $"* TYPE: {func}" + Environment.NewLine +
                        $"* DESC: {desc}" + Environment.NewLine +
                        $"****************************************";

            Console.WriteLine(_desc);
            Debug.WriteLine(_desc);
#endif

            try
            {
                InitModule();

                var dbPath = $"Data Source = {DBPATH};";
                using (var conn = new SQLiteConnection(dbPath))
                {
                    conn.Open();

                    CreateTable(conn, module);

                    var sql = $"INSERT INTO {module} VALUES({(int)logType}, '{func}', '{desc}', '{dt:yyyy-MM-dd HH:mm:ss.fff}')";
                    var cmd = new SQLiteCommand(sql, conn);
                    var rowsNum = cmd.ExecuteNonQuery();

                    conn.Close();

                    return rowsNum >= 1;
                }
            }
            catch (Exception ex)
            {
                BaseLog.GetInstance().WriteLog(BaseLog.LogType.ERROR, "AdvanceLog", "WriteLog", ex.Message);
                return false;
            }
        }
        /// <summary>
        /// 按照时间装载日志信息
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="modules">模块名</param>
        public List<LogRecord> LoadLog(DateTime startTime, DateTime endTime, string[] modules)
        {
            try
            {
                var limit = QueryCountLimit > 0 ? $"{QueryCountLimit}" : "";
                var list = new List<LogRecord>();

                InitModule();

                var dbPath = $"Data Source = {DBPATH};";
                using (var conn = new SQLiteConnection(dbPath))
                {
                    conn.Open();

                    foreach (var module in modules)
                    {
                        CreateTable(conn, module);
                        var startDt = $"{startTime:yyyy-MM-dd HH:mm:ss}";
                        var endDt = $"{endTime:yyyy-MM-dd HH:mm:ss}";
                        var sql = $"SELECT * FROM {module} WHERE time>= '{startDt}' AND time <= '{endDt}' LIMIT {limit}";
                        var cmd = new SQLiteCommand(sql, conn);
                        var reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            var record = new LogRecord()
                            {
                                LogType = (LogType)reader.GetInt32(0),
                                FuncName = reader.GetString(1),
                                Desc = reader.GetString(2),
                                DateTime = reader.GetDateTime(3),
                                ModuleName = module
                            };
                            list.Add(record);
                        }
                    }

                    conn.Close();
                }
                return list;
            }
            catch (Exception ex)
            {
                BaseLog.GetInstance().WriteLog(BaseLog.LogType.ERROR, "AdvanceLog", "Loadlog", ex.Message);
                return null;
            }
        }
        /// <summary>
        /// 按照时间和日志级别装载日志信息
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="modules">模块名</param>
        /// <param name="logType">日志级别</param>
        public List<LogRecord> LoadLog(DateTime startTime, DateTime endTime, string[] modules, LogType logType)
        {
            try
            {
                var limit = QueryCountLimit > 0 ? $"{QueryCountLimit}" : "";
                var list = new List<LogRecord>();

                InitModule();

                var dbPath = $"Data Source = {DBPATH};";
                using (var conn = new SQLiteConnection(dbPath))
                {
                    conn.Open();

                    foreach (var module in modules)
                    {
                        CreateTable(conn, module);
                        var startDt = $"{startTime:yyyy-MM-dd HH:mm:ss}";
                        var endDt = $"{endTime:yyyy-MM-dd HH:mm:ss}";
                        var sql = $"SELECT * FROM {module} WHERE time>= '{startDt}' AND time <= '{endDt}' AND level = {(int)logType} LIMIT {limit}";
                        var cmd = new SQLiteCommand(sql, conn);
                        var reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            var record = new LogRecord()
                            {
                                LogType = (LogType)reader.GetInt32(0),
                                FuncName = reader.GetString(1),
                                Desc = reader.GetString(2),
                                DateTime = reader.GetDateTime(3),
                                ModuleName = module
                            };
                            list.Add(record);
                        }
                    }

                    conn.Close();
                }
                return list;
            }
            catch (Exception ex)
            {
                BaseLog.GetInstance().WriteLog(BaseLog.LogType.ERROR, "AdvanceLog", "Loadlog", ex.Message);
                return null;
            }
        }
        /// <summary>
        /// 按模块名装载日志信息
        /// </summary>
        /// <param name="modules">模块名</param>
        public List<LogRecord> LoadLog(string[] modules)
        {
            try
            {
                var limit = QueryCountLimit > 0 ? $"{QueryCountLimit}" : "";
                var list = new List<LogRecord>();

                InitModule();

                var dbPath = $"Data Source = {DBPATH};";
                using (var conn = new SQLiteConnection(dbPath))
                {
                    conn.Open();

                    foreach (var module in modules)
                    {
                        CreateTable(conn, module);

                        var sql = $"SELECT * FROM {module} LIMIT {limit}";
                        var cmd = new SQLiteCommand(sql, conn);
                        var reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            var record = new LogRecord()
                            {
                                LogType = (LogType)reader.GetInt32(0),
                                FuncName = reader.GetString(1),
                                Desc = reader.GetString(2),
                                DateTime = reader.GetDateTime(3),
                                ModuleName = module
                            };
                            list.Add(record);
                        }
                    }

                    conn.Close();
                }
                return list;
            }
            catch (Exception ex)
            {
                BaseLog.GetInstance().WriteLog(BaseLog.LogType.ERROR, "AdvanceLog", "Loadlog", ex.Message);
                return null;
            }
        }
        /// <summary>
        /// 获取所有有日志的模块名
        /// </summary>
        /// <returns></returns>
        public string[] GetAllModules()
        {
            try
            {
                var dbPath = $"Data Source = {DBPATH};";
                using (var conn = new SQLiteConnection(dbPath))
                {
                    conn.Open();

                    var sql = "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY name;";
                    var cmd = new SQLiteCommand(sql, conn);
                    var reader = cmd.ExecuteReader();

                    var names = new List<string>();
                    while (reader.Read())
                        names.Add(reader.GetString(0));

                    return names.ToArray();
                }
            }
            catch (Exception ex)
            {
                BaseLog.GetInstance().WriteLog(BaseLog.LogType.ERROR, "AdvanceLog", "GetAllModules", ex.Message);
                return null;
            }
        }
        /// <summary>
        /// 自动清理超时的日志
        /// </summary>
        /// <param name="days"></param>
        public void Clean(int days = 180)
        {
            try
            {
                var dbPath = $"Data Source = {DBPATH};";
                using (var conn = new SQLiteConnection(dbPath))
                {
                    conn.Open();

                    var timeStamp = DateTime.Now.AddDays(days * -1).ToString("yyyy-MM-dd HH:mm:ss");

                    var modules = GetAllModules();
                    if (modules == null)
                        return;

                    foreach (var name in modules)
                    {
                        var _sql = $"DELETE FROM {name} WHERE time <= '{timeStamp}'";
                        var _cmd = new SQLiteCommand(_sql, conn);
                        _cmd.ExecuteNonQuery();
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                BaseLog.GetInstance().WriteLog(BaseLog.LogType.ERROR, "AdvanceLog", "Clean", ex.Message);
            }
        }
        #endregion

        #region 内部方法
        /// <summary>
        /// 初始化模块
        /// </summary>
        private void InitModule()
        {
            try
            {
                if (File.Exists(DBPATH))
                    return;

                var dbPath = $"Data Source = {DBPATH};";
                using (var conn = new SQLiteConnection(dbPath))
                {
                    conn.Open();
                }
            }
            catch (Exception ex)
            {
                BaseLog.GetInstance().WriteLog(BaseLog.LogType.ERROR, "AdvanceLog", "InitModule", ex.Message);
            }
        }
        /// <summary>
        /// 如果数据库中不包含以category变量命名的表，则进行创建
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        private void CreateTable(SQLiteConnection conn, string category)
        {
            try
            {
                //缓存判断是否进行过初始化
                if (DBCategoryCache.Contains(category))
                    return;
                else
                    DBCategoryCache.Add(category);

                var sql = $"CREATE TABLE IF NOT EXISTS {category}(level INTEGER, type TEXT, desc TEXT, time TEXT)";
                var cmd = new SQLiteCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                BaseLog.GetInstance().WriteLog(BaseLog.LogType.ERROR, "AdvanceLog", "CreateTable", ex.Message);
            }
        }
        /// <summary>
        /// 替换字符串中的特殊字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string ReplaceSpecialSymbol(string str)
        {
            return str.Replace("“", " ").Replace("”", " ").Replace('"', ' ');
        }
        #endregion
    }
}
