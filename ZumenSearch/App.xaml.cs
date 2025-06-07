using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using ZumenSearch.Services;
using ZumenSearch.Services.Extensions;
using ZumenSearch.ViewModels;
using ZumenSearch.Views;

namespace ZumenSearch
{
    public partial class App : Application
    {
        // App basic info
        private static readonly string _appName = "ZumenSearch";
        private static readonly string _appDeveloper = "torum";

        // Data folder path
        private static readonly string _envDataFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);//Windows.Storage.ApplicationData.Current.LocalFolder.Path;
        public static string AppDataFolder { get; } = _envDataFolder + System.IO.Path.DirectorySeparatorChar + _appDeveloper + System.IO.Path.DirectorySeparatorChar + _appName;

        // Config file path
        public static string AppConfigFilePath { get; } = System.IO.Path.Combine(AppDataFolder, _appName + ".config");

        // Log file
        public bool IsSaveErrorLog = true;
        public string LogFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + System.IO.Path.DirectorySeparatorChar + _appName + "_errors.txt";
        private readonly StringBuilder Errortxt = new();

        // DispatcherQueuecherQueue
        public static readonly Microsoft.UI.Dispatching.DispatcherQueue CurrentDispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        // Main Window
        public static MainWindow? MainWindow { get; private set; }

        // Getneric Host
        public IHost Host
        {
            get;
        }

        public static T GetService<T>()
            where T : class
        {
            if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
            {
                throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
            }

            return service;
        }

        public App()
        {
            this.InitializeComponent();

            Host = Microsoft.Extensions.Hosting.Host.
            CreateDefaultBuilder().
            UseContentRoot(AppContext.BaseDirectory).
            ConfigureServices((context, services) =>
            {
                // Services
                // TODO:
                //services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
                services.AddSingleton<IDataAccessService, DataAccessService>();

                // Views and ViewModels
                services.AddSingleton<MainShell>();
                services.AddSingleton<MainViewModel>();

                services.AddSingleton<Views.Rent.RentSearchPage>();

                services.AddSingleton<Views.Rent.Residentials.SearchPage>();
                services.AddSingleton<Views.Rent.Residentials.SearchResultPage>();
                
                services.AddTransient<Views.Rent.Residentials.Editor.EditorWindow>();
                services.AddTransient<ViewModels.Rent.Residentials.Editor.EditorViewModel>();
                services.AddEditorFactory<Views.Rent.Residentials.Editor.EditorShell>();

                services.AddSingleton<Views.Rent.Commercials.CommercialsPage>();
                services.AddSingleton<ViewModels.Rent.Commercials.CommercialsViewModel>();

                services.AddSingleton<Views.Rent.Parkings.ParkingsPage>();
                services.AddSingleton<ViewModels.Rent.Parkings.ParkingsViewModel>();

                services.AddSingleton<Views.Rent.Owners.OwnersPage>();
                services.AddSingleton<ViewModels.Rent.Owners.OwnersViewModel>();

                services.AddSingleton<Views.Brokers.BrokersPage>();
                services.AddSingleton<ViewModels.Brokers.BrokersViewModel>();

                services.AddSingleton<SettingsPage>();
                services.AddSingleton<SettingsViewModel>();
            }).
            Build();

            UnhandledException += App_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        protected async override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);
            /*
            * https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/migrate-to-windows-app-sdk/guides/applifecycle
            */
            // If this is the first instance launched, then register it as the "main" instance.
            // If this isn't the first instance launched, then "main" will already be registered,
            // so retrieve it.
            var mainInstance = Microsoft.Windows.AppLifecycle.AppInstance.FindOrRegisterForKey(_appName + "Main");

            // If the instance that's executing the OnLaunched handler right now
            // isn't the "main" instance.
            if (!mainInstance.IsCurrent)
            {
                // Redirect the activation (and args) to the "main" instance, and exit.
                var activatedEventArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();
                await mainInstance.RedirectActivationToAsync(activatedEventArgs);

                System.Diagnostics.Process.GetCurrentProcess().Kill();
                return;
            }
            else
            {
                // Otherwise, register for activation redirection
                Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().Activated += App_Activated;
            }

            // Create the window and load settings and apply size and position etc.
            MainWindow = new();
            // AFTER creating the main window, initialize main page and window.
            var shell = App.GetService<MainShell>();//new();
            // Set the the content of the app.
            MainWindow.Content = shell;
            // Activate the window.
            MainWindow?.Activate();


        }

        // Activated from other instance.
        private void App_Activated(object? sender, Microsoft.Windows.AppLifecycle.AppActivationArguments e)
        {
            CurrentDispatcherQueue?.TryEnqueue(() =>
            {
                // Due to the bag of the Winui3, the window may not be activated.
                // see https://github.com/microsoft/microsoft-ui-xaml/issues/7595
                MainWindow?.Activate();
            });
        }

        #region == UnhandledException ==

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine("App_UnhandledException", e.Message);
            Debug.WriteLine($"StackTrace: {e.Exception.StackTrace}, Source: {e.Exception.Source}");
            AppendErrorLog("App_UnhandledException", e.Message + System.Environment.NewLine + $"StackTrace: {e.Exception.StackTrace}, Source: {e.Exception.Source}");

            SaveErrorLog();
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            if (e.Exception.InnerException is not Exception exception)
            {
                return;
            }

            Debug.WriteLine("TaskScheduler_UnobservedTaskException: " + exception.Message);
            AppendErrorLog("TaskScheduler_UnobservedTaskException", exception.Message);
            SaveErrorLog();

            e.SetObserved();
        }

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is not Exception exception)
            {
                return;
            }

            if (exception is TaskCanceledException)
            {
                // can ignore.
                Debug.WriteLine("CurrentDomain_UnhandledException (TaskCanceledException): " + exception.Message);
                AppendErrorLog("CurrentDomain_UnhandledException (TaskCanceledException)", exception.Message);
            }
            else
            {
                Debug.WriteLine("CurrentDomain_UnhandledException: " + exception.Message);
                AppendErrorLog("CurrentDomain_UnhandledException", exception.Message);
                SaveErrorLog();
            }
        }

        public void AppendErrorLog(string kindTxt, string errorTxt)
        {
            Errortxt.AppendLine(kindTxt + ": " + errorTxt);
            var dt = DateTime.Now;
            Errortxt.AppendLine($"Occured at {dt.ToString("yyyy/MM/dd HH:mm:ss")}");
            Errortxt.AppendLine("");
        }

        public void SaveErrorLog()
        {
            if (!IsSaveErrorLog)
            {
                return;
            }

            if (string.IsNullOrEmpty(LogFilePath))
            {
                return;
            }

            if (Errortxt.Length > 0)
            {
                Errortxt.AppendLine("");
                var dt = DateTime.Now;
                Errortxt.AppendLine($"Saved at {dt.ToString("yyyy/MM/dd HH:mm:ss")}");

                var s = Errortxt.ToString();
                if (!string.IsNullOrEmpty(s))
                {
                    File.WriteAllText(LogFilePath, s);
                }
            }
        }

        #endregion
    }
}
