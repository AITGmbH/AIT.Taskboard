//using System;
//using System.ComponentModel.Composition;
//using System.Globalization;
//using AIT.Taskboard.Interface.Licensing;
//using AIT.Taskboard.Model.Properties;
//using LogicNP.CryptoLicensing;
//using System.IO;

//namespace AIT.Taskboard.Model.Licensing
//{ 
//    // [Export (typeof(ILicensingService))]
//    public class LicensingService : ILicensingService
//    {
//        #region Constants

//        // License version number is validated agianst the license key.
//        private const int AppLicenseVersionNo = 1;
//        // Get the validation key using Ctrl+K in the Generator UI.
//        private const string ValidationKey = "AMAAMACGXCVg90KvhFaQ00Zz6K1HOOTr8wlwwHkqxO7dZwNeBd//pvVZNi3L6DNKHp1wdW8DAAEAAQ==";
//        private const string TrialLicenseCode = "tgIAAMo8qZd0EMwByrwNkAcozAEeABcATmFtZT0jQ29tcGFueT0jQWRkcmVzcz0ivVHq7fS8JQEfVZSjKrGq0AK4ORPqiVXdJwXOT69EopgARJPRDgyAd77qfgqniaw=";
//        private const string ValidationServiceUrl = "https://licensing.aitag.com/Service.asmx";
//        private const string LicenseServiceSettingsFile = "Settings.xml";
//        private const char UserDataSeparator = '#';
//        private const int NameUserDataIndex = 0;
//        private const int CompanyUserDataIndex = 1;
//        private const int AddressUserDataIndex = 2;
//        private const int LicenseVersionNoUserDataIndex = 3; 

//        #endregion Constants

//        #region Fields

//        private CryptoLicense _cryptoLicense;
//        private string _lastRegistrationStatusError;
//        private Exception _lastRegistrationStatusException;
//        private string LicenseServiceSettingsPath;
        
//        #endregion Fields

//        #region Constructor
//        public LicensingService()
//        {
//            //get the executing application path
//            var appDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
//            LicenseServiceSettingsPath = Path.Combine(appDir, LicenseServiceSettingsFile);
 
//            _cryptoLicense = new CryptoLicense();
//            // set validation key
//            _cryptoLicense.ValidationKey = ValidationKey;
//            // set path for license service where is Settings.xml file located
//            _cryptoLicense.LicenseServiceSettingsFilePath = LicenseServiceSettingsPath;
//            // The license will be loaded from/saved to the registry
//            _cryptoLicense.StorageMode = LicenseStorageMode.ToRegistry;
//            // get licencse from registry
//            IntitializeLicense();
//        }
//        #endregion Constructor

//        #region Implementation of ILicensingService

//        #region Public properties

//        // <summary>
//        /// Gets whether add child work item functionality is enabled.
//        /// </summary>
//        public bool IsAddChildWorkItemsEnabled
//        {
//            get
//            {
//                return IsPremiumTrial || IsPremiumRegistered;
//            }
//        }

//        /// <summary>
//        /// Gets whether iteration planner functionality is enabled.
//        /// </summary>
//        public bool IsIterationPlannerEnabled
//        {
//            get
//            {
//                return IsPremiumTrial || IsPremiumRegistered;
//            }
//        }

//        /// <summary>
//        /// Gets whether multitouch functionality is enabled.
//        /// </summary>
//        public bool IsMultitouchSupportEnabled
//        {
//            get
//            {
//                return IsPremiumTrial || IsPremiumRegistered;
//            }
//        }

//        /// <summary>
//        /// Gets whether kinect functionality is enabled.
//        /// </summary>
//        public bool IsKinectSupportEnabled
//        {
//            get
//            {
//                return IsPremiumTrial || IsPremiumRegistered;
//            }
//        }
//        /// <summary>
//        /// Gets wheter premium edition is in trial mode.
//        /// </summary>
//        public bool IsPremiumTrial
//        {
//            get
//            {
//                return _cryptoLicense.IsEvaluationLicense() && _cryptoLicense.IsEvaluationExpired() == false;
//            }
//        }

//        /// <summary>
//        /// Gets whether premium trial duration has expired.
//        /// </summary>
//        public bool IsPremiumTrialExpired
//        {
//            get
//            {
//                return _cryptoLicense.IsEvaluationLicense() && _cryptoLicense.IsEvaluationExpired() == true;
//            }
//        }

//        /// <summary>
//        /// Gets remaining days for premium trial.
//        /// </summary>
//        public short PremiumTrialDay
//        {
//            get
//            {
//                return _cryptoLicense.CurrentUsageDays;
//            }
//        }

//        /// <summary>
//        /// Gets max usage days of premium trial.
//        /// </summary>
//        public short PremiumTrialMaxUsageDays
//        {
//            get
//            {
//                return _cryptoLicense.MaxUsageDays;
//            }
//        }

//        /// <summary>
//        /// Gets whether premium edition is registered.
//        /// </summary>
//        public bool IsPremiumRegistered
//        {
//            get
//            {
//                return _cryptoLicense.IsEvaluationLicense() == false && _cryptoLicense.Status == LicenseStatus.Valid;
//            }
//        }

//        /// <summary>
//        /// Gets license user name.
//        /// </summary>
//        public string LicenseUserName
//        {
//            get
//            {
//                return GetLicenseUserData(_cryptoLicense.UserData, NameUserDataIndex);
//            }
//        }

//        /// <summary>
//        /// Gets license company.
//        /// </summary>
//        public string LicenseCompany
//        {
//            get
//            {
//                return GetLicenseUserData(_cryptoLicense.UserData, CompanyUserDataIndex);
//            }
//        }

//        /// <summary>
//        /// Gets license address.
//        /// </summary>
//        public string LicenseAddress
//        {
//            get
//            {
//                return GetLicenseUserData(_cryptoLicense.UserData, AddressUserDataIndex);
//            }
//        }

//        #endregion Public properties

//        #region Public methods
//        /// <summary>
//        /// Gets detailed error from registration process. 
//        /// </summary>
//        public string GetRegistrationLicenseStatusError()
//        {
//            return _lastRegistrationStatusError;
//        }
//        /// <summary>
//        /// Gets detailed exception, if any, associated with a license status code from registration process. 
//        /// </summary>
//        public Exception GetRegistrationLicenseStatusException()
//        {
//            return _lastRegistrationStatusException;
//        }
//        /// <summary>
//        /// Register application.
//        /// </summary>
//        /// <param name="licenseKey">License key.</param>
//        /// <param name="licenseName">License name.</param>
//        /// <returns>Registration result.</returns>
//        public void Register(string licenseKey, string licenseName)
//        {
//            // reset error objects
//            _lastRegistrationStatusError = string.Empty;
//            _lastRegistrationStatusException = null;

//            // Check license user name. This must be done in other CryptoLicense instance to avoid activation validation via web service.
//            var cryptoLicense = new CryptoLicense(ValidationKey);
//            cryptoLicense.LicenseCode = licenseKey;
//            // if the license key is invalid then get error and exit
//            if (cryptoLicense.Status != LicenseStatus.Valid &&
//                cryptoLicense.Status != LicenseStatus.ActivationFailed)
//            {
//                _lastRegistrationStatusError = GetRegistrationLicenseStatusMessage(cryptoLicense.Status);
//                _lastRegistrationStatusException = cryptoLicense.GetStatusException(cryptoLicense.Status);
//                cryptoLicense.Dispose();
//                return;
//            }
//            // validate LicenseVersionNo value 
//            if (GetLicenseKeyVersionNo(cryptoLicense.UserData) < AppLicenseVersionNo)
//            {
//                _lastRegistrationStatusError = Resources.LicenseRegistration_WrongVersion;
//                cryptoLicense.Dispose();
//                return;
//            }
//            // if license user name is invalid then get error and exit
//            if (GetLicenseUserData(cryptoLicense.UserData, NameUserDataIndex) != licenseName)
//            {
//                _lastRegistrationStatusError = Resources.LicenseRegistration_UserNameFailed;
//                cryptoLicense.Dispose();
//                return;
//            }
//            cryptoLicense.Dispose();
//            cryptoLicense = null;

//            // licensing service url
//            _cryptoLicense.LicenseServiceURL = ValidationServiceUrl;

//            // set license code
//            _cryptoLicense.LicenseCode = licenseKey;

//            // if premium is not registered then get error and exit
//            if (IsPremiumRegistered == false)
//            {
//                _lastRegistrationStatusError = GetRegistrationLicenseStatusMessage(_cryptoLicense.Status);
//                _lastRegistrationStatusException = _cryptoLicense.GetStatusException(_cryptoLicense.Status);
//                return;
//            }

//            // save new license key into storage object (registry)
//            _cryptoLicense.Save();
//        }

//        /// <summary>
//        /// Terminate actual license.
//        /// </summary>
//        public void TerminateLicense()
//        {
//            // licensing service url
//            _cryptoLicense.LicenseServiceURL = ValidationServiceUrl;
//            // deactivate license from license service for current machine
//            _cryptoLicense.Deactivate();
//            _cryptoLicense.Remove();
//            // re-initialize license object
//            IntitializeLicense();
//        }

//        #endregion Public methods

//        #endregion Implementation of ILicensingService

//        #region Private methods

//        private void IntitializeLicense()
//        {
//            // Load the license from the registry
//            if (!_cryptoLicense.Load())
//            {
//                // When app runs for first time, the load will fail, so specify an evaluation code....
//                // This license code was generated from the Generator UI with a "Limit Usage Days To" setting of 30 days.
//                _cryptoLicense.LicenseCode = TrialLicenseCode;

//                // Save it so that it will get loaded the next time app runs
//                _cryptoLicense.Save();
//            }
//        }
//        private static int GetLicenseKeyVersionNo(string licenseUserData)
//        {
//            var licVerNoStr = GetLicenseUserData(licenseUserData, LicenseVersionNoUserDataIndex);
//            if (string.IsNullOrEmpty(licVerNoStr) == false)
//            {
//                int licVerNo;
//                if (int.TryParse(licVerNoStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out licVerNo))
//                    return licVerNo;
//            }
//            return -1;
//        }
//        private static string GetRegistrationLicenseStatusMessage(LicenseStatus status)
//        {
//            string message = null;
//            switch (status)
//            {
//                case LicenseStatus.Valid:
//                    message = Resources.LicenseStatus_Valid;
//                    break;
//                case LicenseStatus.NotValidated:
//                    message = Resources.LicenseStatus_NotValidated;
//                    break;
//                case LicenseStatus.SerialCodeInvalid:
//                    message = Resources.LicenseStatus_SerialCodeInvalid;
//                    break;
//                case LicenseStatus.SignatureInvalid:
//                    message = Resources.LicenseStatus_SignatureInvalid;
//                    break;
//                case LicenseStatus.MachineCodeInvalid:
//                    message = Resources.LicenseStatus_MachineCodeInvalid;
//                    break;
//                case LicenseStatus.Expired:
//                    message = Resources.LicenseStatus_Expired;
//                    break;
//                case LicenseStatus.UsageModeInvalid:
//                    message = Resources.LicenseStatus_UsageModeInvalid;
//                    break;
//                case LicenseStatus.ActivationFailed:
//                    message = Resources.LicenseStatus_ActivationFailed;
//                    break;
//                case LicenseStatus.UsageDaysExceeded:
//                    message = Resources.LicenseStatus_UsageDaysExceeded;
//                    break;
//                case LicenseStatus.UniqueUsageDaysExceeded:
//                    message = Resources.LicenseStatus_UniqueUsageDaysExceeded;
//                    break;
//                case LicenseStatus.ExecutionsExceeded:
//                    message = Resources.LicenseStatus_ExecutionsExceeded;
//                    break;
//                case LicenseStatus.EvaluationlTampered:
//                    message = Resources.LicenseStatus_EvaluationlTampered;
//                    break;
//                case LicenseStatus.GenericFailure:
//                    message = Resources.LicenseStatus_GenericFailure;
//                    break;
//                case LicenseStatus.InstancesExceeded:
//                    message = Resources.LicenseStatus_InstancesExceeded;
//                    break;
//                case LicenseStatus.RunTimeExceeded:
//                    message = Resources.LicenseStatus_RunTimeExceeded;
//                    break;
//                case LicenseStatus.CumulativeRunTimeExceeded:
//                    message = Resources.LicenseStatus_CumulativeRunTimeExceeded;
//                    break;
//                case LicenseStatus.EvaluationExpired:
//                    message = Resources.LicenseStatus_EvaluationExpired;
//                    break;
//                case LicenseStatus.ServiceNotificationFailed:
//                    message = Resources.LicenseStatus_ServiceNotificationFailed;
//                    break;
//                case LicenseStatus.HostAssemblyDifferent:
//                    message = Resources.LicenseStatus_HostAssemblyDifferent;
//                    break;
//                case LicenseStatus.StrongNameVerificationFailed:
//                    message = Resources.LicenseStatus_StrongNameVerificationFailed;
//                    break;
//                case LicenseStatus.Deactivated:
//                    message = Resources.LicenseStatus_Deactivated;
//                    break;
//                case LicenseStatus.DebuggerDetected:
//                    message = Resources.LicenseStatus_DebuggerDetected;
//                    break;
//                case LicenseStatus.DomainInvalid:
//                    message = Resources.LicenseStatus_DomainInvalid;
//                    break;
//                case LicenseStatus.DateRollbackDetected:
//                    message = Resources.LicenseStatus_DateRollbackDetected;
//                    break;
//                case LicenseStatus.LocalTimeInvalid:
//                    message = Resources.LicenseStatus_LocalTimeInvalid;
//                    break;
//                case LicenseStatus.CryptoLicensingModuleTampered:
//                    message = Resources.LicenseStatus_CryptoLicensingModuleTampered;
//                    break;
//                case LicenseStatus.RemoteSessionDetected:
//                    message = Resources.LicenseStatus_RemoteSessionDetected;
//                    break;
//                case LicenseStatus.InValid:
//                    message = Resources.LicenseStatus_InValid;
//                    break;
//            }
//            return message;
//        }
        
//        #endregion Private methods

//        #region Public methods

//        public static string GetLicenseUserData(string licenseUserData, int userDataIndex)
//        {
//            var userData = licenseUserData.Split(UserDataSeparator);
//            if (userData != null && userData.Length > userDataIndex)
//            {
//                return userData[userDataIndex];
//            }

//            return string.Empty;
//        }

//        #endregion Public methods

//        #region Implementation of IDisposable

//        internal bool isDisposed;

//        /// <summary>
//        /// Dispose current object.
//        /// </summary>
//        public void Dispose()
//        {
//            Dispose(true);
//            GC.SuppressFinalize(this);
//        }

//        /// <summary>
//        /// Dispose current object.
//        /// </summary>
//        /// <param name="disposing"></param>
//        protected virtual void Dispose(bool disposing)
//        {
//            if (!isDisposed)
//            {
//                isDisposed = true;
//                if (disposing)
//                {
//                    _cryptoLicense.Dispose();
//                    _cryptoLicense = null;
//                }
//            }
//        }

//        /// <summary>
//        /// Class destructor
//        /// </summary>
//        ~LicensingService()
//        {
//            Dispose(false);
//        }
//        #endregion Implementation of IDisposable

//    }
//}
