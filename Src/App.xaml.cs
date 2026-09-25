using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WinRT.Interop;
using ZumenSearch.Helpers;
using ZumenSearch.Services;
using ZumenSearch.Services.Contracts;
using ZumenSearch.Services.Extensions;

namespace ZumenSearch;

public partial class App : Application
{
    // App basic info
    public const string AppName = "ZumenSearch";
    private const string AppDeveloper = "torum";

    // Data folder path
    private static readonly string _envDataFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);//ApplicationData 
    public static string AppDataFolder { get; private set; } = System.IO.Path.Combine(System.IO.Path.Combine(_envDataFolder, AppDeveloper), AppName);

    // "BlobData" includes building/unit pictures, PDF and its thumbnail image files.
    public static string PropertyBlobDataFolder { get; private set; } = System.IO.Path.Combine(AppDataFolder, "BlobData");

    // Config file path
    public static string AppConfigFilePath { get; private set; } = System.IO.Path.Combine(AppDataFolder, AppName + ".config");

    //public static Window Window { get; private set; } = null!;

    public static Microsoft.UI.Dispatching.DispatcherQueue CurrentDispatcherQueue { get; private set; } = null!;

    //public static nint WindowHandle => WinRT.Interop.WindowNative.GetWindowHandle(Window);

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
        var culture = new System.Globalization.CultureInfo("ja-JP");
        System.Globalization.CultureInfo.CurrentCulture = culture;
        System.Globalization.CultureInfo.CurrentUICulture = culture;
        Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "ja-JP";

        InitializeComponent();

        if (RuntimeHelper.IsMSIX)
        {
            Debug.WriteLine("IsMSIX");
            var envDataFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            AppDataFolder = System.IO.Path.Combine(System.IO.Path.Combine(envDataFolder, AppDeveloper), AppName);
            AppConfigFilePath = System.IO.Path.Combine(AppDataFolder, AppName + ".config");
            PropertyBlobDataFolder = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(AppDataFolder, AppDeveloper), AppName), "BlobData");
        }

        try
        {
            if (!System.IO.Directory.Exists(App.AppDataFolder))
            {
                System.IO.Directory.CreateDirectory(App.AppDataFolder);
            }
            if (!System.IO.Directory.Exists(App.PropertyBlobDataFolder))
            {
                System.IO.Directory.CreateDirectory(App.PropertyBlobDataFolder);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("CreateDirectory@App(): " + ex.Message);

            // Log the exception for debugging
            //AppendErrorLog("Failed to create folders on startup.", ex.ToString());
            //SaveErrorLog();
        }

        // Used for DispatcherService (passed as a parameter) to enqueue actions to the UI thread.
        CurrentDispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        Host = Microsoft.Extensions.Hosting.Host.
            CreateDefaultBuilder().
            UseContentRoot(AppContext.BaseDirectory).
            ConfigureServices((context, services) =>
            {
                // Services
                services.AddSingleton<IDispatcherService>(new DispatcherService(CurrentDispatcherQueue));
                services.AddSingleton<IDataAccessService, DataAccessService>();
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddTransient<INavigationGenericService, NavigationGenericService>();
                services.AddTransient<IDialogGenericService, DialogGenericService>();
                services.AddTransient<IDataAccessLocationService, DataAccessLocationService>();
                services.AddTransient<IDataAccessTransportationService, DataAccessTransportationService>();

                // Views and ViewModels
                services.AddSingleton<ViewModels.MainViewModel>();
                services.AddSingleton<Views.MainWindow>();
                services.AddSingleton<Views.ShellPage>();

                services.AddTransient<ViewModels.Rent.Residentials.PropertyViewModel>();
                services.AddTransient<Views.Rent.Residentials.ShellPage>();
                services.AddGenericFactory<Models.Rent.Residentials.Property, INavigationGenericService, IDialogGenericService, ViewModels.Rent.Residentials.PropertyViewModel>();
                services.AddGenericFactory<Models.Rent.Residentials.Property, Views.Rent.Residentials.ShellPage>();

                services.AddTransient<ViewModels.Rent.Residentials.Listing.ListingViewModel>();
                services.AddTransient<Views.Rent.Residentials.Listing.ShellPage>();
                services.AddGenericFactory<Models.Rent.Residentials.Listing.Listing, INavigationGenericService, IDialogGenericService, ViewModels.Rent.Residentials.Listing.ListingViewModel>();
                services.AddGenericFactory<Models.Rent.Residentials.Listing.Listing, Views.Rent.Residentials.Listing.ShellPage>();

                services.AddTransient<ViewModels.Rent.Lessors.LessorViewModel>();
                services.AddTransient<Views.Rent.Lessors.ShellPage>();
                services.AddGenericFactory<Models.Base.PersonBase, INavigationGenericService, IDialogGenericService, ViewModels.Rent.Lessors.LessorViewModel>();
                services.AddGenericFactory<Models.Base.PersonBase, Views.Rent.Lessors.ShellPage>();

                services.AddTransient<ViewModels.Brokers.BrokerViewModel>();
                services.AddTransient<Views.Brokers.ShellPage>();
                services.AddGenericFactory<Models.Base.PersonBase, INavigationGenericService, IDialogGenericService, ViewModels.Brokers.BrokerViewModel>();
                services.AddGenericFactory<Models.Base.PersonBase, Views.Brokers.ShellPage>();

                services.AddTransient<ViewModels.Sale.Residentials.PropertyViewModel>();
                services.AddTransient<Views.Sale.Residentials.ShellPage>();
                services.AddGenericFactory<Models.Sale.Residentials.Property, INavigationGenericService, IDialogGenericService, ViewModels.Sale.Residentials.PropertyViewModel>();
                services.AddGenericFactory<Models.Sale.Residentials.Property, Views.Sale.Residentials.ShellPage>();

                services.AddTransient<ViewModels.Rent.Commercials.PropertyViewModel>();
                services.AddTransient<Views.Rent.Commercials.ShellPage>();
                services.AddGenericFactory<Models.Rent.Commercials.Property, INavigationGenericService, IDialogGenericService, ViewModels.Rent.Commercials.PropertyViewModel>();
                services.AddGenericFactory<Models.Rent.Commercials.Property, Views.Rent.Commercials.ShellPage>();


                // Instead of AddEditorFactory for each, typeof.. <,> registers all.
                //services.AddSingleton(typeof(IAbstractFactory<,>), typeof(AbstractFactory<,>)); 

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
        var mainInstance = Microsoft.Windows.AppLifecycle.AppInstance.FindOrRegisterForKey(AppName + "Main");

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

        var shell = GetService<Views.ShellPage>();

        //main.AppWindow.Show();
        //main.Activate();
        shell.MainWindow.AppWindow.Show(true);
    }

    // Activated from other instance.
    private void App_Activated(object? sender, Microsoft.Windows.AppLifecycle.AppActivationArguments e)
    {
        CurrentDispatcherQueue?.TryEnqueue(() =>
        {
            var main = App.GetService<Views.MainWindow>();

            //main?.Activate();
            main?.AppWindow.Show(true);

            IntPtr hWnd = WindowNative.GetWindowHandle(main);
            NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // Ensure it's not minimized
            NativeMethods.SetForegroundWindow(hWnd); // Attempt to set it as the foreground window
        });
    }

    #region == BringToFront ==

    private static partial class NativeMethods
    {
        internal const int SW_RESTORE = 9; // Restores a minimized window and brings it to the foreground.

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetForegroundWindow(IntPtr hWnd);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

    }

    #endregion

    #region == UnhandledException ==

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        Debug.WriteLine("App_UnhandledException", e.Message);
        Debug.WriteLine($"StackTrace: {e.Exception.StackTrace}, Source: {e.Exception.Source}");
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        if (e.Exception.InnerException is not Exception exception)
        {
            return;
        }

        Debug.WriteLine("TaskScheduler_UnobservedTaskException: " + exception.Message);

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
        }
        else
        {
            Debug.WriteLine("CurrentDomain_UnhandledException: " + exception.Message);
        }
    }

    #endregion
}
