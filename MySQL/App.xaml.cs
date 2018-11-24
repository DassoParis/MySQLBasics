using System;
using System.Windows;

namespace MySQL
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Custom startup first function that is load
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup (StartupEventArgs e)
        {
            /* Let the base application do what it needs */

            base.OnStartup(e);

            /* Show the main window */

            Current.MainWindow = new MainWindow();
            Current.MainWindow.Show();

            /* Non UI-Thread unhandled exception */

            AppDomain.CurrentDomain.UnhandledException += (sender, ee) =>
            {
                Exception ex = ee.ExceptionObject as Exception;

                UnhandledExceptionMessage (ex.Source, 
                                           ex.TargetSite.ToString(), 
                                           ex.GetType().ToString(), 
                                           ex.Message, 
                                           ex.ToString(), 
                                           "Unhandled Non UIThread Exception");

                Current.MainWindow.Close();
            };

            /* Main UI-Thread unhandled exception */

            DispatcherUnhandledException += (sender, ee) =>
            {
                UnhandledExceptionMessage (ee.Exception.Source, 
                                           ee.Exception.TargetSite.ToString(), 
                                           ee.Exception.GetType().ToString(), 
                                           ee.Exception.Message,
                                           ee.Exception.ToString(), 
                                           "Unhandled Main UIThread Exception");

                Current.MainWindow.Close();
            };

        }       

        private void UnhandledExceptionMessage(string source, string function, string type, string message, string stackTrace, string title)
        {
            MessageBox.Show("Namespace: \n   " + source +
                            "\n\nFunction: \n   " + function +
                            "\n\nType: \n   " + type +
                            "\n\nMessage: \n   " + message +
                            "\n\nStackTrace: \n   " + stackTrace
                           , title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
