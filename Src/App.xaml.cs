using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WinRT.Interop;
using ZumenSearch.Application.Contracts;
using ZumenSearch.Application.Services;
using ZumenSearch.Application.Services.Rent.Residentials;
using ZumenSearch.Infrastructure.Configuration;
using ZumenSearch.Infrastructure.Configuration.Contracts;
using ZumenSearch.Infrastructure.DataAccess;
using ZumenSearch.Infrastructure.DataAccess.Contracts;
using ZumenSearch.Infrastructure.Platform;
using ZumenSearch.Presentation.Factories;
using ZumenSearch.Presentation.Services;
using ZumenSearch.Presentation.Services.Contracts;


namespace ZumenSearch;

public partial class App : Microsoft.UI.Xaml.Application
{
    // App basic info
    public const string AppName = "ZumenSearch";
    private const string AppDeveloper = "torum";

    // Data folder path
    private static readonly string _envDataFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);//ApplicationData 
    public static string AppDataFolder { get; private set; } = System.IO.Path.Combine(_envDataFolder, AppDeveloper, AppName);

    // "BlobData" includes building/unit pictures, PDF and its thumbnail image files.
    public static string PropertyBlobDataFolder { get; private set; } = System.IO.Path.Combine(AppDataFolder, "BlobData");

    // Config file path
    public static string AppConfigFilePath { get; private set; } = System.IO.Path.Combine(AppDataFolder, AppName + ".config");

    public static Microsoft.UI.Dispatching.DispatcherQueue CurrentDispatcherQueue { get; private set; } = null!;

    private Task? _appHostStartTask;
    private Task? _appHostStopTask;

    // ErrorLog
    private static readonly string _logFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + System.IO.Path.DirectorySeparatorChar + "ZumenSearch_errors.txt";
    private readonly StringBuilder _errortxt = new();
    private readonly Lock _errorLogLock = new();
#if DEBUG
    public bool IsSaveErrorLog { get; set; } = true;
#else
    public bool IsSaveErrorLog { get; set; } = false;
#endif

    public IHost Host
    {
        get;
    }

    public static T GetService<T>() where T : class
    {
        return ((App)Current).Host.Services.GetRequiredService<T>();
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
            AppDataFolder = System.IO.Path.Combine(envDataFolder, AppDeveloper, AppName);
            AppConfigFilePath = System.IO.Path.Combine(AppDataFolder, AppName + ".config");
            PropertyBlobDataFolder = System.IO.Path.Combine(AppDataFolder, "BlobData");
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
            Debug.WriteLine("CreateDirectory @App(): " + ex.Message);

            // Log the exception for debugging
            AppendErrorLog("Failed to create folders on startup.", ex.ToString());
            SaveErrorLog();
        }

        // Used for DispatcherService (passed as a parameter) to enqueue actions to the UI thread.
        CurrentDispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        Host = Microsoft.Extensions.Hosting.Host.
            CreateDefaultBuilder().
            UseContentRoot(AppContext.BaseDirectory).
            ConfigureLogging((context, logging) =>
            {
#if DEBUG
                logging.AddFilter("Microsoft.Extensions.Hosting", LogLevel.Debug);
                logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Information);
#else
                // Strips log destinations and shuts down the framework engines for production
                logging.ClearProviders(); 
                logging.AddFilter(null, LogLevel.None); 
#endif
            }).
            ConfigureServices((context, services) =>
            {
                // Services
                services.AddSingleton<IDispatcherService>(new DispatcherService(CurrentDispatcherQueue));
                services.AddSingleton<DataAccessService>();
                services.AddSingleton<IDataAccessService>(provider => provider.GetRequiredService<DataAccessService>());
                services.AddSingleton<IPropertySearchRepository>(provider => provider.GetRequiredService<DataAccessService>());
                services.AddSingleton<IRentResidentialPropertyRepository>(provider => provider.GetRequiredService<DataAccessService>());
                services.AddSingleton<IRentResidentialPersonRepository>(provider => provider.GetRequiredService<DataAccessService>());
                services.AddSingleton<IRentResidentialListingRepository>(provider => provider.GetRequiredService<DataAccessService>());
                services.AddTransient<PropertySearchService>();
                services.AddTransient<RentResidentialPropertyService>();
                services.AddTransient<RentResidentialListingService>();
                services.AddTransient<RentPersonService>();
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddTransient<INavigationGenericService, NavigationGenericService>();
                services.AddTransient<IDialogGenericService, DialogGenericService>();
                services.AddTransient<IDataAccessLocationService, DataAccessLocationService>();
                services.AddTransient<ILocationRepository, DataAccessLocationService>();
                services.AddTransient<IDataAccessTransportationService, DataAccessTransportationService>();
                services.AddSingleton<IConfigurationService>(new ConfigurationService(AppendErrorLog));

                // Views and ViewModels
                services.AddSingleton<ZumenSearch.Presentation.Shell.ViewModels.MainViewModel>();
                services.AddSingleton<ZumenSearch.Presentation.Shell.Views.MainWindow>();
                services.AddSingleton<ZumenSearch.Presentation.Shell.Views.ShellPage>();

                services.AddTransient<ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyViewModel>();
                services.AddTransient<ZumenSearch.Presentation.Features.Rent.Residentials.Views.ShellPage>();
                services.AddGenericFactory<ZumenSearch.Domain.Entities.Rent.Residentials.Property, bool, INavigationGenericService, IDialogGenericService, ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyViewModel>();
                services.AddGenericFactory<ZumenSearch.Domain.Entities.Rent.Residentials.Property, bool, ZumenSearch.Presentation.Features.Rent.Residentials.Views.ShellPage>();

                services.AddTransient<ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.ListingViewModel>();
                services.AddTransient<ZumenSearch.Presentation.Features.Rent.Residentials.Views.Listing.ShellPage>();
                services.AddGenericFactory<ZumenSearch.Domain.Entities.Rent.Residentials.Listing, bool, INavigationGenericService, IDialogGenericService, ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.ListingViewModel>();
                services.AddGenericFactory<ZumenSearch.Domain.Entities.Rent.Residentials.Listing, bool, ZumenSearch.Presentation.Features.Rent.Residentials.Views.Listing.ShellPage>();

                services.AddTransient<ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyViewModel>();
                services.AddTransient<ZumenSearch.Presentation.Features.Rent.Commercials.Views.ShellPage>();
                services.AddGenericFactory<ZumenSearch.Domain.Entities.Rent.Commercials.Property, INavigationGenericService, IDialogGenericService, ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyViewModel>();
                services.AddGenericFactory<ZumenSearch.Domain.Entities.Rent.Commercials.Property, ZumenSearch.Presentation.Features.Rent.Commercials.Views.ShellPage>();

                services.AddTransient<ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel>();
                services.AddTransient<ZumenSearch.Presentation.Features.Rent.Commercials.Views.Listing.ShellPage>();
                services.AddGenericFactory<ZumenSearch.Domain.Entities.Rent.Commercials.Listing.Listing, INavigationGenericService, IDialogGenericService, ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel>();
                services.AddGenericFactory<ZumenSearch.Domain.Entities.Rent.Commercials.Listing.Listing, ZumenSearch.Presentation.Features.Rent.Commercials.Views.Listing.ShellPage>();

                services.AddTransient<ZumenSearch.Presentation.Features.Rent.Lessors.ViewModels.LessorViewModel>();
                services.AddTransient<ZumenSearch.Presentation.Features.Rent.Lessors.Views.ShellPage>();
                services.AddGenericFactory<ZumenSearch.Domain.Entities.Base.PersonBase, INavigationGenericService, IDialogGenericService, ZumenSearch.Presentation.Features.Rent.Lessors.ViewModels.LessorViewModel>();
                services.AddGenericFactory<ZumenSearch.Domain.Entities.Base.PersonBase, ZumenSearch.Presentation.Features.Rent.Lessors.Views.ShellPage>();

                services.AddTransient<ZumenSearch.Presentation.Features.Sale.Residentials.ViewModels.PropertyViewModel>();
                services.AddTransient<ZumenSearch.Presentation.Features.Sale.Residentials.Views.ShellPage>();
                services.AddGenericFactory<ZumenSearch.Domain.Entities.Sale.Residentials.Property, INavigationGenericService, IDialogGenericService, ZumenSearch.Presentation.Features.Sale.Residentials.ViewModels.PropertyViewModel>();
                services.AddGenericFactory<ZumenSearch.Domain.Entities.Sale.Residentials.Property, ZumenSearch.Presentation.Features.Sale.Residentials.Views.ShellPage>();

                services.AddTransient<ZumenSearch.Presentation.Features.Brokers.ViewModels.BrokerViewModel>();
                services.AddTransient<ZumenSearch.Presentation.Features.Brokers.Views.ShellPage>();
                services.AddGenericFactory<ZumenSearch.Domain.Entities.Base.PersonBase, INavigationGenericService, IDialogGenericService, ZumenSearch.Presentation.Features.Brokers.ViewModels.BrokerViewModel>();
                services.AddGenericFactory<ZumenSearch.Domain.Entities.Base.PersonBase, ZumenSearch.Presentation.Features.Brokers.Views.ShellPage>();

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
        try
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

                //System.Diagnostics.Process.GetCurrentProcess().Kill();
                Environment.Exit(0);
                return;
            }
            else
            {
                // Otherwise, register for activation redirection
                Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().Activated += App_Activated;
            }

            var configurationService = GetService<IConfigurationService>();
            configurationService.Initialize(AppConfigFilePath);
            await configurationService.LoadAppSettingsAsync();

            _appHostStartTask = StartAppHostAsync();
            var shell = GetService<ZumenSearch.Presentation.Shell.Views.ShellPage>();
            shell.MainWindow.Closed += (sender, e) =>
            {
                _appHostStopTask = StopAppHostAsync();
            };
            shell.MainWindow.Activate();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnLaunched: {ex}");
            AppendErrorLog("OnLaunched", ex.ToString());
            SaveErrorLog();

            // Startup failed and the app is in an unknown state, so exit instead of leaving a half-initialized process.
            Environment.Exit(1);
        }
    }

    private async Task StartAppHostAsync()
    {
        try
        {
            await Host.StartAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("StartAppHostAsync: {0}", ex);
            //AppendErrorLog("AppHost.StartAsync", ex.ToString());
            //SaveErrorLog();
        }

        Debug.WriteLine("AppHost started.");
    }

    private async Task StopAppHostAsync()
    {
        if (_appHostStartTask != null)
        {
            await _appHostStartTask;
        }
        try
        {
            await Host.StopAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("StopAppHostAsync: {0}", ex);
            AppendErrorLog("AppHost.StopAsync", ex.ToString());
        }
        finally
        {
            Host.Dispose();
            Debug.WriteLine("AppHost Dispose().");
            SaveErrorLog();
        }
    }

    // Activated from other instance.
    private void App_Activated(object? sender, Microsoft.Windows.AppLifecycle.AppActivationArguments e)
    {
        CurrentDispatcherQueue?.TryEnqueue(() =>
        {
            var main = App.GetService<ZumenSearch.Presentation.Shell.Views.MainWindow>();

            main?.Activate();
            //main?.AppWindow.Show(true);
            main?.AppWindow.MoveInZOrderAtTop();

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
        //Debug.WriteLine("App_UnhandledException", $"{e.Exception.Message}, {e.Exception.Data}");

        // e.Exception can lose detail when it crosses the WinRT boundary,
        // but ToString() still gives the best available type, message, HResult and stack trace.
        var details = $"{e.Exception}{Environment.NewLine}HResult: 0x{e.Exception.HResult:X8}{Environment.NewLine}Message: {e.Message}";

        Debug.WriteLine($"App_UnhandledException: {details}");
        AppendErrorLog("App_UnhandledException", details);
        SaveErrorLog();

        // Mark as handled only for exceptions that are known to be recoverable.
        // Leaving it unhandled lets the app terminate, which is safer for unknown states.
        if (e.Exception is OperationCanceledException)
        {
            e.Handled = true;
        }
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        // Flatten so nested AggregateExceptions are unwrapped, and log full details (type, message, stack trace).
        var details = e.Exception.Flatten().ToString();

        Debug.WriteLine($"TaskScheduler_UnobservedTaskException: {details}");
        AppendErrorLog("TaskScheduler_UnobservedTaskException", details);
        SaveErrorLog();

        e.SetObserved();
    }

    private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is not Exception exception)
        {
            var raw = e.ExceptionObject?.ToString() ?? "Unknown exception object";
            Debug.WriteLine($"CurrentDomain_UnhandledException: {raw}");
            AppendErrorLog("CurrentDomain_UnhandledException", raw);
            SaveErrorLog();
            return;
        }

        if (exception is TaskCanceledException)
        {
            // can ignore.
            Debug.WriteLine($"CurrentDomain_UnhandledException (TaskCanceledException): {exception.Message}");
            return;
        }

        // Log full details (type, message, stack trace, inner exceptions).
        var details = exception.ToString();

        Debug.WriteLine($"CurrentDomain_UnhandledException (IsTerminating={e.IsTerminating}): {details}");
        AppendErrorLog("CurrentDomain_UnhandledException", details);
        SaveErrorLog();
    }

    public void AppendErrorLog(string kindTxt, string errorTxt)
    {
        if (!IsSaveErrorLog)
        {
            return;
        }
        var timestamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
        lock (_errorLogLock)
        {
            _errortxt.AppendLine(kindTxt + ": " + errorTxt);
            _errortxt.AppendLine(CultureInfo.InvariantCulture, $"Occurred at {timestamp}");
        }
    }

    public void SaveErrorLog()
    {
        if (!IsSaveErrorLog)
        {
            return;
        }

        if (string.IsNullOrEmpty(_logFilePath))
        {
            return;
        }

        var timestamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);

        lock (_errorLogLock)
        {
            if (_errortxt.Length == 0)
            {
                return;
            }

            _errortxt.AppendLine(CultureInfo.InvariantCulture, $"Saved at {timestamp}");
            _errortxt.AppendLine();

            try
            {
                File.WriteAllText(_logFilePath, _errortxt.ToString());
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // The error logger must never throw from inside an exception handler.
                Debug.WriteLine($"SaveErrorLog failed: {ex.Message}");
            }
        }
    }

    #endregion
}
