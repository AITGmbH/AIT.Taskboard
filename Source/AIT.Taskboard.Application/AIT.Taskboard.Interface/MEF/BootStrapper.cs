using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Reflection;
using System.Windows;

namespace AIT.Taskboard.Interface.MEF
{
    /// <summary></summary>
    public class Bootstrapper
    {
        private readonly Application _application;
        private readonly Assembly _assembly;
        private CompositionContainer _container;
        private ILogger _logger;

        /// <summary>
        ///   Initializes a new instance of the
        ///   <see cref="Bootstrapper" />
        ///   class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        ///   The
        ///   <see cref="BootstrapperEventArgs" />
        ///   instance containing the event data.
        /// </param>
        public Bootstrapper(object sender, BootstrapperEventArgs e)
        {
            _application = sender as Application;
            _assembly = e.Assembly;

            AppDomain.CurrentDomain.UnhandledException += HandleException;

            if (_application != null)
            {
                _application.Exit += ApplicationExitHandler;

                if (Compose())
                    _application.MainWindow.Show();
                else
                    _application.Shutdown();
            }
        }

        /// <summary>
        ///   Handles the Exit event of the application control.
        /// </summary>
        /// <param name="sender">
        ///   The source of the event.
        /// </param>
        /// <param name="e">
        ///   The
        ///   <see cref="System.Windows.ExitEventArgs" />
        ///   instance containing the event data.
        /// </param>
        private void ApplicationExitHandler(object sender, ExitEventArgs e)
        {
            if (_container != null)
                _container.Dispose();
        }

        /// <summary>
        ///   Composes this instance.
        /// </summary>
        /// <returns></returns>
        private bool Compose()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(_assembly));
            catalog.Catalogs.Add(new DirectoryCatalog(".", "AIT.*.dll"));

            var mefApplication = _application as IMefApplication;

            if (mefApplication != null)
            {
                mefApplication.OnAddCatalog(catalog.Catalogs);
            }

            _container = new CompositionContainer(catalog);

            try
            {
                _container.ComposeExportedValue(_container);
                _container.ComposeParts(_application);
            }
            catch (CompositionException compositionException)
            {
                Logger.LogException(compositionException);
                return false;
            }
            return true;
        }

        private void HandleException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex == null) return;
            Logger.LogException(ex);
        }

        protected ILogger Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = _container.GetExportedValue<ILogger>();
                }
                return _logger;
            }
        }
    }

    internal static class Extension
    {
        public static void ComposeExportedValue(this CompositionContainer container, string contractName,
                                                object exportedValue)
        {
            if (container == null) throw new ArgumentNullException("container");
            if (exportedValue == null) throw new ArgumentNullException("exportedValue");
            CompositionBatch batch = new CompositionBatch();
            var metadata = new Dictionary<string, object>
                               {
                                   {
                                       "ExportTypeIdentity",
                                       AttributedModelServices.GetTypeIdentity(exportedValue.GetType())
                                       }
                               };
            batch.AddExport(new Export(contractName, metadata, () => exportedValue));
            container.Compose(batch);
        }

        public static void ComposeExportedValue(this CompositionContainer container, object exportedValue)
        {
            if (container == null) throw new ArgumentNullException("container");
            if (exportedValue == null) throw new ArgumentNullException("exportedValue");
            ComposeExportedValue(container,
                                 AttributedModelServices.GetContractName(exportedValue.GetType()), exportedValue);
        }
    }
}