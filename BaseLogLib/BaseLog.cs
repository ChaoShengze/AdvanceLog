using System;
using System.Diagnostics;
using System.IO;

namespace BaseLogLib
{
    public class BaseLog
    {
        #region 单例模式
        private BaseLog() { }
        private static readonly object locker = new object();
        private static BaseLog _instance;
        public static BaseLog GetInstance()
        {
            if (_instance == null)
                lock (locker)
                    _instance = new BaseLog();

            return _instance;
        }
        #endregion

        public enum LogType
        {
            INFO,
            WARN,
            ERROR,
            DEBUG
        }

        /// <summary>
        /// 写文本日志
        /// </summary>
        /// <param name="logType">级别</param>
        /// <param name="module">模块名</param>
        /// <param name="func">函数名</param>
        /// <param name="desc">信息</param>
        public void WriteLog(LogType logType, string module, string func, string desc)
        {
            try
            {
                string _level = "";
                switch (logType)
                {
                    case LogType.INFO:
                        _level = "INFO";
                        break;
                    case LogType.WARN:
                        _level = "WARN";
                        break;
                    case LogType.ERROR:
                        _level = "ERROR";
                        break;
                }

                var folder = Path.Combine("LOG");
                var filePath = Path.Combine(folder, $"{DateTime.Now:yyyy-MM-dd}.log");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var _desc = $"********************" + Environment.NewLine +
                            $"* TIME: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}" + Environment.NewLine +
                            $"* LEVEL: {_level}" + Environment.NewLine +
                            $"* MODULE: {module}" + Environment.NewLine +
                            $"* FUNC: {func}" + Environment.NewLine +
                            $"* DESC: {desc}" + Environment.NewLine +
                            $"********************";

#if DEBUG
                Console.WriteLine(_desc);
                Debug.WriteLine(_desc);
#endif

                File.AppendAllText(filePath, _desc);
            }
            catch
            {

            }
        }
    }
}
