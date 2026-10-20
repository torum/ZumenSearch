using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Xml;
using System.Xml.Linq;
using WinRT;
using WinRT.Interop;
using ZumenSearch.Services;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.Views;

public sealed partial class MainWindow : Window
{
    private int _winRestoreWidth = 1274;
    private int _winRestoreHeight = 794;
    private int _winRestoreTop = 100;
    private int _winRestoreLeft = 100;

    private bool _isClosing;

    private readonly ViewModels.MainViewModel _viewModel;
    private readonly IDispatcherService _dispatcherService;
    private readonly IConfigurationService _configurationService;

    public MainWindow(ViewModels.MainViewModel viewModel, IDispatcherService dispatcherService, IConfigurationService configurationService)
    {
        _viewModel = viewModel;
        _dispatcherService = dispatcherService;
        _configurationService = configurationService;

        InitializeComponent();

        ExtendsContentIntoTitleBar = true;

        //this.AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets\\App.ico"));
        this.AppWindow.SetIcon("Assets/App.ico");

        LoadSetting();

        this.AppWindow.Closing += AppWindow_Closing;
        this.Closed += Window_Closed;
        this.SizeChanged += Window_SizeChanged;
        this.AppWindow.Changed += AppWindow_Changed;
        this.Activated += Window_Activated;

        if (this.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = 700;
            presenter.PreferredMinimumHeight = 600;
        }
    }

    private void LoadSetting()
    {
        var config = _configurationService.CurrentConfiguration;

        Debug.WriteLine($"LoadSetting: MainWindow: Width={config.MainWindow.Width}, Height={config.MainWindow.Height}, Top={config.MainWindow.Top}, Left={config.MainWindow.Left}, WindowState={config.MainWindow.WindowState}");

        if (config.MainWindow.Width < 500)
        {
            config.MainWindow.Width = 500;
        }
        if (config.MainWindow.Height < 500)
        {
            config.MainWindow.Height = 500;
        }
        if (config.MainWindow.Top < 0)
        {
            config.MainWindow.Top = 0;
        }
        if (config.MainWindow.Left < 0)
        {
            config.MainWindow.Left = 0;
        }

        if (config.MainWindow.RestoreWidth < 500)
        {
            config.MainWindow.RestoreWidth = 500;
        }
        if (config.MainWindow.RestoreHeight < 500)
        {
            config.MainWindow.RestoreHeight = 500;
        }
        if (config.MainWindow.RestoreTop < 0)
        {
            config.MainWindow.RestoreTop = 0;
        }
        if (config.MainWindow.RestoreLeft < 0)
        {
            config.MainWindow.RestoreLeft = 0;
        }

        // Restore window size and position
        Microsoft.UI.Windowing.AppWindow? appWindow = this.AppWindow;
        if (appWindow != null)
        {
            // AOT bad
            /*
            // https://github.com/microsoft/CsWinRT/issues/1930
            // Window state
            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
            }
            */

            if (config.MainWindow.WindowState == OverlappedPresenterState.Maximized)
            {
                // Sets restore size and position.
                appWindow.MoveAndResize(new Windows.Graphics.RectInt32(config.MainWindow.Left, config.MainWindow.Top, config.MainWindow.Width, config.MainWindow.Height));
                // Maximize the window.
                (appWindow.Presenter as OverlappedPresenter)!.Maximize();
            }
            else if (config.MainWindow.WindowState == OverlappedPresenterState.Minimized)
            {
                // This should not happen. DONT restore minimized size and pos.

                (appWindow.Presenter as OverlappedPresenter)!.Restore();

                appWindow.MoveAndResize(new Windows.Graphics.RectInt32(config.MainWindow.Left, config.MainWindow.Top, config.MainWindow.Width, config.MainWindow.Height));
            }
            else if (config.MainWindow.WindowState == OverlappedPresenterState.Restored)
            {
                // TODO:
                //var dpi = Windows.Win32.PInvoke.GetDpiForWindow(new Windows.Win32.Foundation.HWND(WinRT.Interop.WindowNative.GetWindowHandle(this)));
                //var scalingFactor = (float)dpi / 96;
                //AppWindow.Resize(new Windows.Graphics.SizeInt32((int)(400.0f * scalingFactor), (int)(300.0f * scalingFactor)));

                // Sets restore size and position.
                appWindow.MoveAndResize(new Windows.Graphics.RectInt32(config.MainWindow.Left, config.MainWindow.Top, config.MainWindow.Width, config.MainWindow.Height));
            }
        }

        //
        _viewModel.RentResidentialEditorWinTop = config.RentResidentialEditorWindow.Top;
        _viewModel.RentResidentialEditorWinLeft = config.RentResidentialEditorWindow.Left;
        _viewModel.RentResidentialEditorWinHeight = config.RentResidentialEditorWindow.Height;
        _viewModel.RentResidentialEditorWinWidth = config.RentResidentialEditorWindow.Width;
        if (_viewModel.RentResidentialEditorWinWidth < 500)
        {
            _viewModel.RentResidentialEditorWinWidth = 500;
        }
        if (_viewModel.RentResidentialEditorWinHeight < 500)
        {
            _viewModel.RentResidentialEditorWinHeight = 500;
        }
        if (_viewModel.RentResidentialEditorWinTop < 0)
        {
            _viewModel.RentResidentialEditorWinTop = 0;
        }
        if (_viewModel.RentResidentialEditorWinLeft < 0)
        {
            _viewModel.RentResidentialEditorWinLeft = 0;
        }

        //
        _viewModel.RentResidentialListingEditorWinTop = config.RentResidentialListingEditorWindow.Top;
        _viewModel.RentResidentialListingEditorWinLeft = config.RentResidentialListingEditorWindow.Left;
        _viewModel.RentResidentialListingEditorWinHeight = config.RentResidentialListingEditorWindow.Height;
        _viewModel.RentResidentialListingEditorWinWidth = config.RentResidentialListingEditorWindow.Width;
        if (_viewModel.RentResidentialListingEditorWinWidth < 500)
        {
            _viewModel.RentResidentialListingEditorWinWidth = 500;
        }
        if (_viewModel.RentResidentialListingEditorWinHeight < 500)
        {
            _viewModel.RentResidentialListingEditorWinHeight = 500;
        }
        if (_viewModel.RentResidentialListingEditorWinTop < 0)
        {
            _viewModel.RentResidentialListingEditorWinTop = 0;
        }
        if (_viewModel.RentResidentialListingEditorWinLeft < 0)
        {
            _viewModel.RentResidentialListingEditorWinLeft = 0;
        }

        //
        _viewModel.RentCommercialEditorWinWidth = config.RentCommercialEditorWindow.Width;
        _viewModel.RentCommercialEditorWinHeight = config.RentCommercialEditorWindow.Height;
        _viewModel.RentCommercialEditorWinTop = config.RentCommercialEditorWindow.Top;
        _viewModel.RentCommercialEditorWinLeft = config.RentCommercialEditorWindow.Left;
        if (_viewModel.RentCommercialEditorWinWidth < 500)
        {
            _viewModel.RentCommercialEditorWinWidth = 500;
        }
        if (_viewModel.RentCommercialEditorWinHeight < 500)
        {
            _viewModel.RentCommercialEditorWinHeight = 500;
        }
        if (_viewModel.RentCommercialEditorWinTop < 0)
        {
            _viewModel.RentCommercialEditorWinTop = 0;
        }
        if (_viewModel.RentCommercialEditorWinLeft < 0)
        {
            _viewModel.RentCommercialEditorWinLeft = 0;
        }

        //
        _viewModel.RentCommercialListingEditorWinTop = config.RentCommercialListingEditorWindow.Top;
        _viewModel.RentCommercialListingEditorWinLeft = config.RentCommercialListingEditorWindow.Left;
        _viewModel.RentCommercialListingEditorWinHeight = config.RentCommercialListingEditorWindow.Height;
        _viewModel.RentCommercialListingEditorWinWidth = config.RentCommercialListingEditorWindow.Width;
        if (_viewModel.RentCommercialListingEditorWinWidth < 500)
        {
            _viewModel.RentCommercialListingEditorWinWidth = 500;
        }
        if (_viewModel.RentCommercialListingEditorWinHeight < 500)
        {
            _viewModel.RentCommercialListingEditorWinHeight = 500;
        }
        if (_viewModel.RentCommercialListingEditorWinTop < 0)
        {
            _viewModel.RentCommercialListingEditorWinTop = 0;
        }
        if (_viewModel.RentCommercialListingEditorWinLeft < 0)
        {
            _viewModel.RentCommercialListingEditorWinLeft = 0;
        }

        //
        _viewModel.LessorEditorWinTop = config.LessorEditorWindow.Top;
        _viewModel.LessorEditorWinLeft = config.LessorEditorWindow.Left;
        _viewModel.LessorEditorWinHeight = config.LessorEditorWindow.Height;
        _viewModel.LessorEditorWinWidth = config.LessorEditorWindow.Width;
        if (_viewModel.LessorEditorWinWidth < 500)
        {
            _viewModel.LessorEditorWinWidth = 500;
        }
        if (_viewModel.LessorEditorWinHeight < 500)
        {
            _viewModel.LessorEditorWinHeight = 500;
        }
        if (_viewModel.LessorEditorWinTop < 0)
        {
            _viewModel.LessorEditorWinTop = 0;
        }
        if (_viewModel.LessorEditorWinLeft < 0)
        {
            _viewModel.LessorEditorWinLeft = 0;
        }

        //
        _viewModel.BrokerEditorWinTop = config.BrokerEditorWindow.Top;
        _viewModel.BrokerEditorWinLeft = config.BrokerEditorWindow.Left;
        _viewModel.BrokerEditorWinHeight = config.BrokerEditorWindow.Height;
        _viewModel.BrokerEditorWinWidth = config.BrokerEditorWindow.Width;
        if (_viewModel.BrokerEditorWinWidth < 500)
        {
            _viewModel.BrokerEditorWinWidth = 500;
        }
        if (_viewModel.BrokerEditorWinHeight < 500)
        {
            _viewModel.BrokerEditorWinHeight = 500;
        }
        if (_viewModel.BrokerEditorWinTop < 0)
        {
            _viewModel.BrokerEditorWinTop = 0;
        }
        if (_viewModel.BrokerEditorWinLeft < 0)
        {
            _viewModel.BrokerEditorWinLeft = 0;
        }

    }

    private async void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        Debug.WriteLine("AppWindow_Closing");
        
        args.Cancel = true;
        if (_isClosing) return;
        _isClosing = true;

        var isCancel = false;

        if (_viewModel.RentResidentialListingEditorList.Count > 0)
        {
            foreach (var editor in _viewModel.RentResidentialListingEditorList)
            {
                if (editor.ViewModel is null)
                {
                    Debug.WriteLine("AppWindow_Closing: editor.ViewModel is null");
                    continue;
                }
                if (editor.ViewModel.IsDirty)
                {
                    args.Cancel = true;
                    isCancel = true;

                    try
                    {
                        IntPtr hWnd = WindowNative.GetWindowHandle(editor);
                        NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // Ensure it's not minimized
                        NativeMethods.SetForegroundWindow(hWnd); // Attempt to set it as the foreground window

                        editor.Activate();
                        editor.AppWindow.MoveInZOrderAtTop();

                        // Show comfirmation dialog to user to save changes or not.
                        if (editor.Content is Views.Rent.Residentials.Listing.ShellPage shell)
                        {
                            await shell.ShowEditorCloseConfirmationDialog();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"@AppWindow_Closing: {ex}");
                        args.Cancel = true;
                        isCancel = true;
                        continue;
                    }
                    finally
                    {

                    }

                    break;
                }
            }

            if (!isCancel)
            {
                foreach (var editor in _viewModel.RentResidentialListingEditorList.ToList()) // Create snapshot of the list to avoid collection modification issues during iteration
                {
                    //IntPtr hWnd = WindowNative.GetWindowHandle(editor);
                    //NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // Ensure it's not minimized
                    
                    //editor.Activate();

                    editor.Close();
                }
            }
        }

        if (isCancel)
        {
            _isClosing = false;
            return;
        }

        if (_viewModel.RentResidentialEditorList.Count > 0)
        {
            foreach (var editor in _viewModel.RentResidentialEditorList)
            {
                if (editor.ViewModel is null)
                {
                    Debug.WriteLine("AppWindow_Closing: editor.ViewModel is null");
                    continue;
                }
                if (editor.ViewModel.IsDirty)
                {
                    args.Cancel = true;
                    isCancel = true;

                    try
                    {
                        IntPtr hWnd = WindowNative.GetWindowHandle(editor);
                        NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // Ensure it's not minimized
                        NativeMethods.SetForegroundWindow(hWnd); // Attempt to set it as the foreground window

                        editor.Activate();
                        editor.AppWindow.MoveInZOrderAtTop();

                        // Show comfirmation dialog to user to save changes or not.
                        if (editor.Content is Views.Rent.Residentials.ShellPage shell)
                        {
                            await shell.ShowEditorCloseConfirmationDialog();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"@AppWindow_Closing: {ex}");
                        args.Cancel = true;
                        isCancel = true;
                        continue;
                    }

                    break;
                }
            }

            if (!isCancel)
            {
                foreach (var editor in _viewModel.RentResidentialEditorList.ToList()) // Create snapshot of the list to avoid collection modification issues during iteration
                {
                    //IntPtr hWnd = WindowNative.GetWindowHandle(editor);
                    //NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // Ensure it's not minimized

                    //editor.Activate();

                    editor.Close();
                }
            }
        }

        if (isCancel)
        {
            _isClosing = false;
            return;
        }

        if (_viewModel.RentCommercialListingEditorList.Count > 0)
        {
            foreach (var editor in _viewModel.RentCommercialListingEditorList)
            {
                if (editor.ViewModel is null)
                {
                    Debug.WriteLine("AppWindow_Closing: editor.ViewModel is null");
                    continue;
                }
                if (editor.ViewModel.IsDirty)
                {
                    args.Cancel = true;
                    isCancel = true;

                    try
                    {
                        IntPtr hWnd = WindowNative.GetWindowHandle(editor);
                        NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // Ensure it's not minimized
                        NativeMethods.SetForegroundWindow(hWnd); // Attempt to set it as the foreground window

                        editor.Activate();
                        editor.AppWindow.MoveInZOrderAtTop();

                        // Show comfirmation dialog to user to save changes or not.
                        if (editor.Content is Views.Rent.Commercials.Listing.ShellPage shell)
                        {
                            await shell.ShowEditorCloseConfirmationDialog();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"@AppWindow_Closing: {ex}");
                        args.Cancel = true;
                        isCancel = true;
                        continue;
                    }

                    break;
                }
            }

            if (!isCancel)
            {
                foreach (var editor in _viewModel.RentCommercialListingEditorList.ToList()) // Create snapshot of the list to avoid collection modification issues during iteration
                {
                    //editor.IsAutoClose = true;

                    //IntPtr hWnd = WindowNative.GetWindowHandle(editor);
                    //NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // Ensure it's not minimized

                    //editor.Activate();

                    editor.Close();
                }
            }
        }

        if (isCancel)
        {
            _isClosing = false;
            return;
        }

        if (_viewModel.RentCommercialEditorList.Count > 0)
        {
            foreach (var editor in _viewModel.RentCommercialEditorList)
            {
                if (editor.ViewModel is null)
                {
                    Debug.WriteLine("AppWindow_Closing: editor.ViewModel is null");
                    continue;
                }
                if (editor.ViewModel.IsDirty)
                {
                    args.Cancel = true;
                    isCancel = true;

                    try
                    {
                        IntPtr hWnd = WindowNative.GetWindowHandle(editor);
                        NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // Ensure it's not minimized
                        NativeMethods.SetForegroundWindow(hWnd); // Attempt to set it as the foreground window

                        editor.Activate();
                        editor.AppWindow.MoveInZOrderAtTop();

                        // Show comfirmation dialog to user to save changes or not.
                        if (editor.Content is Views.Rent.Commercials.ShellPage shell)
                        {
                            await shell.ShowEditorCloseConfirmationDialog();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"@AppWindow_Closing: {ex}");
                        args.Cancel = true;
                        isCancel = true;
                        continue;
                    }

                    break;
                }
            }

            if (!isCancel)
            {
                foreach (var editor in _viewModel.RentCommercialEditorList.ToList()) // Create snapshot of the list to avoid collection modification issues during iteration
                {
                    //editor.IsAutoClose = true;

                    //IntPtr hWnd = WindowNative.GetWindowHandle(editor);
                    //NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // Ensure it's not minimized

                    //editor.Activate();

                    editor.Close();
                }
            }
        }

        if (isCancel)
        {
            _isClosing = false;
            return;
        }

        if (_viewModel.LessorEditorList.Count > 0)
        {
            foreach (var editor in _viewModel.LessorEditorList)
            {
                if (editor.ViewModel is null)
                {
                    Debug.WriteLine("AppWindow_Closing: editor.ViewModel is null");
                    continue;
                }
                if (editor.ViewModel.IsDirty)
                {
                    args.Cancel = true;
                    isCancel = true;

                    try
                    {
                        IntPtr hWnd = WindowNative.GetWindowHandle(editor);
                        NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // Ensure it's not minimized
                        NativeMethods.SetForegroundWindow(hWnd); // Attempt to set it as the foreground window

                        editor.Activate();
                        editor.AppWindow.MoveInZOrderAtTop();

                        // Show comfirmation dialog to user to save changes or not.
                        if (editor.Content is Views.Rent.Lessors.ShellPage shell)
                        {
                            await shell.ShowEditorCloseConfirmationDialog();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"@AppWindow_Closing: {ex}");
                        args.Cancel = true;
                        isCancel = true;
                        continue;
                    }

                    break;
                }
            }

            if (!isCancel)
            {
                foreach (var editor in _viewModel.LessorEditorList.ToList()) // Create snapshot of the list to avoid collection modification issues during iteration
                {
                    //IntPtr hWnd = WindowNative.GetWindowHandle(editor);
                    //NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // Ensure it's not minimized

                    //editor.Activate();

                    editor.Close();
                }
            }
        }

        if (isCancel)
        {
            _isClosing = false;
            return;
        }

        if (_viewModel.BrokerEditorList.Count > 0)
        {
            foreach (var editor in _viewModel.BrokerEditorList)
            {
                if (editor.ViewModel is null)
                {
                    Debug.WriteLine("AppWindow_Closing: editor.ViewModel is null");
                    continue;
                }
                if (editor.ViewModel.IsDirty)
                {
                    args.Cancel = true;
                    isCancel = true;

                    try
                    {
                        IntPtr hWnd = WindowNative.GetWindowHandle(editor);
                        NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // Ensure it's not minimized
                        NativeMethods.SetForegroundWindow(hWnd); // Attempt to set it as the foreground window

                        editor.Activate();
                        editor.AppWindow.MoveInZOrderAtTop();

                        // Show comfirmation dialog to user to save changes or not.
                        if (editor.Content is Views.Brokers.ShellPage shell)
                        {
                            await shell.ShowEditorCloseConfirmationDialog();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"@AppWindow_Closing: {ex}");
                        args.Cancel = true;
                        isCancel = true;
                        continue;
                    }

                    break;
                }
            }

            if (!isCancel)
            {
                foreach (var editor in _viewModel.BrokerEditorList.ToList()) // Create snapshot of the list to avoid collection modification issues during iteration
                {
                    editor.IsAutoClose = true;

                    //IntPtr hWnd = WindowNative.GetWindowHandle(editor);
                    //NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // Ensure it's not minimized

                    //editor.Activate();

                    editor.Close();
                }
            }
        }

        if (isCancel)
        {
            _isClosing = false;
            return;
        }

        // 
        _viewModel.UpdateSettings();

        // Save window state and position, etc.
        await SaveSettingAsync();

        this.Close();
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        Debug.WriteLine("MainWindow_Closed");

        this.AppWindow.Closing -= AppWindow_Closing;
        this.Closed -= Window_Closed;
        this.SizeChanged -= Window_SizeChanged;
        this.AppWindow.Changed -= AppWindow_Changed;
        this.Activated -= Window_Activated;
    }

    private async Task SaveSettingAsync()
    {
        var config = _configurationService.CurrentConfiguration;

        OverlappedPresenterState? winState = null; // For AOT workaround, don't set default.  = OverlappedPresenterState.Restored;

        // AOT bad
        // https://github.com/microsoft/CsWinRT/issues/1930
        if (this.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            // AOT bad
            //OverlappedPresenter? presenter = appWindow.Presenter as OverlappedPresenter;

            if (presenter?.State == OverlappedPresenterState.Maximized)
            {
                winState = OverlappedPresenterState.Maximized;
            }
            else if (presenter?.State == OverlappedPresenterState.Minimized)
            {
                winState = OverlappedPresenterState.Minimized;
            }
            else if (presenter?.State == OverlappedPresenterState.Restored)
            {
                winState = OverlappedPresenterState.Restored;
            }
        }
        else
        {
            //App.AppendErrorLog("AOT Error",$"{appWindow.Presenter}"); //Microsoft.UI.Windowing.AppWindowPresenter
            //App.SaveErrorLog();

            try
            {
                // For AOT tmp workaround
                OverlappedPresenter presenterAOT = this.AppWindow.Presenter.As<OverlappedPresenter>();

                if (presenterAOT.State == OverlappedPresenterState.Maximized)
                {
                    winState = OverlappedPresenterState.Maximized;
                }
                else if (presenterAOT.State == OverlappedPresenterState.Minimized)
                {
                    winState = OverlappedPresenterState.Minimized;
                }
                else if (presenterAOT.State == OverlappedPresenterState.Restored)
                {
                    winState = OverlappedPresenterState.Restored;
                }
            }
            catch (Exception ex)
            {
                (Application.Current as App)?.AppendErrorLog("AOT Error@SaveSettings", $"{ex}");
                (Application.Current as App)?.SaveErrorLog();
            }

        }

        // For AOT, check if winState is null or not.
        if (winState != null)
        {
            // Main Window attributes
            if (winState == OverlappedPresenterState.Restored)
            {
                config.MainWindow.Width = this.AppWindow.Size.Width;
            }
            else
            {
                config.MainWindow.Width = _winRestoreWidth;
            }
            config.MainWindow.RestoreWidth = _winRestoreWidth;

            if (winState == OverlappedPresenterState.Restored)
            {
                config.MainWindow.Height = this.AppWindow.Size.Height;
            }
            else
            {
                config.MainWindow.Height = _winRestoreHeight;
            }
            config.MainWindow.RestoreHeight = _winRestoreHeight;

            if (winState == OverlappedPresenterState.Restored)
            {
                config.MainWindow.Top = this.AppWindow.Position.Y;
            }
            else
            {
                config.MainWindow.Top = _winRestoreTop;
            }
            config.MainWindow.RestoreTop = _winRestoreTop;

            if (winState == OverlappedPresenterState.Restored)
            {
                config.MainWindow.Left = this.AppWindow.Position.X;
            }
            else
            {
                config.MainWindow.Left = _winRestoreLeft;
            }
            config.MainWindow.RestoreLeft = _winRestoreLeft;

            if (winState == OverlappedPresenterState.Maximized)
            {
                //attrs.Value = "Maximized";
                config.MainWindow.WindowState = OverlappedPresenterState.Maximized;
            }
            else if (winState == OverlappedPresenterState.Restored)
            {
                //attrs.Value = "Normal";
                config.MainWindow.WindowState = OverlappedPresenterState.Restored;
            }
            else if (winState == OverlappedPresenterState.Minimized)
            {
                //attrs.Value = "Minimized";
                config.MainWindow.WindowState = OverlappedPresenterState.Restored; // Ignore actual value.
            }
        }

        config.RentResidentialEditorWindow.Top = _viewModel.RentResidentialEditorWinTop;
        config.RentResidentialEditorWindow.Left = _viewModel.RentResidentialEditorWinLeft;
        config.RentResidentialEditorWindow.Height = _viewModel.RentResidentialEditorWinHeight;
        config.RentResidentialEditorWindow.Width = _viewModel.RentResidentialEditorWinWidth;

        config.RentResidentialListingEditorWindow.Top = _viewModel.RentResidentialListingEditorWinTop;
        config.RentResidentialListingEditorWindow.Left = _viewModel.RentResidentialListingEditorWinLeft;
        config.RentResidentialListingEditorWindow.Height = _viewModel.RentResidentialListingEditorWinHeight;
        config.RentResidentialListingEditorWindow.Width = _viewModel.RentResidentialListingEditorWinWidth;

        config.RentCommercialEditorWindow.Top = _viewModel.RentCommercialEditorWinTop;
        config.RentCommercialEditorWindow.Left = _viewModel.RentCommercialEditorWinLeft;
        config.RentCommercialEditorWindow.Height = _viewModel.RentCommercialEditorWinHeight;
        config.RentCommercialEditorWindow.Width = _viewModel.RentCommercialEditorWinWidth;

        config.RentCommercialListingEditorWindow.Top = _viewModel.RentCommercialListingEditorWinTop;
        config.RentCommercialListingEditorWindow.Left = _viewModel.RentCommercialListingEditorWinLeft;
        config.RentCommercialListingEditorWindow.Height = _viewModel.RentCommercialListingEditorWinHeight;
        config.RentCommercialListingEditorWindow.Width = _viewModel.RentCommercialListingEditorWinWidth;

        config.LessorEditorWindow.Top = _viewModel.LessorEditorWinTop;
        config.LessorEditorWindow.Left = _viewModel.LessorEditorWinLeft;
        config.LessorEditorWindow.Height = _viewModel.LessorEditorWinHeight;
        config.LessorEditorWindow.Width = _viewModel.LessorEditorWinWidth;

        config.BrokerEditorWindow.Top = _viewModel.BrokerEditorWinTop;
        config.BrokerEditorWindow.Left = _viewModel.BrokerEditorWinLeft;
        config.BrokerEditorWindow.Height = _viewModel.BrokerEditorWinHeight;
        config.BrokerEditorWindow.Width = _viewModel.BrokerEditorWinWidth;

        await _configurationService.SaveConfigurationAsync();

    }

    private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        if (this.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            if (presenter.State == OverlappedPresenterState.Restored)
            {
                _winRestoreHeight = (int)this.AppWindow.Size.Height;
                _winRestoreWidth = (int)this.AppWindow.Size.Width;
                _winRestoreTop = (int)this.AppWindow.Position.Y;
                _winRestoreLeft = (int)this.AppWindow.Position.X;
            }
        }
    }

    private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
    {
        if (!args.DidPositionChange)
        {
            return;
        }

        //Microsoft.UI.Windowing.AppWindow? appWindow = this.AppWindow;
        if (sender == null)
        {
            return;
        }
        if (sender.Presenter is not OverlappedPresenter presenter)
        {
            return;
        }
        if (presenter.State == OverlappedPresenterState.Restored)
        {
            _winRestoreHeight = sender.Size.Height;
            _winRestoreWidth = sender.Size.Width;
            _winRestoreTop = sender.Position.Y;
            _winRestoreLeft = sender.Position.X;
        }
    }

    private void Window_Activated(object sender, WindowActivatedEventArgs args)
    {
        // Ensure that the restore position and sizes are updated when the window is activated, especially after being minimized or restored.
        Microsoft.UI.Windowing.AppWindow? appWindow = this.AppWindow;
        if (appWindow == null)
        {
            return;
        }
        if (appWindow.Presenter is not OverlappedPresenter presenter)
        {
            return;
        }
        if (presenter.State == OverlappedPresenterState.Restored)
        {
            _winRestoreHeight = appWindow.Size.Height;
            _winRestoreWidth = appWindow.Size.Width;
            _winRestoreTop = appWindow.Position.Y;
            _winRestoreLeft = appWindow.Position.X;
        }
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
}
