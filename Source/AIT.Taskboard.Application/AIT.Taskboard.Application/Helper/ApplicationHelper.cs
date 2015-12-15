using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AIT.Taskboard.Interface;
using AIT.Taskboard.ViewModel;

namespace AIT.Taskboard.Application.Helper
{
    class ApplicationHelper
    {
        public static string FileToOpen {get; set;}
        private static string HelpSourceDirectory
        {
            get
            {
                var asmUri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
                return Path.Combine(Path.GetDirectoryName(asmUri.AbsolutePath), Constants.HelpFileSubfolder);
            }
        }
        internal static string HelpTargetDirectory
        {
            get
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                           Constants.ApplicationCompany);
                path = Path.Combine(path, Constants.ApplicationName);
                path = Path.Combine(path, AboutModel.AssemblyVersion);
                path = Path.Combine(path, Constants.HelpFileSubfolder);
                string helpDirectory = path;
                Directory.CreateDirectory(helpDirectory);
                return helpDirectory;
            }
        }
        public static void PrepareHelp(string fileName)
        {
            string helpSource = Path.Combine(HelpSourceDirectory, fileName);
            string helpTarget = Path.Combine(HelpTargetDirectory, fileName);

            Assembly assembly = Assembly.GetExecutingAssembly();
            {
                if (File.Exists(helpSource))
                {
                    File.Copy(helpSource, helpTarget);
                }
                else
                {
                    Stream readStream = assembly.GetManifestResourceStream(string.Format(
                                                                               CultureInfo.InvariantCulture,
                                                                               "AIT.Taskboard.Application.Help.{0}",
                                                                               fileName));
                    if (readStream != null)
                    {
                        Stream writeStream = new FileStream(helpTarget, FileMode.Create, FileAccess.Write);
                        ReadWriteStream(readStream, writeStream);
                    }
                }
            }
        }

        /// <summary>
        /// Method copies the file content from one file to other file.
        /// </summary>
        /// <param name="readStream">Stream to copy from.</param>
        /// <param name="writeStream">Stream to copy to.</param>
        private static void ReadWriteStream(Stream readStream, Stream writeStream)
        {
            const int length = 256;
            var buffer = new byte[length];
            int bytesRead = readStream.Read(buffer, 0, length);

            while (bytesRead > 0)
            {
                writeStream.Write(buffer, 0, bytesRead);
                bytesRead = readStream.Read(buffer, 0, length);
            }

            readStream.Close();
            writeStream.Close();
        }
    }
}
