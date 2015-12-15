using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using AIT.Taskboard.Application.Controls;
using Microsoft.Reporting.WinForms;

namespace AIT.Taskboard.Application.UIInteraction
{
    /// <summary>
    /// Handles multitouch events performed on the taskboard and on workitems.
    /// </summary>
    /// <remarks>
    /// CLASS IS DEPRECATED. DO NOT USE!
    /// </remarks>
    [Obsolete("Not used anymore",true)]
    public class MultiTouchManager
    {
        #region Fields

        // Represents a touch point on the screen.
        // The key identifies the finger and the value the position of the finger on the screen.
        // Must be static because any listening multitouch manager may receive TouchUp calls,
        // depending on the position of the fingers on the UI.
        // The multitouch manager who registers the first TouchDown event is granted exclusive
        // write privileges of touch points until end of all active touch operations.
        private static DictionaryEntry _firstPoint = new DictionaryEntry(null, null);
        private static DictionaryEntry _secondPoint = new DictionaryEntry(null, null);

        // These points are the last points captured by touch move events. They are necessary
        // to calculate if an event will be processed or if its data delta is to low and 
        // therefore would only cost cpu time.
        private TouchPoint _lastCapturedFirstPoint;
        private TouchPoint _lastCapturedSecondPoint;

        // Holds the event arguments while a move event is processed since a lot of
        // methods require these arguments.
        private TouchEventArgs _currentMoveEventArgs;
        
        // Minimum distance a touch point (finger) has to be moved before performing any
        // calculations or updating the UI.
        private const int UiUpdateOffset = 3;

        // Sets the distance a touch point has to move to double the size of a workitem.
        // private const int AbsoluteZoomMultiplier = 100;

        // Sets the distance a touch point has to move before a drag and drop action is
        // initialized.
        private const int DragAndDropOffset = 50;

        // Is true while a drag and drop and drop action of a workitem is in process.
        private bool _isDragAndDropAction;

        // max capacitiy of queue
        private const int MaxQueueLength = 10;

        // store last x-values of first finger in queue
        private readonly Queue _fingerOnePositions = new Queue(MaxQueueLength);

        // store last x-values of second finger in separate queue
        private readonly Queue _fingerTwoPositions = new Queue(MaxQueueLength);

        // store delta values in queue
        private Queue _deltaQueue = new Queue(MaxQueueLength);

        // use min max values to supress speeding or jumping zoom
        private const double MinimumDelta = 0.8;
        private const double MaximumDelta = 1.2;

        // Percentage of necessary valid delta values in queue
        private const double ValidityTresholdValue = 0.7;

        // Owner of all multitouch events.
        private readonly IInputElement _owner;

        // Holds the multitouch manager currently performing any actions.
        private static MultiTouchManager _operator;

        /// <summary>
        /// Returns the state of multitouch zoom.
        /// </summary>
        /// <remarks>
        /// Returns true while a multitouch zoom operation is in process and false if
        /// such an operation is performing. Can be used to prevent common UI update
        /// procedures to cease while zooming.
        /// </remarks>
        public static bool IsZooming { get; private set; }

        #endregion

        #region Events

        public static event EventHandler TaskboardZoomEnded;
        public static event EventHandler WorkitemZoomended;

        public static void InvokeWorkitemZoomended(EventArgs e)
        {
            EventHandler handler = WorkitemZoomended;
            if (handler != null) handler(null, e);
        }

        public void InvokeTaskboardZoomEnded(EventArgs e)
        {
            EventHandler handler = TaskboardZoomEnded;
            if (handler != null) handler(this, e);
        }

        #endregion

        /// <summary>
        /// Creates a new instance of the <see cref="MultiTouchManager"/> class.
        /// </summary>
        /// <param name="owner">Owner of all multi touch events.</param>
        public MultiTouchManager(IInputElement owner)
        {
            _owner = owner;
            _isDragAndDropAction = false;
            _lastCapturedFirstPoint = null;
            _lastCapturedSecondPoint = null;
            _currentMoveEventArgs = null;
        }

        #region TouchDown

        /// <summary>
        /// Processes touchdown events.
        /// </summary>
        /// <param name="e">Touch down event arguments.</param>
        /// <returns>State of event handling.</returns>
        public bool ProcessTouchDown(TouchEventArgs e)
        {
            // If more than two touchpoints are active ignore any additional touch points.
            if (_firstPoint.Key != null && _secondPoint.Key != null)
            {
                return true;
            }

            // If no other multitouch manager is currently working or we are the working one continue.
            if(_operator == null || _operator == this)
            {
                // If there is no current working multitouch manager assign ourself as the working one.
                if (_operator == null)
                {
                    _operator = this;
                }

                // Register touchpoints and store them.
                if (RegisterTouchPoints(e)) return false;
            }
            else
            {
                return e.Handled;
            }

            // This case should never happen if called correctly:
            // We are working and the touch point we get from the agruments is neither the first
            // nor the second of two maximal touch points. So we ignore this event.
            return true;
        }

        /// <summary>
        /// Registers the touchpoints and stores them in the key fields.
        /// </summary>
        /// <param name="e">Returns value for the handled parameter of the event arguments.</param>
        /// <returns>Event handled sate.</returns>
        private bool RegisterTouchPoints(TouchEventArgs e)
        {
            // Register second finger down and save it.
            if (_firstPoint.Key != null && e.TouchDevice.Id != (int)_firstPoint.Key && _secondPoint.Key == null && !_isDragAndDropAction)
            {
                //Disable Scrolling
                DisableTasboardScrolling();

                _secondPoint = new DictionaryEntry(e.TouchDevice.Id, e.GetTouchPoint(_owner));
                AddPositionToQueue(_fingerTwoPositions, ((TouchPoint)_secondPoint.Value).Position.X);
                
                // Two finger operations are always zoom operations.
                IsZooming = true;
                
                // This event cannot be a click and therefor is considered handled.
                return true;
            }

            // Register first finger down an save it.
            if (_firstPoint.Key == null)
            {
                //Enable Scrolling
                EnableTasboardScrolling();

                _firstPoint = new DictionaryEntry(e.TouchDevice.Id, e.GetTouchPoint(_owner));
                AddPositionToQueue(_fingerOnePositions, ((TouchPoint)_firstPoint.Value).Position.X);

                // This event cannot be a click and therefor is considered handled.
                return true;
            }

            return false;
        }

        #region TasboardScrolling

        /// <summary>
        /// Enables TaskboardsScrolling if current owner is Taskboardcontrol
        /// </summary>
        private void EnableTasboardScrolling()
        {
            if (OwnerIsTasbboardControl)
            {
                (_owner as TaskboardControl).EnableTaskboardScrolling();
            }
        }

        /// <summary>
        /// Disables TaskboardsScrolling if current owner is Taskboardcontrol
        /// </summary>
        private void DisableTasboardScrolling()
        {
            if (OwnerIsTasbboardControl)
            {
                (_owner as TaskboardControl).DisableTaskboardScrolling();
            }
        }

        #endregion

        #endregion

        #region TouchUp

        /// <summary>
        /// Processes touchup events.
        /// </summary>
        /// <param name="e">Touch up event arguments.</param>
        /// <returns>State of event handling.</returns>
        public bool ProcessTouchUp(TouchEventArgs e)
        {
            // If no valid touch point is stored mark the event as handled and return.
            // This case should never occur but better be save than sorry.
            if (_firstPoint.Key == null && _secondPoint.Key == null) return true;

            // If no operator exists return.
            // This event should never occur, too. But then again ...
            if (_operator == null) return e.Handled;

            // Unregister any released touch points from the storage.
            return UnregisterTouchPoints(e);
        }

        /// <summary>
        /// Unregisters touchpoints from the storage.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Handled state of the event arguments.</returns>
        private bool UnregisterTouchPoints(TouchEventArgs e)
        {
            bool handledState = false;

            // Register release of first finger and kill touch point.
            if (_firstPoint.Key != null && e.TouchDevice.Id == (int)_firstPoint.Key)
            {
                _firstPoint = new DictionaryEntry(null, null);
                _isDragAndDropAction = false;
                _fingerOnePositions.Clear();
            }

            // Register release of second finger and kill touch point.
            if (_secondPoint.Key != null && e.TouchDevice.Id == (int)_secondPoint.Key)
            {
                _secondPoint = new DictionaryEntry(null, null);
                _fingerTwoPositions.Clear();
                handledState = true;
                IsZooming = false;
                if(OwnerIsTasbboardControl) InvokeTaskboardZoomEnded(null);
                else if(OwnerIsWorkItemControl) InvokeWorkitemZoomended(null);
            }

            // If we do not have any active touch points we are done working and unregister as 
            // currently working multitouch manager to enable the processing of further touch events.
            if (_firstPoint.Key == null && _secondPoint.Key == null)
            {
                _operator = null;
            }

            // Clear queue for delta values, it is only relevant for 2fingergestures, so it is obsolete
            // if a finger, no matter which one, is released
            _deltaQueue.Clear();

            return handledState;
        }

        #endregion

        #region TouchMove

        /// <summary>
        /// Processes touch move events.
        /// </summary>
        /// <param name="e">Touch move event arguments.</param>
        /// <returns>State of event handling.</returns>
        public bool ProcessTouchMove(TouchEventArgs e)
        {
            // This touch move event is related to a drag and drop action we already have identified, we can quit here
            // since drag and drop is handled somewhere else.
            if (_isDragAndDropAction) return PerformSaveReturn();

            // Store event arguments for fast access by processing methods.
            _currentMoveEventArgs = e;

            // TouchMove events are fired as long as a finger is on the touchpanel, even while not moving at all.
            // If there is no actual movement ignore the event.
            // Not filtering this will cause massive performance losses since this event is fired over 100 times a second.
            if(CatchInvalidMoveEvents()) return PerformSaveReturn(); // true

            // If we are not responsible for the move event ignore it.)
            if (_operator == null || _operator != this) return PerformSaveReturn(); // true

            // If there is only one finger on the touch panel it might be a drag and drop action.
            // However drag and drop is only fired if an offset of movement is exceeded.
            if(RegisterDragAndDropAction()) return PerformSaveReturn();

            // Do the actual zoom.
            PerformZoomOperation();

            // This case should never happen. All events should have been handled by now.
            return true;
        }

        /// <summary>
        /// Returns true if two touchpoints are active, otherwise false.
        /// This is used to prevent any kind of drag and drop action 
        /// while a zoom action is active.
        /// </summary>
        /// <returns>Returns true if two touch poins are active.</returns>
        private static bool  PerformSaveReturn()
        {
            if (_firstPoint.Key != null && _secondPoint.Key != null) return true;

            return false;
        }

        /// <summary>
        /// Performs the zooming operation on workitems or the taskboard pane.
        /// </summary>
        private void PerformZoomOperation()
        {
            // Only perform any actions if we have a two finger touch event.
            if (_firstPoint.Key == null || _secondPoint.Key == null) return;

            // If we are not the currently working multitouch manager
            // do not do anything.
            if (_operator != this) return;

            double delta = 0;
            bool hasChanged = false;

            // If the first finger has been moved save it after delta calculation.
            // and finger has moved
            if ((_currentMoveEventArgs.TouchDevice.Id == (int)_firstPoint.Key) && (!_firstPoint.Value.Equals(_currentMoveEventArgs.GetTouchPoint(_owner))))
            {  
                AddPositionToQueue(_fingerTwoPositions, ((TouchPoint) _firstPoint.Value).Position.X);

                //remember previous position of current finger
                var prev = (TouchPoint)_firstPoint.Value;

                //refresh current position
                _firstPoint.Value = _currentMoveEventArgs.GetTouchPoint(_owner);

                if (!QueueMismatch(_fingerOnePositions))
                {
                    //calculate delta for position change
                    delta = GetTouchPositionDelta(_secondPoint.Value as TouchPoint, prev, _firstPoint.Value as TouchPoint);
                    //add delta to delta queue
                    if (delta > 0) AddPositionToQueue(_deltaQueue, delta);

                    //report change
                    hasChanged = true;
                }
            }

            // if the second finger has been moved save it after delta calculation.
            if ((_currentMoveEventArgs.TouchDevice.Id == (int)_secondPoint.Key) && (!_firstPoint.Value.Equals(_currentMoveEventArgs.GetTouchPoint(_owner))))
            {
               AddPositionToQueue(_fingerTwoPositions, ((TouchPoint) _secondPoint.Value).Position.X);

                //remember previous position of current finger
                var prev = (TouchPoint)_secondPoint.Value;

                //refresh current position
                _secondPoint.Value = _currentMoveEventArgs.GetTouchPoint(_owner);

                if (!QueueMismatch(_fingerTwoPositions))
                {
                    //calculate delta for position change
                    delta = GetTouchPositionDelta(_firstPoint.Value as TouchPoint, prev, _secondPoint.Value as TouchPoint);
                    if (delta > 0) AddPositionToQueue(_deltaQueue, delta);
                    
                    //report change
                    hasChanged = true;
                }
            }

            // If we have a valid delta, use it to change the scale of the workitem.
            if (hasChanged && delta > 0)
            {
                //supress errors by using average values from delta queue
                delta = GetCumulativeDelta();
                if (delta > 0) ScaleUiElement(delta);
            }
        }

        /// <summary>
        /// Returns the average delta of growing values (>1) and shrinking values (smaller than 1 and greater than 0)
        /// if the percentage of deltas in queue is greater or equal the ValidityTresholdValue.
        /// 
        /// Returns 0 if ValidityTresholdValue is exceeded or queue is empty
        /// </summary>
        /// <returns>cumulative delta</returns>
        private double GetCumulativeDelta()
        {
            IList growingValues = new List<double>();
            IList shrinkingValues = new List<double>();
            double avg = 0;

            //sort deltas for growing and shrinking values
            foreach (double queuedValue in _deltaQueue)
            {
                if (queuedValue > 1) growingValues.Add(queuedValue);
                else shrinkingValues.Add(queuedValue);
            }

            // if more growing values than necessary by ValidityTresholdValue
            // calculate average growing value
            if (growingValues.Count > _deltaQueue.Count * ValidityTresholdValue)
            {
                foreach (double growingValue in growingValues)
                {
                    avg += growingValue;
                }
                return avg / growingValues.Count;
            }

            // if more shrinking values than necessary by ValidityTresholdValue
            // calculate average shrinking value
            if (shrinkingValues.Count > _deltaQueue.Count * ValidityTresholdValue)
            {
                foreach (double shrinkingValue in shrinkingValues)
                {
                    avg += shrinkingValue;
                }
                return avg / shrinkingValues.Count;
            }

            return 0;
        }

        /// <summary>
        /// Register drag and drop actions.
        /// </summary>
        /// <returns>Returns true if the event initializes a drag and drop action.</returns>
        private bool RegisterDragAndDropAction()
        {
            // If there is only one finger on the touch panel it might be a drag and drop action.
            // However drag and drop is only fired if an offset of movement is exceeded.
            if (_firstPoint.Key != null && _secondPoint.Key == null)
            {
                // Only recognize the move action as a drag and drop event
                // if the touch point exceeds the dragdropoffset.
                if (ChangeIsRelevant(_firstPoint.Value as TouchPoint, _currentMoveEventArgs.GetTouchPoint(_owner), DragAndDropOffset))
                {
                    _isDragAndDropAction = true;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Identifies any invalid move events.
        /// </summary>
        /// <remarks>
        /// TouchMove events are fired continuously. To prevent performance peaks this method
        /// may identify movement event agruments which hold no new event data. These events
        /// can be dismissed without any further analysis.
        /// </remarks>
        /// <returns>Returns true if the event is invalid.</returns>
        private bool CatchInvalidMoveEvents()
        {
            TouchPoint p = _currentMoveEventArgs.GetTouchPoint(_owner);

            // Catches events which hold no new data.
            if (CatchMatchingEventArguments()) return true;

            // Calculate the actual delta between the current an the previous valid touch point.
            // If the delta does not match the defined offset for UI updates, dismiss the event.
            if (_firstPoint.Key != null && _currentMoveEventArgs.TouchDevice.Id == (int)_firstPoint.Key &&
                !ChangeIsRelevant(_firstPoint.Value as TouchPoint, p, UiUpdateOffset)) return true;


            if (_secondPoint.Key != null && _currentMoveEventArgs.TouchDevice.Id == (int)_secondPoint.Key &&
                !ChangeIsRelevant(_secondPoint.Value as TouchPoint, p, UiUpdateOffset)) return true;

            return false;
        }

        /// <summary>
        /// Catches event that have the same position as the previous catched event.
        /// </summary>
        /// <returns>Returns true if the current event position matches the previous event position.</returns>
        private bool CatchMatchingEventArguments()
        {
            if (_firstPoint.Key != null && _currentMoveEventArgs.TouchDevice.Id == (int)_firstPoint.Key)
            {
                if (_lastCapturedFirstPoint == null)
                {
                    _lastCapturedFirstPoint = _currentMoveEventArgs.GetTouchPoint(_owner);
                }
                else
                {
                    if (_lastCapturedFirstPoint.Position.Equals(_currentMoveEventArgs.GetTouchPoint(_owner).Position))
                    {
                        return true;
                    }
                    _lastCapturedFirstPoint = _currentMoveEventArgs.GetTouchPoint(_owner);
                }
            }

            if (_secondPoint.Key != null && _currentMoveEventArgs.TouchDevice.Id == (int)_secondPoint.Key)
            {
                if (_lastCapturedSecondPoint == null)
                {
                    _lastCapturedSecondPoint = _currentMoveEventArgs.GetTouchPoint(_owner);
                }
                else
                {
                    if (_lastCapturedSecondPoint.Position.Equals(_currentMoveEventArgs.GetTouchPoint(_owner).Position))
                    {
                        return true;
                    }
                    _lastCapturedSecondPoint = _currentMoveEventArgs.GetTouchPoint(_owner);
                }
            }

            return FilterInvalidTouchMoves();
        }

        /// <summary>
        /// Returns true if current _owner is of type TaskboardControl
        /// </summary>
        private bool OwnerIsTasbboardControl
        {
            get
            {
                if (_owner.GetType() == typeof(TaskboardControl))
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Returns true if current _owner is of type ReportControl
        /// </summary>
        private bool OwnerIsReportControl
        {
            get
            {
                if (_owner.GetType() == typeof(ReportControl))
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Returns true if current _owner is of type WorkItemControl
        /// </summary>
        private bool OwnerIsWorkItemControl
        {

            get
            {
                if (_owner.GetType() == typeof(WorkItemControl))
                {
                    return true;
                }

                return false;
            }

        }
        
        /// <summary>
        /// Check if an occuring touch event does have a delta in its
        /// coordinates.
        /// </summary>
        /// <returns>Returns true if the touch event does not contain any new information and
        /// false if the touch event has a valid delta.</returns>
        private bool FilterInvalidTouchMoves()
        {
            bool firstPoint = false;
            bool secondPoint = false;

            if (_firstPoint.Value != null)
            {
                if (((TouchPoint)(_firstPoint.Value)).Position == (_currentMoveEventArgs.GetTouchPoint(_owner).Position)) firstPoint = true;
            }
            else
            {
                firstPoint = true;
            }

            if (_secondPoint.Value != null)
            {
                if (((TouchPoint)(_secondPoint.Value)).Position == (_currentMoveEventArgs.GetTouchPoint(_owner).Position)) secondPoint = true;
            }
            else
            {
                secondPoint = true;
            }

            return (firstPoint && secondPoint);
        }

        /// <summary>
        /// Returns true if x or y values of current touchpoint related to old touchpoint
        /// exceed the given offset value.
        /// </summary>
        /// <param name="old">former touchpoint</param>
        /// <param name="curr">current touchpoint</param>
        /// <param name="offset">offset</param>
        /// <returns>true if offset exceeded</returns>
        private static bool ChangeIsRelevant(TouchPoint old, TouchPoint curr, int offset)
        {
            // absolute difference of x values
            double x = Math.Abs(old.Position.X - curr.Position.X);
            // absolute difference of x values
            double y = Math.Abs(old.Position.Y - curr.Position.Y);

            // compare with offset
            if (x > offset)
                return true;

            if (y > offset)
                return true;
            
            // current touchpoint is irrelavant
            return false;
        }

        /// <summary>
        /// Calculate the delta value using a reference point and the previous point
        /// depending on changes on x-axis and y-axis
        /// </summary>
        /// <param name="reference">referencePoint</param>
        /// <param name="previous">previousPoint</param>
        /// <param name="current">currentPoint</param>
        /// <returns>delta</returns>
        private static double GetTouchPositionDelta(TouchPoint reference, TouchPoint previous, TouchPoint current)
        {
            // calculate distance of points before movement
            double previousDistance = GetDistance(previous, reference);
            // calculate distance of points after movement
            double currentDistance = GetDistance(current, reference);

            //return value of changement, so called delta
            return currentDistance / previousDistance;
        }

        /// <summary>
        /// Returns the distance between Point 1 and Point 2.
        /// Calculated with pythagoras.
        /// </summary>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">Point 2</param>
        /// <returns>distance between Point 1 and Point 2</returns>
        private static double GetDistance(TouchPoint p1, TouchPoint p2)
        {
            double x = p2.Position.X - p1.Position.X;
            double y = p2.Position.Y - p1.Position.Y;
            double hypothenuse = Math.Sqrt((x * x) + (y * y));
            return hypothenuse;
        }

        /// <summary>
        /// Scales the target UI element.
        /// </summary>
        /// <param name="delta">Scaling factor.</param>
        private void ScaleUiElement(double delta)
        {
            if (delta == 0) return;

            // exceeding minvalue? 
            if (delta < MinimumDelta)
                delta = MinimumDelta;
            // exceeding maxvalue
            if (delta > MaximumDelta)
                delta = MaximumDelta;

            if (OwnerIsWorkItemControl)
            {
                ScaleWorkItem(delta);
                return;
            }

            if (OwnerIsTasbboardControl)
            {
                ((TaskboardControl)_owner).ZoomFactor *= delta;
                return;
            }

            if (OwnerIsReportControl)
            {
                ScaleReportItem(delta);
                return;
            }

            throw new ArgumentException(
                "This method only supports WorkItemControl and TaskBoardControl touch events.");
        }
        
        /// <summary>
        /// Rescales the workitems RenderTransform matrix to change its size.
        /// </summary>
        /// <param name="delta">Delta for transformation.</param>
        private void ScaleWorkItem(double delta)
        {
            if (delta == 0) return;

            var convertedOwner = _owner as WorkItemControl;

            if (convertedOwner == null)
            {
                throw new NullReferenceException("This method may only be called for scaling WorkItemControl objects.");
            }

            // Perform workitem zoom.

            // Use LayoutTransform instead of RenderTransform to automatically 
            // align space between single workitems
            MatrixTransform renderTransformObject = convertedOwner.LayoutTransform as MatrixTransform;

            if (renderTransformObject != null)
            {
                Matrix matrix = renderTransformObject.Matrix;

                matrix.Scale(delta, delta);
                MatrixTransform newTransformObject = new MatrixTransform(matrix);
                ((WorkItemControl)_owner).LayoutTransform = newTransformObject;

            }
        }

        /// <summary>
        /// Rescales the reportitems RenderTransform matrix to change its size.
        /// </summary>
        /// <param name="delta">Delta for transformation.</param>
        private void ScaleReportItem(double delta)
        {
            if (delta == 0 || delta == 1) return;

            var convertedOwner = _owner as ReportControl;
            if(convertedOwner!=null)
            {
                ReportViewer rv = convertedOwner.viewerInstance;
                rv.ZoomMode = ZoomMode.Percent;
                int currZoomValue = rv.ZoomPercent;
                double newZoomValue = currZoomValue * delta;
                rv.ZoomPercent = (int) newZoomValue;
               
                
            }
            else
            {
                throw new NullReferenceException("This method may only be called for scaling ReportControl objects.");
            }
        }

        #region Store previous finger positions in queue to prevent flickering of workitems

        /// <summary>
        /// add current position as last item to given queue. if queue capacity is
        /// reached first item is automaticly removed.
        /// </summary>
        /// <param name="q">queue</param>
        /// <param name="position">position</param>
        private static void AddPositionToQueue(Queue q, double position)
        {
            //if max_capacity is reached remove first item
            if (q.Count.Equals(MaxQueueLength)) q.Dequeue();
            
            //append item
            q.Enqueue(position);
        }

        /// <summary>
        /// check given queue for mismatch. contained values must be
        /// constantly increasing or decreasing to be valid. mismatch
        /// is intepreted as invalid.
        /// </summary>
        /// <param name="q">queue to be checked for mismatch</param>
        /// <returns>true if no mismatch found</returns>
        private static bool QueueMismatch(Queue q)
        {
            bool incr = false;
            bool decr = false;

            //convert queue to array
            var arr = q.ToArray();
            //store first value as prev
            double prev = (double)arr[0];

            //if queue has more than one item
            for (int i = 1; i < q.Count; i++)
            {
                //compare current value with previous
                double curr = (double)arr[i];
                //if it is the first comparision, notice
                //if values are increasing or decreasing
                if (i == 1)
                {
                    if (prev < curr)
                    {
                        incr = true;
                    }
                    else
                    {
                        decr = true;
                    }
                }

                //if values in queue are increasing but current value
                //is smaller than previous -> report mismatch
                if ((incr) && (prev > curr))
                {
                    return true;
                }

                //if values in queue are decreasing but current value
                //is greater than previous -> report mismatch
                if ((decr) && (prev < curr))
                {
                    return true;
                }

                //no errors so far, store current value as prevoius and
                //go to next value in the queue
                prev = curr;
            }

            return false;
        }

        #endregion

        #endregion

        #region Gestures

        private bool panningInProgress = false;
        private long initialPanValue;
        private long currentPanningValue;

        private bool ZoomingInProgress = false;
        private long initialZoomValue;
        private long currentZoomValue;
       

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

            if (ZoomingInProgress)
            {
                // Process changing ZoomValue relative to first zoomvalue
                currentZoomValue = gestureEventArgs.Arguments;
                
            }
            else
            {
                // Notify 1st Zoom Message has been processed
                // and remember first zoomvalue
                ZoomingInProgress = true;
                initialZoomValue = gestureEventArgs.Arguments;
            }
        }

        /// <summary>
        /// Process GesturePanMessage
        /// </summary>
        /// <param name="gestureEventArgs"></param>
        public void ProcessGesturePan(GestureEventArgs gestureEventArgs)
        {
            if (panningInProgress)
            {
                
                /* TODO: implement panning
                 * */
            }
            else
            {
                // Notify 1st Zoom Message has been processed
                // and remember first zoomvalue
                panningInProgress = true;
                initialPanValue = gestureEventArgs.Arguments;
            }
        }

        /// <summary>
        /// Process GestureEndMessage, Finish Gesture, reset temporary values
        /// </summary>
        /// <param name="gestureEventArgs"></param>
        public void ProcessGestureEnd(GestureEventArgs gestureEventArgs)
        {
            if(ZoomingInProgress)
            {
                double val = (currentZoomValue - initialZoomValue);

                try
                {
                    ScaleReportItem(Math.Abs(val / initialZoomValue));
                }
                catch (Exception e)
                {
                    Debug.Print(e.Message + ", " + e.StackTrace);
                }

                ZoomingInProgress = false;
            }

            if(panningInProgress)
            {
                panningInProgress = false;
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

      #endregion
    }
}
