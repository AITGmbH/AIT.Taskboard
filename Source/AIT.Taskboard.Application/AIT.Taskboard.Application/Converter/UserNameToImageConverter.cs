using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.IO;
using System.Reflection;

namespace AIT.Taskboard.Application.Converter
{
    public class UserNameToImageConverter : IValueConverter
    {
        private static readonly Dictionary<string, string> FoundImages = new Dictionary<string, string>();
        static int _lastDefaultUser;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var userName = value as string;
            // NO user name ... no picture
            if (userName == null) return Binding.DoNothing;
            // try to reuse found images
            if (FoundImages.ContainsKey(userName)) return FoundImages[userName];
            
            var defaultUserImage = GetDefaultUserImage();
            var userImageName = GetUserImage(userName);
            var resultImage = defaultUserImage;
            // If there is a user specific image file we will use that
            if (File.Exists(userImageName)) resultImage = userImageName;
            FoundImages.Add(userName, resultImage);
            return resultImage;
        }

        private static string GetUserImage(string userName)
        {
            string userImageName = string.Format("{0}.png", userName);
            Assembly exeAssembly = Assembly.GetExecutingAssembly();
            string exeDirectory = Path.GetDirectoryName (exeAssembly.CodeBase);            
            userImageName = Path.Combine(exeDirectory, userImageName);
            Uri uri = new Uri(userImageName);
            return uri.LocalPath;
        }

        private static string GetDefaultUserImage()
        {
            string defaultUserImage = string.Format("SmallImages\\User{0}.png", _lastDefaultUser+1);
            // Ensure to pick another user image the next time
            _lastDefaultUser = ++_lastDefaultUser % 3;
            return defaultUserImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
