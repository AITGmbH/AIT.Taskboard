namespace AIT.Taskboard.Interface
{
    public interface IConfigurationService
    {
        IConfiguration ReadTaskboardConfiguration(string configurationData);
        void SaveTaskboardConfiguration(string configurationData, IConfiguration config);
        IConfiguration GetDefaultConfiguration();
        
        /// <summary>
        /// Gets or sets whether to use memory file instead original config file.
        /// </summary>
        bool UseMemoryFile { get; set; }
    }
}