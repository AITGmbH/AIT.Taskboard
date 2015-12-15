using System;
using System.Reflection;

namespace AIT.Taskboard.Interface.MEF
{
    /// <summary>
    /// 
    /// </summary>
    public class BootstrapperEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BootstrapperEventArgs"/> class.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public BootstrapperEventArgs(Assembly assembly)
        {
            Assembly = assembly;
        }

        /// <summary>
        /// Gets or sets the assembly.
        /// </summary>
        /// <value>The assembly.</value>
        public Assembly Assembly { get; set; }
    }
}