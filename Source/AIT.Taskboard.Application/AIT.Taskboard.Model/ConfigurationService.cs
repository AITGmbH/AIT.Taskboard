using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Media;
using System.Xml;
using AIT.Taskboard.Interface;
using AIT.Taskboard.Model.Properties;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.ComponentModel;

namespace AIT.Taskboard.Model
{
    [Export(typeof(IConfigurationService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ConfigurationService : IConfigurationService, INotifyPropertyChanged
    {
        private static Dictionary <string, Color> KnownColors = new Dictionary<string, Color>();

        

        private static int lastColorIndex = 0;
        private MemoryStream _memoryFile;
        private bool _useMemoryFile;

        /// <summary>
        /// Gets or sets whether to use memory file instead original config file.
        /// </summary>
        public bool UseMemoryFile 
        {
            get { return _useMemoryFile;  }
            set
            {
                _useMemoryFile = value;
                if (value == false)
                {
                    MemoryFile = null;
                }
                OnPropertyChanged("UseMemoryFile");
            }
        }

        /// <summary>
        /// Gets or sets memory stream which presents temporary memory file.
        /// </summary>
        private MemoryStream MemoryFile 
        {
            get
            {
                if (_memoryFile == null)
                    _memoryFile = new MemoryStream();
                _memoryFile.Seek(0, SeekOrigin.Begin);
                return _memoryFile;
            }
            set
            {
                if (value == null && _memoryFile != null)
                {
                    _memoryFile.Close();
                }
                _memoryFile = value;
            }
        }

        public IConfiguration ReadTaskboardConfiguration(string configurationData)
        {
            if (string.IsNullOrEmpty(configurationData) && !UseMemoryFile)
                throw new ArgumentException(
                    "Configuration data is required when not using memory file. It must be a file path.",
                    "configurationData");
            if (UseMemoryFile)
                return ReadFromMemoryFile();

            if (!string.IsNullOrEmpty(configurationData) && !File.Exists(configurationData))
            {
                // If the file does not exist we create a new file and also a new default configuration and we ensure to persist the default configuration to that file
                // Ensure that a file is created for the configuration
                IConfiguration config = GetDefaultConfiguration();
                SaveToXml(config, configurationData);
                return config;
            }

            try
            {
                using (StreamReader s = new StreamReader(configurationData))
                {
                    try
                    {
                        return ReadTaskboardConfigurationV5(s, configurationData);
                    }
                    catch (Exception e)
                    {
                        return LoadWhenException(e, configurationData);
                    }
                }
            }
            catch (Exception e)
            {
                return LoadWhenException(e, configurationData);
            }
        }

        private IConfiguration ReadTaskboardConfigurationV5(StreamReader s, string configurationData)
        {
            Logger.Write(TraceCategory.Information, "V5 .tbconfig version is being loaded");
            try
            {
                var serializer = new DataContractSerializer(typeof(ConfigurationV5), new[] { typeof(CustomState), typeof(WorkItemSize) });
                return (IConfiguration)serializer.ReadObject(s.BaseStream);
            }
            // this happens if old version of .tbconfig is being loaded
            catch (SerializationException)
            {
                Logger.Write(TraceCategory.Information, "V5 .tbconfig version could not be loaded");
                s.BaseStream.Seek(0, new SeekOrigin());
                var cfgV4 = ReadTaskboardConfigurationV4(s, configurationData);
                var cfg = new ConfigurationV5();
                cfg.CopyFromConfigurationOld4(cfgV4);
                return cfg;
            }
        }

        private ConfigurationV4 ReadTaskboardConfigurationV4(StreamReader s, string configurationData)
        {
            Logger.Write(TraceCategory.Information, "V4 .tbconfig version is being loaded");
            try
            {
                var serializer = new DataContractSerializer(typeof(ConfigurationV4), new[] { typeof(CustomState), typeof(WorkItemSize) });
                return (ConfigurationV4)serializer.ReadObject(s.BaseStream);
            }
            // this happens if old version of .tbconfig is being loaded
            catch (SerializationException)
            {
                Logger.Write(TraceCategory.Information, "V4 .tbconfig version could not be loaded");
                s.BaseStream.Seek(0, new SeekOrigin());
                ConfigurationV3 cfgV3 = ReadTaskboardConfigurationV3(s, configurationData);
                ConfigurationV4 cfg = new ConfigurationV4();
                cfg.CopyFromConfigurationOld3(cfgV3);
                return cfg;
            }
        }

        private ConfigurationV3 ReadTaskboardConfigurationV3(StreamReader s, string configurationData)
        {
            Logger.Write(TraceCategory.Information, "V3 .tbconfig version is being loaded");
            try
            {
                var serializer = new DataContractSerializer(typeof(ConfigurationV3), new[] { typeof(CustomState) });
                return (ConfigurationV3)serializer.ReadObject(s.BaseStream);
            }
            // this happens if old version of .tbconfig is being loaded
            catch (SerializationException)
            {
                Logger.Write(TraceCategory.Information, "V3 .tbconfig version could not be loaded");
                s.BaseStream.Seek(0, new SeekOrigin());
                ConfigurationV2 cfgV2 = ReadTaskboardConfigurationV2(s, configurationData);
                ConfigurationV3 cfg = new ConfigurationV3();
                cfg.CopyFromConfigurationOld2(cfgV2);
                return cfg;
            }
        }

        private ConfigurationV2 ReadTaskboardConfigurationV2(StreamReader s, string configurationData)
        {
            Logger.Write(TraceCategory.Information, "V2 .tbconfig version is being loaded");
            try
            {
                var serializer = new DataContractSerializer(typeof(ConfigurationV2), new[] { typeof(CustomState) });
                return (ConfigurationV2)serializer.ReadObject(s.BaseStream);
                
            }
            // this happens if old version of .tbconfig is being loaded
            catch (SerializationException)
            {
                Logger.Write(TraceCategory.Information, "V2 .tbconfig version could not be loaded");
                s.BaseStream.Seek(0, new SeekOrigin());
                Configuration configOld1 = ReadTaskboardConfigurationV1(s, configurationData);
                ConfigurationV2 cfg = new ConfigurationV2();
                cfg.CopyFromConfigurationOld1(configOld1);
                return cfg;
            }
        }

        private Configuration ReadTaskboardConfigurationV1(StreamReader s, string configurationData)
        {
            Logger.Write(TraceCategory.Information, "V1 .tbconfig version is being loaded");

            var serializer = new DataContractSerializer(typeof(Configuration), new[] { typeof(CustomState) });
            Configuration configOld1 = new Configuration();
            // move cursor to beginning of file
            return (Configuration)serializer.ReadObject(s.BaseStream);
        }

        private IConfiguration LoadWhenException(Exception ex, string configurationData)
        {
            Logger.LogException(ex);
            IConfiguration config = GetDefaultConfiguration();
            SaveToXml(config, configurationData);
            return config;
        }

        private IConfiguration ReadFromMemoryFile()
        {
            IConfiguration config;
            using (var reader = XmlReader.Create(MemoryFile))
            {
                try
                {
                    var serializer = new DataContractSerializer(typeof(ConfigurationV5), new[] { typeof(CustomState), typeof(WorkItemSize) });
                    config = (IConfiguration)serializer.ReadObject(reader, false);
                }
                catch
                {
                    Logger.Write(TraceCategory.Warning, "Unable to deserialize memory file, creating default configuration");
                    reader.Close();
                    // We were not able to deserialize from stream. We create a default configuration and use that.
                    config = GetDefaultConfiguration();
                    SaveToXml(config, "");
                }
            }
            return config;
        }

        public IConfiguration GetDefaultConfiguration()
        {
            ConfigurationV5 config = new ConfigurationV5();
            if (TfsInformationProvider.TeamProject != null)
                config.TeamProject = TfsInformationProvider.TeamProject.Name;
            if (TfsInformationProvider.TeamProjectCollection != null)
                config.TeamProjectCollection = TfsInformationProvider.TeamProjectCollection.Uri.ToString();
            FillDefaultBacklogItems(config);
            FillDefaultChildItems(config);
            FillDefaultCustomStates(config);
            
            return config;
        }

        private void FillDefaultCustomStates(ConfigurationV2 config)
        {
            var allWorkItemTypes = TaskboardService.GetWorkItemTypes();
            if (allWorkItemTypes != null)
            {
                List<WorkItemType> workItemTypes = new List<WorkItemType>();
                if ((config.ChildItems !=  null) && (config.ChildItems.Count > 0))
                {
                    // Only consider the work item types that have been configured as child items
                    workItemTypes.AddRange(allWorkItemTypes.Cast<WorkItemType>().Where(workItemType => config.ChildItems.Contains(workItemType.Name)));
                }
                else
                {
                    // Consider all work item types
                    workItemTypes.AddRange(allWorkItemTypes.Cast<WorkItemType>());
                }
                var states = new List<string>();
                foreach (WorkItemType workItemType in workItemTypes)
                {
                    List<string> witStates = GetStatesForWorkItemType(workItemType);
                    foreach (string allowedValue in witStates)
                    {
                        if (!states.Contains(allowedValue))
                            states.Add(allowedValue);
                    }
                }
                foreach (string state in states)
                {
                    CreateDefaultCustomStates(states, config, state, state, GetStateColor(state));
                }
            }
        }

        private List<string> GetStatesForWorkItemType(WorkItemType workItemType)
        {
            List<string> states = new List<string>();

            // get Work Item type definition
            XmlDocument witd = workItemType.Export(true);
            // retrieve the transitions node
            XmlNode statesNode = witd.SelectSingleNode("descendant::STATES");
            // for each state definition (== possible next allowed state)
            if (statesNode != null)
            {
                var nodes = statesNode.SelectNodes("STATE");
                if (nodes != null)
                {
                    foreach (XmlNode stateNode in nodes)
                    {
                        string value = stateNode.Attributes.GetNamedItem("value").Value;
                        states.Add(value);
                    }
                }
            }
            return states;
        }

        private Color GetStateColor(string state)
        {
            // We need to get the colors for certain states. 
            // We will remember each determined color in order to return it again upon next request.
            // We will use the color values from the default templates

            if (KnownColors.ContainsKey(state))
                return KnownColors[state];

            var activeColor = Color.FromArgb(0xFF, 0x59, 0x89, 0xA3);
            var resolvedColor = Color.FromArgb(0xFF, 0xDB, 0xCB, 0x62);
            var closedColor = Color.FromArgb(0xFF, 0xBE, 0xE4, 0x6A);

            Color color;
            switch (state)
            {
                case "Active":
                    color = activeColor;
                    break;
                case "Resolved":
                    color = resolvedColor;
                    break;
                case "Closed":
                    color = closedColor;
                    break;
                default:
                    color = Constants.PresetColors[lastColorIndex++];
                    lastColorIndex = lastColorIndex % Constants.PresetColors.Length;
                    break;
            }
            // Remember the color:
            KnownColors[state] = color;
            return color;
        }

        private static void CreateDefaultCustomStates(List<string> states, ConfigurationV2 configuration, string customeStateName, string expectedWorkItemState, Color customeStateColor)
        {
            if (states.Contains(expectedWorkItemState))
            {
                var newState = new CustomState() {Color = customeStateColor, Name = customeStateName};
                newState.WorkItemStates.Add(expectedWorkItemState);
                configuration.States.Add(newState);
                
            }
        }

        private void FillDefaultChildItems(ConfigurationV2 config)
        {
            var workItemTypes = TaskboardService.GetWorkItemTypes();
            if (workItemTypes == null)
                return;
            foreach (WorkItemType workItemType in workItemTypes)
            {
                if ((workItemType.Name.ToUpper().Contains("TASK")) && (!config.BacklogItems.Contains(workItemType.Name)))
                {
                    config.ChildItems.Add(workItemType.Name);
                }
            }
        }

        private void FillDefaultBacklogItems(ConfigurationV2 config)
        {
            if (TfsInformationProvider.TeamProject == null)
                return;
            foreach (Category category in TfsInformationProvider.TeamProject.Categories)
            {
                if (( category.ReferenceName == "Microsoft.RequirementCategory") || (category.ReferenceName == "Microsoft.BugCategory"))
                {
                    // Pick the items from the category and make them backlog items
                    foreach (WorkItemType workItemType in category.WorkItemTypes)
                    {
                        config.BacklogItems.Add(workItemType.Name);
                    }
                }
            }
        }

        private void SaveToXml(IConfiguration configuration, string fileName)
        {
            try
            {
                if (UseMemoryFile)
                {
                    SaveToMemoryFile(configuration);
                }
                else
                {
                    using (StreamWriter s = new StreamWriter(fileName))
                    {
                        var serializer = new DataContractSerializer(configuration.GetType(), new[] { typeof(CustomState) });
                        serializer.WriteObject(s.BaseStream, configuration);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Logger.Write(TraceCategory.Warning, "File is not accessible, writing to memory file");
                // when is file readonly or not accessible then use memory file
                SaveToMemoryFile(configuration);
            }
            catch (IOException)
            {
                Logger.Write(TraceCategory.Warning, "File is not accessible, writing to memory file");
                SaveToMemoryFile(configuration);
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                if (StatusService != null)
                {
                    string message = string.Format(CultureInfo.InvariantCulture, Resources.CouldNotSaveConfigFile, fileName);
                    var item = new StatusItem() { Id = "CouldNotSaveConfigFile", IsAutoHide = false, Message = message };
                    StatusService.EnqueueStatusItem(item);
                }
            }
            
        }

        /// <summary>
        /// Use memory file when config file is not writeable or accessible
        /// </summary>
        private void SaveToMemoryFile(IConfiguration configuration)
        {
            try
            {
                // first close previuous memory file
                UseMemoryFile = false;

                // save to new memory file
                var memoryFile = new MemoryStream();
                using (var writer = XmlWriter.Create(memoryFile))
                {
                    var ser = new DataContractSerializer(configuration.GetType(), new[] { typeof(CustomState) });
                    ser.WriteObject(writer, configuration);
                }
                MemoryFile = memoryFile;
                UseMemoryFile = true;
            }
            catch(Exception exception)
            {
                Logger.Write(TraceCategory.Exception,"ConfigurationService: SaveToMemoryFile");
                Logger.LogException(exception);
            }
        }

        [Import]
        public ITaskboardService TaskboardService { get; set; }
        [Import]
        private ILogger Logger { get; set; }
        [Import]
        public ITfsInformationProvider TfsInformationProvider { get; set; }
        [Import]
        public IStatusService StatusService { get; set; }

        public void SaveTaskboardConfiguration(string configurationData, IConfiguration config)
        {
            if (!UseMemoryFile && !File.Exists(configurationData))
            {
                if (configurationData == null)
                    return; //cannot save if file location is not configured
                var stream = File.Create(configurationData);
                stream.Close();
            }
            SaveToXml(config, configurationData);
        }

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
