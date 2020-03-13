using System;
using System.Diagnostics;

using Common.Logging;
using Common.Logging.Factory;

using log4net.Core;

namespace PPWCode.Vernacular.NHibernate.III.Test.Log4Net
{
    /// <summary>
    ///     Concrete implementation of <see cref="ILog" /> interface specific to log4net 1.2.9-1.2.11.
    /// </summary>
    /// <remarks>
    ///     Log4net is capable of outputting extended debug information about where the current
    ///     message was generated: class name, method name, file, line, etc. Log4net assumes that the location
    ///     information should be gathered relative to where Debug() was called.
    ///     When using Common.Logging, Debug() is called in Common.Logging.Log4Net.Log4NetLogger. This means that
    ///     the location information will indicate that Common.Logging.Log4Net.Log4NetLogger always made
    ///     the call to Debug(). We need to know where Common.Logging.ILog.Debug()
    ///     was called. To do this we need to use the log4net.ILog.Logger.Log method and pass in a Type telling
    ///     log4net where in the stack to begin looking for location information.
    /// </remarks>
    /// <author>Gilles Bayon</author>
    /// <author>Erich Eichinger</author>
    [Serializable]
    public class Log4NetLogger : AbstractLogger
    {
        private static Type _callerStackBoundaryType;
        private readonly ILogger _logger;

        protected internal Log4NetLogger(ILoggerWrapper log)
        {
            _logger = log.Logger;
        }

        /// <summary>
        ///     Returns the global context for variables
        /// </summary>
        public override IVariablesContext GlobalVariablesContext
            => new Log4NetGlobalVariablesContext();

        /// <summary>
        ///     Returns the thread-specific context for variables
        /// </summary>
        public override IVariablesContext ThreadVariablesContext
            => new Log4NetThreadVariablesContext();

        public override INestedVariablesContext NestedThreadVariablesContext
            => new Log4NetNestedThreadVariablesContext();

        public override bool IsTraceEnabled
            => _logger.IsEnabledFor(Level.Trace);

        public override bool IsDebugEnabled
            => _logger.IsEnabledFor(Level.Debug);

        public override bool IsInfoEnabled
            => _logger.IsEnabledFor(Level.Info);

        public override bool IsWarnEnabled
            => _logger.IsEnabledFor(Level.Warn);

        public override bool IsErrorEnabled
            => _logger.IsEnabledFor(Level.Error);

        public override bool IsFatalEnabled
            => _logger.IsEnabledFor(Level.Fatal);

        /// <summary>
        ///     Actually sends the message to the underlying log system.
        /// </summary>
        /// <param name="logLevel">the level of this log event.</param>
        /// <param name="message">the message to log</param>
        /// <param name="exception">the exception to log (may be null)</param>
        protected override void WriteInternal(LogLevel logLevel, object message, Exception exception)
        {
            // determine correct caller - this might change due to jit optimizations with method inlining
            if (_callerStackBoundaryType == null)
            {
                lock (GetType())
                {
                    StackTrace stack = new StackTrace();
                    Type thisType = GetType();
                    _callerStackBoundaryType = typeof(AbstractLogger);
                    for (int i = 1; i < stack.FrameCount; i++)
                    {
                        if (!IsInTypeHierarchy(thisType, stack.GetFrame(i).GetMethod().DeclaringType))
                        {
                            _callerStackBoundaryType = stack.GetFrame(i - 1).GetMethod().DeclaringType;
                            break;
                        }
                    }
                }
            }

            Level level = GetLevel(logLevel);
            _logger.Log(_callerStackBoundaryType, level, message, exception);
        }

        private bool IsInTypeHierarchy(Type currentType, Type checkType)
        {
            while ((currentType != null) && (currentType != typeof(object)))
            {
                if (currentType == checkType)
                {
                    return true;
                }

                currentType = currentType.BaseType;
            }

            return false;
        }

        public static Level GetLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.All:
                    return Level.All;
                case LogLevel.Trace:
                    return Level.Trace;
                case LogLevel.Debug:
                    return Level.Debug;
                case LogLevel.Info:
                    return Level.Info;
                case LogLevel.Warn:
                    return Level.Warn;
                case LogLevel.Error:
                    return Level.Error;
                case LogLevel.Fatal:
                    return Level.Fatal;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, "unknown log level");
            }
        }
    }
}
