using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using AIT.Taskboard.Application.Helper;
using AIT.Taskboard.Interface.MEF;
using AIT.Taskboard.Interface;

namespace AIT.Taskboard.Application
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : IMefApplication
    {
        [Import("MainWindow", typeof(Window))]
        public Window StartWindow
        {
            get { return MainWindow; }
            set { MainWindow = value; }
        }

        [Import]
        public ITaskboardService TaskboardService { get; set; }

        [Import]
        private ILogger Logger { get; set; }


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var bootStrapper = new Bootstrapper(this,
               new BootstrapperEventArgs(Assembly.GetExecutingAssembly()));
            
            this.DispatcherUnhandledException += HandleException;
            AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;
        }

        private void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // TODO: Check correct exception handling
            try
            {
                Logger.LogException(e.ExceptionObject as Exception);
                TaskboardService.HandleConnectionException(e.ExceptionObject as Exception);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void HandleException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                Logger.LogException(e.Exception);
                if (TaskboardService.HandleConnectionException(e.Exception))
                {
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public void OnAddCatalog(ICollection<ComposablePartCatalog> catalogs)
        {
        }

        private void HandleApplicationStartup(object sender, StartupEventArgs e)
        {
            // Ensure to run required preparations
            ApplicationHelper.PrepareHelp("Taskboard.Quickstart.pdf");
            // Ensure to get all command line parameters
            var args = Environment.GetCommandLineArgs();
            
            // try to find attached file in command line arguments
            string associateFile = args.Length >= 2 ? args[1] : null;
            if (associateFile == null 
                && AppDomain.CurrentDomain.SetupInformation.ActivationArguments != null
                && AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData != null
                && AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData.Length > 0)
            {
                // try to find associate file from clickonce association
                var suri = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData[0];
                var uri = new Uri(suri);
                associateFile = uri.LocalPath;
            }
            ApplicationHelper.FileToOpen = associateFile;
        }
    }
}