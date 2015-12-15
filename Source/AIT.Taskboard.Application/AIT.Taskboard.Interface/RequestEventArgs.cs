using System;
using System.ComponentModel;

namespace AIT.Taskboard.Interface
{
    public class RequestEventArgs : CancelEventArgs
    {
        public bool Handled { get; set; }
    }
}