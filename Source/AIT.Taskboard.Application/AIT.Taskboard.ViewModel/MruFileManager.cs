using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace AIT.Taskboard.ViewModel
{
    class MruFileManager
    {
        private const int Capacity = 6;

        public MruFileManager()
        {
            LoadMruFileList();
        }
        ~MruFileManager ()
        {
            try
            {
                SaveMruFileList();
            }
            catch
            {
                // catch any exception. Under certain scenarios a null reference exception might be thrown during shutdown in the following call stack
                /*
                    at System.Configuration.ApplicationSettingsBase.IsClickOnceDeployed(AppDomain appDomain)
                    at System.Configuration.LocalFileSettingsProvider.IsRoamingSetting(SettingsProperty setting)
                    at System.Configuration.LocalFileSettingsProvider.SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection values)
                    at System.Configuration.SettingsBase.SaveCore()
                    at System.Configuration.SettingsBase.Save()
                    at System.Configuration.ApplicationSettingsBase.Save()
                    at AIT.Taskboard.ViewModel.MruFileManager.SaveMruFileList()
                    at AIT.Taskboard.ViewModel.MruFileManager.Finalize()
                 */
            }
        }

        private void LoadMruFileList()
        {
            MostRecentlyUsedFiles = Properties.Settings.Default.MruList ?? new StringCollection();
        }
        public StringCollection MostRecentlyUsedFiles { get; private set; }
        public void AddFile (string fileName)
        {
            if (MostRecentlyUsedFiles.Count > Capacity)
            {
                // Remove the last entry from the list
                MostRecentlyUsedFiles.RemoveAt(MostRecentlyUsedFiles.Count-1);
                // The next call ensures that files are removed until the capacity fits again.
                AddFile(fileName);
            }
            // Check if the file is already in the list
            if (MostRecentlyUsedFiles.Contains(fileName))
            {
                // In order to move the item to the first position we just remove it from the list. The next step afterwards will add it at the first position.
                MostRecentlyUsedFiles.Remove(fileName);
            }
            MostRecentlyUsedFiles.Insert(0, fileName);

            SaveMruFileList();
            
        }
        private void SaveMruFileList ()
        {
            if (MostRecentlyUsedFiles.Count > 0)
            {
                Properties.Settings.Default.MruList = MostRecentlyUsedFiles;
                Properties.Settings.Default.Save();
            }
        }
    }
}
