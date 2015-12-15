using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using AIT.Taskboard.Interface;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace AIT.Taskboard.Model
{
    [Export(typeof(IWorkItemTemplateProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class WorkItemTemplateProvider : IWorkItemTemplateProvider
    {
        #region Constants

        private const string TemplateWidth = "TemplateWidth";
        private const string TemplateHeight = "TemplateHeight";

        #endregion Constants

        #region Fields

        private readonly Dictionary<string, ResourceDictionary> _localResources = new Dictionary<string, ResourceDictionary>();
        private readonly Dictionary<string, DataTemplate> _workItemTemplates = new Dictionary<string, DataTemplate>();

        #endregion Fields

        #region Public properties

        public Size GetWorkItemSize(WorkItemSize size)
        {
            EnsureCustomResourceDictionary();

            var templateWidthFullPropertyName = GetFullPropertyName(TemplateWidth, size);
            var templateHeightFullPropertyName = GetFullPropertyName(TemplateHeight, size);
            // The template is not cached. retrieve it from the current dictionary
            var templateFile = WorkItemTemplatesFile;
            if (_localResources.ContainsKey(templateFile))
            {
                var templateWidth = _localResources[templateFile][templateWidthFullPropertyName];
                var templateHeight = _localResources[templateFile][templateHeightFullPropertyName];
                if (templateWidth != null && templateHeight != null)
                    return new Size((double)templateWidth, (double)templateHeight);
            }

            var width = Application.Current.MainWindow.TryFindResource(templateWidthFullPropertyName);
            var height = Application.Current.MainWindow.TryFindResource(templateHeightFullPropertyName);

            if(width != null && height != null)
                return new Size((double)width, (double)height);

            return Size.Empty;
        }

        private string GetFullPropertyName(string basePropertyName, WorkItemSize size)
        {
            if (size != WorkItemSize.Medium)
                basePropertyName = size.ToString() + basePropertyName;
            return basePropertyName;
        }

        public string WorkItemTemplatesFile
        {
            get { return Properties.Settings.Default.WorkItemTemplates; }
            set
            {
                if (value != Properties.Settings.Default.WorkItemTemplates)
                    _workItemTemplates.Clear();
                Properties.Settings.Default.WorkItemTemplates = value;
                Properties.Settings.Default.Save();
            }
        }

        #endregion Public properties

        #region Public methods

        public DataTemplate GetWorkItemTemplate(WorkItemType workItemType, WorkItemSize size)
        {
            // TODO: Query template cache first
            // TODO: Cache Dictionary
            // TODO: Cache found templates
            // TODO: Clean templates cache on dictionary change

            // Ensure that the resource dictionary is correctly set up.
            EnsureCustomResourceDictionary();

            // Determine the key of the data template to use for the work item
            var keyName = String.Format("{0}WorkItemTemplate", workItemType.Name).Replace(" ", "");

            DataTemplate template = null;
            var fullKeyName = GetFullPropertyName(keyName, size);

            template = FindDataTemplate(fullKeyName);
            if (template != null)
                return template;
            return (DataTemplate)Application.Current.MainWindow.TryFindResource(fullKeyName);
        }

        #endregion Public methods

        #region Private methods

        private void EnsureCustomResourceDictionary()
        {
            // Check if we already loaded the customer dictionary
            string templateFile = WorkItemTemplatesFile;
            // If we have no custom template file we do not have anything to load
            if (string.IsNullOrEmpty(templateFile)) return;

            if (!_localResources.ContainsKey(templateFile))
            {
                //try loading the resource, remember it and add it to the container
                if (File.Exists(templateFile))
                {
                    using (FileStream fs = new FileStream(templateFile, FileMode.Open, FileAccess.Read))
                    {
                        try
                        {
                            // Read the resource dictionary
                            ResourceDictionary dic = (ResourceDictionary)XamlReader.Load(fs);
                            // Drop all previously cahced dictionaries
                            _localResources.Clear();
                            // Remember the current dictionary
                            _localResources.Add(templateFile, dic);
                            // Clear the work item template cache so that templates are retrieved from the new resource dictionary
                            _workItemTemplates.Clear();
                        }
                        catch (XamlParseException xamlEx)
                        {
                            // TODO: Take the exceptipon and publish it to someone who can handle it.
                        }
                    }
                }
            }
        }

        private DataTemplate FindDataTemplate(string keyName)
        {
            // Check if we already know the template. If this is the case just return it.
            if (_workItemTemplates.ContainsKey(keyName))
                return _workItemTemplates[keyName];

            // The template is not cached. retrieve it from the current dictionary
            var templateFile = WorkItemTemplatesFile;
            if (_localResources.ContainsKey(templateFile))
            {
                var dictionary = _localResources[templateFile];
                var templateObject = dictionary[keyName];
                var template = (DataTemplate)templateObject;
                _workItemTemplates[keyName] = template;
                return template;
            }
            return null;
        }

        #endregion Private methods

        public bool IsSizeDefined(WorkItemSize workItemSize)
        {
            EnsureCustomResourceDictionary();

            // The template is not cached. retrieve it from the current dictionary
            if (_localResources.ContainsKey(WorkItemTemplatesFile))
            {
                var templateWidthFullPropertyName = GetFullPropertyName(TemplateWidth, workItemSize);
                var templateHeightFullPropertyName = GetFullPropertyName(TemplateHeight, workItemSize);
                return _localResources[WorkItemTemplatesFile].Contains(templateWidthFullPropertyName) && _localResources[WorkItemTemplatesFile].Contains(templateHeightFullPropertyName);
            }
            else
            {
                object width = Application.Current.MainWindow.TryFindResource(GetFullPropertyName(TemplateWidth, workItemSize));
                object height = Application.Current.MainWindow.TryFindResource(GetFullPropertyName(TemplateHeight, workItemSize));
                return width != null && height != null;
            }
        }
    }
}
