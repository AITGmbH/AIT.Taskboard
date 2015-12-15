using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace AIT.Taskboard.Interface
{
    public interface ISyncService
    {
        /// <summary>
        /// Invokes the specified method.
        /// </summary>
        /// <param name="method">The method to invoke on the UI thread.</param>
        /// <param name="args">The arguments to pass the the method referenced by <paramref name="method"/>.</param>
        /// <returns></returns>
        object Invoke(Delegate method, params Object[] args);
        /// <summary>
        /// Invokes the specified method.
        /// </summary>
        /// <param name="method">The method to invoke on the UI thread.</param>
        /// <param name="priority">The priority.</param>
        /// <param name="args">The arguments to pass the the method referenced by <paramref name="method"/>.</param>
        /// <returns></returns>
        object Invoke(Delegate method, DispatcherPriority priority, params Object[] args);
        /// <summary>
        /// Executes a delegate asynchronously on the UI thread.
        /// </summary>
        /// <param name="method">The method to execute.</param>
        /// <param name="args">The arguments to pass the the method referenced by <paramref name="method"/>.</param>
        /// <returns></returns>
        DispatcherOperation BeginInvoke(Delegate method, params Object[] args);

        DispatcherOperation BeginInvoke(DispatcherPriority priority, Delegate method);
        DispatcherOperation BeginInvoke(DispatcherPriority priority, Delegate method, params Object[] args);
        /// <summary>
        /// Determines whether the calling thread is the thread associated with the UI. 
        /// </summary>
        /// <returns><see langword="true"/> if the thread calling this method is the UI thread; otherwise <see langword="false"/></returns>
        bool CheckAccess();
    }
}
