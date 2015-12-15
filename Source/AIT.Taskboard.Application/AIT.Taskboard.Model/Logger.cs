using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AIT.Taskboard.Interface;
using System.Globalization;
using System.IO;

namespace AIT.Taskboard.Model
{
    [Export(typeof(ILogger))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class Logger : ILogger
    {
        private const string LogFileName = "logs.txt";

        private readonly TraceSwitch DebugLevel = new TraceSwitch("DebugLevel", "The Output level of tracing", "3");

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncServiceTrace"/> class.
        /// </summary>
        public Logger()
        {
            Trace.AutoFlush = true;
            Trace.Listeners.Add(new TextWriterTraceListener(LogFile, "TaskboardTraceListener"));
        }

        public string LogFile
        {
            get
            {
                string directory = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                        Constants.ApplicationCompany), 
                                     Constants.ApplicationName);

                Directory.CreateDirectory(directory);

                return Path.Combine(directory, LogFileName);
            }
        }

        /// <summary>
        /// Method logs the exception.
        /// </summary>
        /// <param name="ex">Exception to log.</param>
        public void LogException(Exception ex)
        {
            Write(TraceCategory.Exception, ex.Message);
        }

        /// <summary>
        /// Method writes the message to log.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public void Write(string message)
        {
            Write(TraceCategory.Information, message);
        }

        /// <summary>
        /// Method writes the message to log.
        /// </summary>
        /// <param name="category">Category of the message to log.</param>
        /// <param name="message">Message to log.</param>
        public void Write(string category, string message)
        {
            if (category == TraceCategory.Warning)
            {
                Trace.WriteLineIf(DebugLevel.TraceWarning, FormatMessage(message));
            }
            else if (category == TraceCategory.Exception)
            {
                Trace.WriteLineIf(DebugLevel.TraceError, FormatMessage(message));
            }
            else if (category == TraceCategory.Information)
            {
                Trace.WriteLineIf(DebugLevel.TraceInfo, FormatMessage(message));
            }
            else
            {
                Trace.WriteLineIf(DebugLevel.TraceVerbose, FormatMessage(message));
            }
        }

        /// <summary>
        /// Method writes an exception to log.
        /// </summary>
        /// <param name="category">Category of the log message.</param>
        /// <param name="exception">Exception to log.</param>
        public void Write(string category, Exception exception)
        {
            String message;

            message = exception.TargetSite + "throws " + exception + Environment.NewLine;
            message += exception.Message + Environment.NewLine;
            message += exception.InnerException + Environment.NewLine;
            message += exception.StackTrace + Environment.NewLine;
            message += exception.Data;

            switch (category)
            {
                case TraceCategory.Warning:
                    Trace.WriteLineIf(DebugLevel.TraceWarning, FormatMessage(message));
                    break;
                case TraceCategory.Information:
                    Trace.WriteLineIf(DebugLevel.TraceInfo, FormatMessage(message));
                    break;
                case TraceCategory.Exception:
                    Trace.WriteLineIf(DebugLevel.TraceError, FormatMessage(message));
                    break;
                default:
                    Trace.WriteLineIf(DebugLevel.TraceVerbose, FormatMessage(message));
                    break;
            }
        }

        /// <summary>
        /// Writes an empty new line to the config file.
        /// </summary>
        public void WriteNewLine()
        {
            Trace.WriteLine(String.Empty);
        }

        /// <summary>
        /// Adds a time stamp to a <paramref name="message"/>.
        /// </summary>
        /// <param name="message">Raw message.</param>
        /// <returns>Formated message.</returns>
        private string FormatMessage(string message)
        {
            return string.Format(CultureInfo.CurrentUICulture, "{0} {1}", DateTime.Now, message);
        }

    }
}
