using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using ZumenSearch.Helpers;
using ZumenSearch.Services;
using ZumenSearch.Services.Contracts;
using ZumenSearch.Services.Extensions;

namespace ZumenSearch;

public partial class App : Application
{
    // App basic info
    public static readonly string AppName = "ZumenSearch";
    private static readonly string AppDeveloper = "torum";

    // Data folder path
    private static readonly string EnvDataFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);//ApplicationData 
    public static string AppDataFolder { get; private set; } = System.IO.Path.Combine(System.IO.Path.Combine(EnvDataFolder, AppDeveloper), AppName);

    // "BlobData" includes building/unit pictures, PDF and its thumbnail image files.
    public static string AppDataPictureFolder { get; private set; } = System.IO.Path.Combine(AppDataFolder, "Blob");

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
            AppDataPictureFolder = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(AppDataFolder, AppDeveloper), AppName), "Pictures");
        }

        try
        {
            if (!System.IO.Directory.Exists(App.AppDataFolder))
            {
                System.IO.Directory.CreateDirectory(App.AppDataFolder);
            }
            if (!System.IO.Directory.Exists(App.AppDataPictureFolder))
            {
                System.IO.Directory.CreateDirectory(App.AppDataPictureFolder);
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
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IDataAccessService, DataAccessService>();
                services.AddTransient<INavigationGenericService, NavigationGenericService>();
                services.AddTransient<IModalDialogService, ModalDialogService>();
                services.AddTransient<IDataAccessLocationService, DataAccessLocationService>();
                services.AddTransient<IDataAccessTransportationService, DataAccessTransportationService>();

                // Views and ViewModels
                services.AddSingleton<ViewModels.MainViewModel>();
                services.AddSingleton<Views.MainWindow>();
                services.AddSingleton<Views.ShellPage>();

                services.AddTransient<ViewModels.Rent.Residentials.Bldg.MainViewModel>();
                services.AddTransient<Views.Rent.Residentials.Bldg.EditorWindow>();
                services.AddTransient<Views.Rent.Residentials.Bldg.ShellPage>();

                services.AddTransient<ViewModels.Rent.Residentials.Room.MainViewModel>();
                services.AddTransient<Views.Rent.Residentials.Room.EditorWindow>();
                services.AddTransient<Views.Rent.Residentials.Room.ShellPage>();

                services.AddEditorFactory<ViewModels.Rent.Residentials.Bldg.MainViewModel, Models.Rent.Residentials.Bldg.Property>();
                services.AddEditorFactory<Views.Rent.Residentials.Bldg.ShellPage, Models.Rent.Residentials.Bldg.Property>();
                services.AddEditorFactory<ViewModels.Rent.Residentials.Room.MainViewModel, Models.Rent.Residentials.Room.Listing>();
                services.AddEditorFactory<Views.Rent.Residentials.Room.ShellPage, Models.Rent.Residentials.Room.Listing>();
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

        // Create the window and load settings and apply size and position etc.
        var main = GetService<Views.MainWindow>();
        var shell = GetService<Views.ShellPage>();
        main.Content = shell;

        var navigationService = GetService<INavigationService>();
        navigationService.Initialize(shell.NavigationFrame);

        shell.CallMeAfterMainWindowIsCreated(main);

        main.Activate();
    }

    // Activated from other instance.
    private void App_Activated(object? sender, Microsoft.Windows.AppLifecycle.AppActivationArguments e)
    {
        CurrentDispatcherQueue?.TryEnqueue(() =>
        {
            var main = App.GetService<Views.MainWindow>();

            // Due to the bag of the Winui3, the window may not be activated.
            // see https://github.com/microsoft/microsoft-ui-xaml/issues/7595
            main?.Activate();
        });
    }

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
