using System;
using System.Collections.Generic;
using System.Text;

namespace AdvanceLogLib
{
    /// <summary>
    /// 单条日志记录
    /// </summary>
    public class LogRecord
    { 
        /// <summary>
        /// 日志级别
        /// </summary>
        public LogType LogType { get; set; }
        /// <summary>
        /// 函数名
        /// </summary>
        public string FuncName { get; set; }
        /// <summary>
        /// 日志内容
        /// </summary>
        public string Desc { get; set; }
        /// <summary>
        /// 日志条目时间
        /// </summary>
        public DateTime DateTime { get; set; } 
        /// <summary>
        /// 模块名称
        /// </summary>
        public string ModuleName { get; set; }
    }
    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// 最低级，信息：一般用于记录运行状态等
        /// </summary>
        INFO,
        /// <summary>
        /// 第二级，警告：一般用于记录意料之外，控制之内的，需要关注的异常情况
        /// </summary>
        WARN,
        /// <summary>
        /// 第三级，错误：一般用于记录发生的异常的相关信息
        /// </summary>
        ERROR,
        /// <summary>
        /// 第四级，调试：仅在DEBUG模式下才会进行记录的信息
        /// </summary>
        DEBUG
    }
}
