using System;
using System.Diagnostics;

namespace AIT.Taskboard.Application.UIInteraction
{
    class GestureEventManager
    {
        private readonly IControlManipulation _manipulator;
        private bool _panningInProgress = false;
        private long _initialPanValue;
        private long _currentPanningValue;

        private bool _zoomingInProgress = false;
        private long _initialZoomValue;
        private long _currentZoomValue;

        /// <summary>
        /// Keep GestureZoom in specific Range
        /// </summary>
        const double maxvalue = 1.3;
        const double minvalue = 0.8;

        public GestureEventManager(IControlManipulation manipulator)
        {
            if(manipulator == null) throw new ArgumentException("manipulator");
            _manipulator = manipulator;
        }

        /// <summary>
        /// Process GestureZoomMessage
        /// </summary>
        /// <param name="gestureEventArgs"></param>
        public void ProcessGestureZoom(GestureEventArgs gestureEventArgs)
        {
            /*
             The first GID_ZOOM command message begins a zoom 
             but does not cause any zooming. The second GID_ZOOM 
             command triggers a zoom relative to the state 
             contained in the first GID_ZOOM.
             */

            if (_zoomingInProgress)
            {
                // Process changing ZoomValue relative to first zoomvalue
                _currentZoomValue = gestureEventArgs.Arguments;

            }
            else
            {
                // Notify 1st Zoom Message has been processed
                // and remember first zoomvalue
                _zoomingInProgress = true;
                _initialZoomValue = gestureEventArgs.Arguments;
            }
        }

        /// <summary>
        /// Process GesturePanMessage
        /// </summary>
        /// <param name="gestureEventArgs"></param>
        public void ProcessGesturePan(GestureEventArgs gestureEventArgs)
        {
            if (_panningInProgress)
            {

                /* TODO: implement panning
                 * */
            }
            else
            {
                // Notify 1st Zoom Message has been processed
                // and remember first zoomvalue
                _panningInProgress = true;
                _initialPanValue = gestureEventArgs.Arguments;
            }
        }

      
           

        /// <summary>
        /// Process GestureEndMessage, Finish Gesture, reset temporary values
        /// </summary>
        /// <param name="gestureEventArgs"></param>
        public void ProcessGestureEnd(GestureEventArgs gestureEventArgs)
        {
            if (_zoomingInProgress)
            {
                double val = (_currentZoomValue - _initialZoomValue);
                double verhaeltnis = Math.Abs(val / _initialZoomValue);
                if (_currentZoomValue > _initialZoomValue)
                {
                    // zoom in must be greater 1
                    val = verhaeltnis+1;
                }
                else
                {
                    // zoom out must be lower 1
                    val=verhaeltnis;
                }               

                try
                {
                    //check range exceeded
                    if (val > maxvalue) val = maxvalue;
                    if (val < minvalue) val = minvalue;

                    //perform zoom operation
                    _manipulator.ZoomControl(val);
                }
                catch (Exception e)
                {
                    Debug.Print(e.Message + ", " + e.StackTrace);
                }

                _zoomingInProgress = false;
            }

            if (_panningInProgress)
            {
                _panningInProgress = false;
            }
        }

        /// <summary>
        /// Process GestureBeginMessage
        /// </summary>
        /// <param name="gestureEventArgs"></param>
        public void ProcessGestureBegin(GestureEventArgs gestureEventArgs)
        {
            // empty on purpose
        }
    }
}
