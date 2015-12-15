using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace AIT.Taskboard.Interface
{
    public static class Constants
    {
        /// <summary>
        /// Constant defines the name of company.
        /// </summary>
        public const string ApplicationCompany = "AIT AG";

        /// <summary>
        /// Constant defines the name of application.
        /// </summary>
        public const string ApplicationName = "AIT Taskboard";

        /// <summary>
        /// Subfolder with all help related files.
        /// </summary>
        public const string HelpFileSubfolder = "Help";

        /// <summary>
        /// Name of the log file
        /// </summary>
        public const string LogFileName = "AIT.Taskboard.Log.txt";

        public static readonly Color[] PresetColors = new Color[]
        {
            Color.FromArgb(0xFF, 0xFF, 0x80, 0x80),
            Color.FromArgb(0xFF, 0xFF, 0xC0, 0x80),
            Color.FromArgb(0xFF, 0xFF, 0xFF, 0x80),
            Color.FromArgb(0xFF, 0x80, 0xFF, 0x80),
            Color.FromArgb(0xFF, 0x80, 0xFF, 0xFF),
            Color.FromArgb(0xFF, 0x80, 0x80, 0xFF),
            Color.FromArgb(0xFF, 0xFF, 0x80, 0xFF),

            Color.FromArgb(0xFF, 0xFF, 0x00, 0x00),
            Color.FromArgb(0xFF, 0xFF, 0x80, 0x00),
            Color.FromArgb(0xFF, 0xFF, 0xFF, 0x00),
            Color.FromArgb(0xFF, 0x00, 0xFF, 0x00),
            Color.FromArgb(0xFF, 0x00, 0xFF, 0xFF),
            Color.FromArgb(0xFF, 0x00, 0x00, 0xFF),
            Color.FromArgb(0xFF, 0xFF, 0x00, 0xFF),

            Color.FromArgb(0xFF, 0xC0, 0x00, 0x00),
            Color.FromArgb(0xFF, 0xC0, 0x40, 0x00),
            Color.FromArgb(0xFF, 0xC0, 0xC0, 0x00),
            Color.FromArgb(0xFF, 0x00, 0xC0, 0x00),
            Color.FromArgb(0xFF, 0x00, 0xC0, 0xC0),
            Color.FromArgb(0xFF, 0x00, 0x00, 0xC0),
            Color.FromArgb(0xFF, 0xC0, 0x00, 0xC0)
        };
    }
}
