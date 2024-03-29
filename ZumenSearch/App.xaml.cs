﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

using ZumenSearch.Activation;
using ZumenSearch.Contracts.Services;
using ZumenSearch.Core.Contracts.Services;
using ZumenSearch.Core.Services;
using ZumenSearch.Helpers;
using ZumenSearch.Services;
using ZumenSearch.ViewModels;
using ZumenSearch.Views;

using ZumenSearch.Models;
using ZumenSearch.Notifications;

namespace ZumenSearch;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{

    private readonly Microsoft.UI.Dispatching.DispatcherQueue _currentDispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
    public Microsoft.UI.Dispatching.DispatcherQueue CurrentDispatcherQueue => _currentDispatcherQueue;

    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
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

    public static WindowEx MainWindow { get; } = new MainWindow();

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers
            services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();

            // Services
            services.AddSingleton<IAppNotificationService, AppNotificationService>();
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Core Services
            services.AddSingleton<ISampleDataService, SampleDataService>();
            services.AddSingleton<IFileService, FileService>();

            // Views and ViewModels

            services.AddTransient<RentMainViewModel>();
            services.AddTransient<MainPage>();

            services.AddTransient<RentLivingSearchViewModel>();
            services.AddTransient<RentLivingSearchPage>();

            services.AddTransient<RentLivingSearchResultViewModel>();
            services.AddTransient<RentLivingSearchResultPage>();

            services.AddTransient<RentLivingEditShellViewModel>();
            services.AddTransient<RentLivingEditShellPage>(); 

            services.AddTransient<RentBussinessViewModel>();
            services.AddTransient<RentBussinessPage>();

            services.AddTransient<RentParkingViewModel>();
            services.AddTransient<RentParkingPage>();

            services.AddTransient<RentOwnerViewModel>();
            services.AddTransient<RentOwnerPage>();

            services.AddTransient<RealEstateBrokerViewModel>();
            services.AddTransient<RealEstateBrokerPage>();

            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsPage>();

            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        App.GetService<IAppNotificationService>().Initialize();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        //App.GetService<IAppNotificationService>().Show(string.Format("AppNotificationSamplePayload".GetLocalized(), AppContext.BaseDirectory));

        await App.GetService<IActivationService>().ActivateAsync(args);


    }
}
