using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AIT.Taskboard.Application
{
    public static class TaskboardCommands
    {

        public static RibbonCommand ConnectToTfs
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["ConnectToTfsCommand"]; }
        }
        public static RibbonCommand OpenConfiguration
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["OpenConfigurationCommand"]; }
        }
        public static RibbonCommand NewConfiguration
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["NewConfigurationCommand"]; }
        }
        public static RibbonCommand SaveConfiguration
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["SaveConfigurationCommand"]; }

        }
        public static RibbonCommand ApplicationMenu
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["ApplicationMenuCommand"]; }
        }
        public static RibbonCommand TeamProject
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["TeamProjectCommand"]; }
        }
        public static RibbonCommand TeamProjectCollection
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["TeamProjectCollectionCommand"]; }
        }
        public static RibbonCommand TfsServer
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["TfsServerCommand"]; }
        }
        public static RibbonCommand Publish
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["PublishCommand"]; }
        }
        public static RibbonCommand Settings
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["SettingsCommand"]; }
        }
        public static RibbonCommand Refresh
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["RefreshCommand"]; }
        }
        public static RibbonCommand Help
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["HelpCommand"]; }
        }
        public static RibbonCommand About
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["AboutCommand"]; }
        }
        public static RibbonCommand ExitApplication
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["ExitApplicationCommand"]; }
        }
        public static RibbonCommand Options
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["OptionsCommand"]; }
        }
        public static RibbonCommand SelectWorkItemResource
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["SelectWorkItemResourceCommand"]; }
        }
        public static RibbonCommand ResetWorkItemResource
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["ResetWorkItemResourceCommand"]; }
        }
        public static RibbonCommand SendFeedback
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["SendFeedbackCommand"]; }
        }
        public static RibbonCommand SendFeedbackEmail
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["SendFeedbackEmailCommand"]; }
        }
        public static RibbonCommand Print
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["PrintCommand"]; }
        }
        public static RibbonCommand PrintCurrentItem
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["PrintCurrentItemCommand"]; }
        }
        public static RibbonCommand PrintBacklogItemAndChildren
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["PrintBacklogItemAndChildrenCommand"]; }
        }
        public static RibbonCommand PrintAllItems
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["PrintAllItemsCommand"]; }
        }

        public static RoutedUICommand SelectBacklogItem
        {
            get { return (RoutedUICommand)System.Windows.Application.Current.Resources["SelectBacklogItemCommand"]; }
        }
        public static RoutedUICommand SelectChildItem
        {
            get { return (RoutedUICommand)System.Windows.Application.Current.Resources["SelectChildItemCommand"]; }
        }
        public static RoutedUICommand AddCustomState
        {
            get { return (RoutedUICommand)System.Windows.Application.Current.Resources["AddCustomStateCommand"]; }
        }
        public static RoutedUICommand RemoveCustomState
        {
            get { return (RoutedUICommand)System.Windows.Application.Current.Resources["RemoveCustomStateCommand"]; }
        }
        public static RoutedUICommand MoveStateUp
        {
            get { return (RoutedUICommand)System.Windows.Application.Current.Resources["MoveStateUpCommand"]; }
        }
        public static RoutedUICommand MoveStateDown
        {
            get { return (RoutedUICommand)System.Windows.Application.Current.Resources["MoveStateDownCommand"]; }
        }
        public static RoutedUICommand SelectWorkItemState
        {
            get { return (RoutedUICommand)System.Windows.Application.Current.Resources["SelectWorkItemStateCommand"]; }
        }
        public static RoutedUICommand CommitConfigurationDialog
        {
            get { return (RoutedUICommand)System.Windows.Application.Current.Resources["CommitConfigurationDialogCommand"]; }
        }
        public static RoutedUICommand CancelConfigurationDialog
        {
            get { return (RoutedUICommand)System.Windows.Application.Current.Resources["CancelConfigurationDialogCommand"]; }
        }
        public static RoutedUICommand BrowseColor
        {
            get { return (RoutedUICommand)System.Windows.Application.Current.Resources["BrowseColorCommand"]; }
        }
        public static RoutedUICommand CancelIssueDialog
        {
            get { return (RoutedUICommand)System.Windows.Application.Current.Resources["CancelIssueDialogCommand"]; }
        }
        public static RoutedUICommand PublishIssueWorkItems
        {
            get { return (RoutedUICommand)System.Windows.Application.Current.Resources["PublishIssueWorkItemsCommand"]; }
        }
        public static RoutedUICommand EditWorkItem
        {
            get { return (RoutedUICommand)System.Windows.Application.Current.Resources["EditWorkItemCommand"]; }
        }
        public static RibbonCommand AddNewLinkedWorkItem
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["AddNewLinkedWorkItemCommand"]; }
        }
        /// <summary>
        /// Command to Auto refresh ToggleBox
        /// </summary>
        public static RibbonCommand AutoRefresh
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["AutoRefreshCommand"]; }
        }

        #region View tab
        public static RibbonCommand FullScreen
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["FullScreenCommand"]; }
        }
        public static RibbonCommand ZoomIn
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["ZoomInCommand"]; }
        }
        public static RibbonCommand ZoomOut
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["ZoomOutCommand"]; }
        }
        public static RibbonCommand ShowHideEditBar
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["ShowHideEditBarCommand"]; }
        }
        public static RibbonCommand ShowHideReportViewer
        {
            get { return (RibbonCommand)System.Windows.Application.Current.Resources["ShowHideReportViewer"]; }
        }
        #endregion

        #region Query select window

        public static RoutedUICommand CommitQueryDialog
        {
            get { return (RoutedUICommand)System.Windows.Application.Current.Resources["CommitQueryDialogCommand"]; }
        }
        public static RoutedUICommand CancelQueryDialog
        {
            get { return (RoutedUICommand)System.Windows.Application.Current.Resources["CancelQueryDialogCommand"]; }
        }
        

        #endregion

        #region Work Item Window

        public static RoutedUICommand SaveAndClose
        {
            get { return (RoutedUICommand)System.Windows.Application.Current.Resources["SaveAndCloseCommand"]; }
        }

        #endregion

    }

    public class RibbonCommand : RoutedCommand, INotifyPropertyChanged
    {
        // Fields
        private CanExecuteRoutedEventHandler _canExecute;
        private CommandBinding _commandBinding;
        private ExecutedRoutedEventHandler _executed;
        private string _labelDescription;
        private string _labelTitle;
        private ImageSource _largeImageSource;
        private CanExecuteRoutedEventHandler _previewCanExecute;
        private ExecutedRoutedEventHandler _previewExecuted;
        private ImageSource _smallImageSource;
        private string _toolTipDescription;
        private string _toolTipFooterDescription;
        private ImageSource _toolTipFooterImageSource;
        private string _toolTipFooterTitle;
        private ImageSource _toolTipImageSource;
        private string _toolTipTitle;

        // Events
        new public event CanExecuteRoutedEventHandler CanExecute
        {
            add
            {
                if (this._commandBinding == null)
                {
                    this._commandBinding = new CommandBinding(this, this._executed, value);
                    CommandManager.RegisterClassCommandBinding(typeof(Window), this._commandBinding);
                }
                else
                {
                    this._commandBinding.CanExecute -= value;
                    this._commandBinding.CanExecute += value;
                }
                this._canExecute = value;
            }
            remove
            {
                if (this._commandBinding != null)
                {
                    this._commandBinding.CanExecute -= value;
                }
                this._canExecute = null;
            }
        }

        public event ExecutedRoutedEventHandler Executed
        {
            add
            {
                if (this._commandBinding == null)
                {
                    this._commandBinding = new CommandBinding(this, value, this._canExecute);
                    CommandManager.RegisterClassCommandBinding(typeof(Window), this._commandBinding);
                }
                else
                {
                    this._commandBinding.Executed -= value;
                    this._commandBinding.Executed += value;
                }
                this._executed = value;
            }
            remove
            {
                if (this._commandBinding != null)
                {
                    this._commandBinding.Executed -= value;
                }
                this._executed = null;
            }
        }

        public event CanExecuteRoutedEventHandler PreviewCanExecute
        {
            add
            {
                if (this._commandBinding == null)
                {
                    this._commandBinding = new CommandBinding(this, this._executed, this._canExecute);
                    CommandManager.RegisterClassCommandBinding(typeof(Window), this._commandBinding);
                }
                this._commandBinding.PreviewCanExecute -= value;
                this._commandBinding.PreviewCanExecute += value;
                this._previewCanExecute = value;
            }
            remove
            {
                if (this._commandBinding != null)
                {
                    this._commandBinding.PreviewCanExecute -= value;
                }
                this._previewCanExecute = null;
            }
        }

        public event ExecutedRoutedEventHandler PreviewExecuted
        {
            add
            {
                if (this._commandBinding == null)
                {
                    this._commandBinding = new CommandBinding(this, this._executed, this._canExecute);
                    CommandManager.RegisterClassCommandBinding(typeof(Window), this._commandBinding);
                }
                this._commandBinding.PreviewExecuted -= value;
                this._commandBinding.PreviewExecuted += value;
                this._previewExecuted = value;
            }
            remove
            {
                if (this._commandBinding != null)
                {
                    this._commandBinding.PreviewExecuted -= value;
                }
                this._previewExecuted = null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Methods
        public RibbonCommand()
        {
        }

        public RibbonCommand(string name, Type ownerType)
            : base(name, ownerType, (InputGestureCollection)null)
        {
        }

        public RibbonCommand(string name, Type ownerType, InputGestureCollection inputGestures)
            : base(name, ownerType, inputGestures)
        {
        }

        private void NotifyPropertyChanged(string info)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        // Properties
        public string LabelDescription
        {
            get
            {
                return this._labelDescription;
            }
            set
            {
                if (value != this._labelDescription)
                {
                    this._labelDescription = value;
                    this.NotifyPropertyChanged("LabelDescription");
                }
            }
        }

        public string LabelTitle
        {
            get
            {
                return this._labelTitle;
            }
            set
            {
                if (value != this._labelTitle)
                {
                    this._labelTitle = value;
                    this.NotifyPropertyChanged("LabelTitle");
                }
            }
        }

        public ImageSource LargeImageSource
        {
            get
            {
                return this._largeImageSource;
            }
            set
            {
                if (value != this._largeImageSource)
                {
                    this._largeImageSource = value;
                    this.NotifyPropertyChanged("LargeImageSource");
                }
            }
        }

        public ImageSource SmallImageSource
        {
            get
            {
                return this._smallImageSource;
            }
            set
            {
                if (value != this._smallImageSource)
                {
                    this._smallImageSource = value;
                    this.NotifyPropertyChanged("SmallImageSource");
                }
            }
        }

        public string ToolTipDescription
        {
            get
            {
                return this._toolTipDescription;
            }
            set
            {
                if (value != this._toolTipDescription)
                {
                    this._toolTipDescription = value;
                    this.NotifyPropertyChanged("ToolTipDescription");
                }
            }
        }

        public string ToolTipFooterDescription
        {
            get
            {
                return this._toolTipFooterDescription;
            }
            set
            {
                if (value != this._toolTipFooterDescription)
                {
                    this._toolTipFooterDescription = value;
                    this.NotifyPropertyChanged("ToolTipFooterDescription");
                }
            }
        }

        public ImageSource ToolTipFooterImageSource
        {
            get
            {
                return this._toolTipFooterImageSource;
            }
            set
            {
                if (value != this._toolTipFooterImageSource)
                {
                    this._toolTipFooterImageSource = value;
                    this.NotifyPropertyChanged("ToolTipFooterImageSource");
                }
            }
        }

        public string ToolTipFooterTitle
        {
            get
            {
                return this._toolTipFooterTitle;
            }
            set
            {
                if (value != this._toolTipFooterTitle)
                {
                    this._toolTipFooterTitle = value;
                    this.NotifyPropertyChanged("ToolTipFooterTitle");
                }
            }
        }

        public ImageSource ToolTipImageSource
        {
            get
            {
                return this._toolTipImageSource;
            }
            set
            {
                if (value != this._toolTipImageSource)
                {
                    this._toolTipImageSource = value;
                    this.NotifyPropertyChanged("ToolTipImageSource");
                }
            }
        }

        public string ToolTipTitle
        {
            get
            {
                return this._toolTipTitle;
            }
            set
            {
                if (value != this._toolTipTitle)
                {
                    this._toolTipTitle = value;
                    this.NotifyPropertyChanged("ToolTipTitle");
                }
            }
        }
    }
}