using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;
using AIT.Taskboard.Interface;

namespace AIT.Taskboard.Application.EmailCreation
{
    [Export(typeof (IEmailCreation))]
    internal class EmailCreationService : IEmailCreation
    {
        #region IEmalCreation implementation

        public int CreateEmail(MailItem mailItem)
        {
            AddRecipient(mailItem.RecipientTo, HowTo.MAPI_TO);
            AddRecipient(mailItem.RecipientCC, HowTo.MAPI_CC);
            AddRecipient(mailItem.RecipientBCC, HowTo.MAPI_BCC);

            _mAttachments.Add(mailItem.Attachement);

            return SendMail(mailItem.Subject, mailItem.Body, MAPI_LOGON_UI | MAPI_DIALOG);
        }

        #endregion

        #region Public Methods

        public string GetLastError()
        {
            if (_mLastError <= 26)
                return _errors[_mLastError];
            return "MAPI error [" + _mLastError.ToString(CultureInfo.InvariantCulture) + "]";
        }

        #endregion

        #region Private Fields

        private readonly string[] _errors = new[]
                                                {
                                                    "OK [0]", "User abort [1]", "General MAPI failure [2]",
                                                    "MAPI login failure [3]",
                                                    "Disk full [4]", "Insufficient memory [5]", "Access denied [6]",
                                                    "-unknown- [7]",
                                                    "Too many sessions [8]", "Too many files were specified [9]",
                                                    "Too many recipients were specified [10]",
                                                    "A specified attachment was not found [11]",
                                                    "Attachment open failure [12]", "Attachment write failure [13]",
                                                    "Unknown recipient [14]", "Bad recipient type [15]",
                                                    "No messages [16]", "Invalid message [17]", "Text too large [18]",
                                                    "Invalid session [19]",
                                                    "Type not supported [20]",
                                                    "A recipient was specified ambiguously [21]", "Message in use [22]",
                                                    "Network failure [23]",
                                                    "Invalid edit fields [24]", "Invalid recipients [25]",
                                                    "Not supported [26]"
                                                };


        private readonly List<MapiRecipDesc> _mRecipients = new List<MapiRecipDesc>();
        private readonly List<string> _mAttachments = new List<string>();
        private int _mLastError;

        // ReSharper disable InconsistentNaming
        private const int MAPI_LOGON_UI = 0x00000001;
        // ReSharper restore InconsistentNaming
        // ReSharper disable InconsistentNaming
        private const int MAPI_DIALOG = 0x00000008;
        // ReSharper restore InconsistentNaming
        // ReSharper disable InconsistentNaming
        private const int maxAttachments = 20;
        // ReSharper restore InconsistentNaming


        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedMember.Local
        private enum HowTo
        {
            MAPI_ORIG = 0,
            MAPI_TO,
            MAPI_CC,
            MAPI_BCC
        };

        // ReSharper restore UnusedMember.Local
        // ReSharper restore InconsistentNaming

        #endregion

        #region Private Methods

        [DllImport("MAPI32.DLL")]
        private static extern int MAPISendMail(IntPtr sess, IntPtr hwnd, MapiMessage message, int flg, int rsv);

        private int SendMail(string strSubject, string strBody, int how)
        {
            var msg = new MapiMessage {subject = strSubject, noteText = strBody};

            msg.recips = GetRecipients(out msg.recipCount);
            msg.files = GetAttachments(out msg.fileCount);

            _mLastError = MAPISendMail(new IntPtr(0), new IntPtr(0), msg, how, 0);

            Cleanup(ref msg);
            return _mLastError;
        }

        private void AddRecipient(string email, HowTo howTo)
        {
            var recipient = new MapiRecipDesc {recipClass = (int) howTo, name = email};

            _mRecipients.Add(recipient);
        }

        private IntPtr GetRecipients(out int recipCount)
        {
            recipCount = 0;
            if (_mRecipients.Count == 0)
                return IntPtr.Zero;

            var size = Marshal.SizeOf(typeof (MapiRecipDesc));
            var intPtr = Marshal.AllocHGlobal(_mRecipients.Count*size);

            var ptr = (int) intPtr;
            foreach (var mapiDesc in _mRecipients)
            {
                Marshal.StructureToPtr(mapiDesc, (IntPtr) ptr, false);
                ptr += size;
            }

            recipCount = _mRecipients.Count;
            return intPtr;
        }

        private IntPtr GetAttachments(out int fileCount)
        {
            fileCount = 0;
            if (_mAttachments == null)
                return IntPtr.Zero;

            if ((_mAttachments.Count <= 0) || (_mAttachments.Count > maxAttachments))
                return IntPtr.Zero;

            var size = Marshal.SizeOf(typeof (MapiFileDesc));
            var intPtr = Marshal.AllocHGlobal(_mAttachments.Count*size);

            var mapiFileDesc = new MapiFileDesc {position = -1};
            var ptr = (int) intPtr;

            foreach (var strAttachment in _mAttachments)
            {
                mapiFileDesc.name = Path.GetFileName(strAttachment);
                mapiFileDesc.path = strAttachment;
                Marshal.StructureToPtr(mapiFileDesc, (IntPtr) ptr, false);
                ptr += size;
            }

            fileCount = _mAttachments.Count;
            return intPtr;
        }

        private void Cleanup(ref MapiMessage msg)
        {
            var size = Marshal.SizeOf(typeof (MapiRecipDesc));
            int ptr;

            if (msg.recips != IntPtr.Zero)
            {
                ptr = (int) msg.recips;
                for (var i = 0; i < msg.recipCount; i++)
                {
                    Marshal.DestroyStructure((IntPtr) ptr, typeof (MapiRecipDesc));
                    ptr += size;
                }
                Marshal.FreeHGlobal(msg.recips);
            }

            if (msg.files != IntPtr.Zero)
            {
                size = Marshal.SizeOf(typeof (MapiFileDesc));

                ptr = (int) msg.files;
                for (var i = 0; i < msg.fileCount; i++)
                {
                    Marshal.DestroyStructure((IntPtr) ptr, typeof (MapiFileDesc));
                    ptr += size;
                }
                Marshal.FreeHGlobal(msg.files);
            }

            _mRecipients.Clear();
            _mAttachments.Clear();
            _mLastError = 0;
        }

        #endregion
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class MapiMessage
    {
        public int reserved;
        public string subject;
        public string noteText;
        public string messageType;
        public string dateReceived;
        public string conversationID;
        public int flags;
        public IntPtr originator;
        public int recipCount;
        public IntPtr recips;
        public int fileCount;
        public IntPtr files;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class MapiFileDesc
    {
        public int reserved;
        public int flags;
        public int position;
        public string path;
        public string name;
        public IntPtr type;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class MapiRecipDesc
    {
        public int reserved;
        public int recipClass;
        public string name;
        public string address;
        public int eIDSize;
        public IntPtr entryID;
    }
}
