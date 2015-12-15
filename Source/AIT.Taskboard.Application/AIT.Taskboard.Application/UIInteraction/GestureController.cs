/**
 * GestureController
 * 
 * Author:  Martin Bialke
 * Date:    22082011
 * 
 * Class GestureController offers Support to process WM_Gesture Messages for WPF.
 */

using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;

namespace AIT.Taskboard.Application.UIInteraction
{

    public class GestureController
    {
        private IntPtr _handle;

        #region Imports

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetGestureInfo(IntPtr hGestureInfo, ref GESTUREINFO pGestureInfo);
        
        #endregion

        #region Constants

        private const int GID_BEGIN = 1;
        private const int GID_END = 2;
        private const int GID_ZOOM = 3;
        private const int GID_PAN = 4;
        private const int GID_ROTATE = 5;
        private const int GID_TWOFINGERTAP = 6;
        private const int GID_PRESSANDTAP = 7;

        private const int WM_GESTURE = 0x119;

        #endregion

        public GestureController()
        {
            _handle = new WindowInteropHelper(System.Windows.Application.Current.MainWindow).Handle;
            RegisterHandleForGestures();
        }
        
        #region Methods

        /// <summary>
        /// Register Window for WM_Gesture Messages
        /// </summary>
        /// <param name="w">Window to process Messages</param>
        public void RegisterWindowForGestures(Window w)
        {
            var winHelper = new WindowInteropHelper(w);
            _handle = winHelper.Handle;
            RegisterHandleForGestures();
        }

        /// <summary>
        /// Register WindowHandle to process WM_Gesture Messages
        /// </summary>
        public void RegisterHandleForGestures()
        {
            HwndSource hwnd = HwndSource.FromHwnd(_handle);
            if (hwnd != null) hwnd.AddHook(MessageProc);
            else Debug.Print("HWND is null");
        }

        /// <summary>
        /// This method is the WPF equivalent to win32 WndProc.  The app receives all the 
        /// Windows messages.  The app can then recognize and respond to the flick or gesture.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="msgID"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="bHandled"></param>
        /// <returns></returns>
        public IntPtr MessageProc(IntPtr hwnd, int msgID, IntPtr wParam, IntPtr lParam, ref bool bHandled)
        {
            if (msgID == WM_GESTURE)
            {
                DecodeGesture(hwnd, msgID, wParam, lParam, ref bHandled);
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Decode WM_Gesture Message using wParam and raise suitable Events
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="mgsID"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="bHandled"></param>
        /// <returns></returns>
        private bool DecodeGesture(IntPtr hwnd, int mgsID, IntPtr wParam, IntPtr lParam, ref bool bHandled)
        {
            GESTUREINFO gi = new GESTUREINFO();

            gi.cbSize = Marshal.SizeOf(new GESTUREINFO());

            // Load the gesture information.
            // We must p/invoke into user32 [winuser.h]
            if (!GetGestureInfo(lParam, ref gi))
            {
                return false;
            }

            try
            {
                switch (gi.dwID)
                {
                    case GID_BEGIN:
                        GestureBegin(_handle,new GestureEventArgs(gi));
                        break;
                    case GID_END:
                        GestureEnd(_handle, new GestureEventArgs(gi));
                        break;

                    case GID_ZOOM:
                        GestureZoom(_handle, new GestureEventArgs(gi));
                        break;

                    case GID_PAN:
                        GesturePan(_handle, new GestureEventArgs(gi));
                        break;

                    case GID_PRESSANDTAP:
                        GesturePressAndTap(_handle, new GestureEventArgs(gi));
                        break;

                    case GID_ROTATE:
                        GestureRotate(_handle, new GestureEventArgs(gi));
                        break;

                    case GID_TWOFINGERTAP:
                        GestureTwoFingerTap(_handle, new GestureEventArgs(gi));
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.Print(this.GetType()+":DecodeGestureError:  "+ex.Message);
            }
            
            

            return true;
        }
        
        #endregion

        #region Events

        public delegate void GestureHandler(object sender, GestureEventArgs e);

        public event GestureHandler GestureBegin;
        public event GestureHandler GestureEnd;
        public event GestureHandler GestureZoom;
        public event GestureHandler GesturePan;
        public event GestureHandler GestureRotate;
        public event GestureHandler GestureTwoFingerTap;
        public event GestureHandler GesturePressAndTap;

        #endregion
    }

    #region EventArgs

    /// <summary>
    /// Class GestureEventArgs
    /// </summary>
    public class GestureEventArgs:EventArgs
    {
        private Point _point;
        private GESTUREINFO _gi;
        private long _args;

        public GestureEventArgs(GESTUREINFO gi)
        {
            _point = new Point(gi.ptsLocation.x,gi.ptsLocation.y);
            _args = gi.ullArguments;
            _gi = gi;
        }

        /// <summary>
        /// Get the arguments containing the relevant change value
        /// </summary>
        public long Arguments
        {
            get { return _args; }
        }

        /// <summary>
        /// Get the complete GestureInfo
        /// </summary>
        public GESTUREINFO Raw
        {
            get { return _gi; }
        }

        /// <summary>
        /// Gets the current FingerPosition
        /// </summary>
        public Point Location
        {
            get { return _point; }
        }
    }

    #endregion
    
    #region Structs
    /// <summary>
    /// Struct GestureInfo holds all necessary GestureData
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct GESTUREINFO
    {
        public int cbSize;           // size, in bytes, of this structure
        // (including variable length Args 
        // field)
        public int dwFlags;          // see GF_* flags
        public int dwID;             // gesture ID, see GID_* defines
        public IntPtr hwndTarget;    // handle to window targeted by this 
        // gesture
        [MarshalAs(UnmanagedType.Struct)]
        internal POINTS ptsLocation; // current location of this gesture
        public int dwInstanceID;     // internally used
        public int dwSequenceID;     // internally used
        public Int64 ullArguments;   // arguments for gestures whose 
        // arguments fit in 8 BYTES
        public int cbExtraArgs;      // size, in bytes, of extra arguments, 
        // if any, that accompany this gesture
    }

    /// <summary>
    /// Struct Points
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct POINTS
    {
        public short x;
        public short y;
    }
    #endregion
    
}