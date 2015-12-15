using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIT.Taskboard.Interface
{
    public class ErrorEventArgs:EventArgs
    {
        public string ErrorMessage { get; set; }
    }
}
