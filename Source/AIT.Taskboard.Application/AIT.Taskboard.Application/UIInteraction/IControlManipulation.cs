namespace AIT.Taskboard.Application.UIInteraction
{
    /// <summary>
    /// Interface for manipulation of UI elements.
    /// </summary>
    public interface IControlManipulation
    {
        /// <summary>
        /// Zooms a control.
        /// </summary>
        /// <param name="delta">Zooming delta value.</param>
        void ZoomControl(double delta);
    }
}
