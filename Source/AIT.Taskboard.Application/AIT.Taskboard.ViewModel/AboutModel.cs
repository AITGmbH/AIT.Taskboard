using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Deployment.Application;
using System.Reflection;
using AIT.Taskboard.ViewModel.Properties;

namespace AIT.Taskboard.ViewModel
{
    [Export]
    public class AboutModel : INotifyPropertyChanged
    {
        #region Fields

        private static Assembly _representativeAssembly;
        private string _licenseKey;
        private string _licenseName;

        #endregion

        #region Constructor

        

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the assembly name.
        /// </summary>
        public static string AssemblyName
        {
            get
            {
                // LAR: 13.05.2010: The assembly name was stripped. Check file history to get previous version
                return Resources.WorkItemTaskBoard;
            }
        }

        /// <summary>
        /// Gets the assembly version
        /// </summary>
        public static string AssemblyVersion
        {
            get
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    return
                        ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
                }

                // Get all Description attributes on this assembly
                object[] attributes = RepresentativeAssembly.GetCustomAttributes(
                    typeof(AssemblyVersionAttribute), false);

                // If there aren't any Description attributes, return an empty string
                if (attributes.Length == 0)
                    return RepresentativeAssembly.GetName().Version.ToString();

                // If there is a Description attribute, return its value
                return ((AssemblyVersionAttribute)attributes[0]).Version;
            }
        }

        /// <summary>
        /// Gets the assembly copyright.
        /// </summary>
        public static string AssemblyCopyright
        {
            get
            {
                // Get all Description attributes on this assembly
                object[] attributes = RepresentativeAssembly.GetCustomAttributes(
                    typeof(AssemblyCopyrightAttribute), false);

                // If there aren't any Description attributes, return an empty string
                if (attributes.Length == 0)
                    return string.Empty;

                // If there is a Description attribute, return its value
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        /// <summary>
        /// Gets the assembly description.
        /// </summary>
        /// <value>The assembly description.</value>
        public static string AssemblyDescription
        {
            get
            {
                // Get all Description attributes on this assembly
                object[] attributes = RepresentativeAssembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute),
                                                                                 false);

                // If there aren't any Description attributes, return an empty string
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }

                // If there is a Description attribute, return its value
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        /// <summary>
        /// Gets the representative assembly.
        /// </summary>
        /// <value>The representative assembly.</value>
        private static Assembly RepresentativeAssembly
        {
            get
            {
                if (_representativeAssembly == null)
                {
                    _representativeAssembly = Assembly.GetExecutingAssembly();

                    if (_representativeAssembly == null)
                    {
                        _representativeAssembly = Assembly.GetEntryAssembly();
                    }
                }

                return _representativeAssembly;
            }
        }
        
        /// <summary>
        /// Gets or set license key.
        /// </summary>
        public string LicenseKey
        {
            get
            {
                return _licenseKey;
            }
            set
            {
                _licenseKey = value;
                OnPropertyChanged("LicenseKey");
            }
        }
        /// <summary>
        /// Gets or set license name.
        /// </summary>
        public string LicenseName
        {
            get
            {
                return _licenseName;
            }
            set
            {
                _licenseName = value;
                OnPropertyChanged("LicenseName");
            }
        }

        /// <summary>
        /// Info about actual license.
        /// </summary>
        public string LicenseInfo
        {
            get
            {
                string licenseInfo = Resources.LIC_PremiumNotRegistered;
                return licenseInfo;
            }
        }
        /// <summary>
        /// Gets whether is premium edition registered.
        /// </summary>
        public bool IsPremiumRegistered
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets whether is premium trial edition.
        /// </summary>
        public bool IsPremiumTrial
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets whether is premium edition registered
        /// </summary>
        public bool IsPremiumUnregistered
        {
            get
            {
                return !IsPremiumRegistered;
            }
        }

        #endregion

        #region Private properties

        #endregion Private properties

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
