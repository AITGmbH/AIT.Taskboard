using System;
using System.Windows.Media;
using AIT.Taskboard.Application.Controls;

namespace AIT.Taskboard.Application.UIInteraction
{
    /// <summary>
    /// Manipulates the WorkItemControl visual representation.
    /// </summary>
    class WorkitemControlManipulator : IControlManipulation
    {
        private readonly WorkItemControl _owningControl;

        /// <summary>
        /// Creates a new instance of the WorkitemControlManipulator class.
        /// </summary>
        /// <param name="workItemControl">Workitem upon which all visual changes are performed.</param>
        public WorkitemControlManipulator(WorkItemControl workItemControl)
        {
            if(workItemControl == null) throw new ArgumentException("workItemControl");
            _owningControl = workItemControl;
        }

        #region IControlManipulation members

        public void ZoomControl(double delta)
        {
            if (delta == 0) return;

            // Use LayoutTransform instead of RenderTransform to automatically 
            // align space between single workitems
            MatrixTransform renderTransformObject = _owningControl.LayoutTransform as MatrixTransform;

            if (renderTransformObject != null)
            {
                Matrix matrix = renderTransformObject.Matrix;
                matrix.Scale(delta, delta);

                MatrixTransform newTransformObject = new MatrixTransform(matrix);
                _owningControl.LayoutTransform = newTransformObject;

            }
        }

        #endregion
    }
}
