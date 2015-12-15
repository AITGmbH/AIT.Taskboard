using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIT.Taskboard.Interface
{
    public interface ILogger
    {
        void LogException(Exception ex);
        void Write(string category, Exception exception);
        void Write(string category, string message);
        void Write(string message);
        void WriteNewLine();
        string LogFile { get; }
    }
}
