using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.System;
using Windows.UI.Core;
using ZumenSearch.Models;
using ZumenSearch.ViewModels;
using ZumenSearch.Views;

namespace ZumenSearch.Views.Rent.Residentials.Editor;

public sealed partial class UnitListPage : Page
{
    private Views.Rent.Residentials.Editor.EditorShell? _editorShell;

    private ViewModels.Rent.Residentials.Editor.EditorViewModel? _viewModel;
    public ViewModels.Rent.Residentials.Editor.EditorViewModel? ViewModel
    {
        get => _viewModel;
        private set
        {
            if (value != null)
            {
                _viewModel = value;
                _viewModel.EventAddNew += (sender, arg) => OnEventAddNew(arg);
            }
        }
    }

    public UnitListPage()
    {
        //ViewModel = new ViewModels.Rent.Residentials.Editor.UnitListViewModel();//App.GetService<RentLivingEditUnitShellViewModel>();

        InitializeComponent();

        BreadcrumbBar1.ItemsSource = new ObservableCollection<Breadcrumb>{
            new() { Name = "部屋", Page = typeof(ZumenSearch.Views.Rent.Residentials.Editor.UnitListPage).FullName! },
        };
        BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;

    }
    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Index == 0)
        {
            //_editorShell?.NavFrame.Navigate(typeof(Views.Rent.Residentials.Editor.SummaryPage), _editorShell, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }
    }

    public void OnEventAddNew(string arg)
    {
        // TODO: arg is temp.
        /*
        if (ContentFrame == null)
        {
            return;
        }
        */
        // 
        //ContentFrame.Navigate(typeof(RentLivingEdit.RentLivingEditUnitEditShellPage), ContentFrame, new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());


        if (_editorShell == null) 
        {
            throw new ArgumentNullException(nameof(_editorShell));
        } 

        if (_editorShell.EditorWin == null)
        {
            // EditorWin should be initialized in the EditorShell constructor.
            throw new ArgumentNullException(nameof(_editorShell.EditorWin));
        }

        Views.Rent.Residentials.Editor.Modal.ModalWindow dialogWin = new();
        dialogWin.Content = new Views.Rent.Residentials.Editor.Modal.ModalShell(dialogWin);

        //TODO: stupid WinUI3... 
        // https://github.com/microsoft/microsoft-ui-xaml/issues/10396
        // https://github.com/microsoft/WindowsAppSDK/discussions/3680

        IntPtr hWndDialog = WinRT.Interop.WindowNative.GetWindowHandle(dialogWin);
        //Microsoft.UI.WindowId windowId1 = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd1);
        //Microsoft.UI.Windowing.AppWindow appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId1);
        //Microsoft.UI.Windowing.OverlappedPresenter presenter = appWindow.Presenter as Microsoft.UI.Windowing.OverlappedPresenter;
        IntPtr hWndEditor = WinRT.Interop.WindowNative.GetWindowHandle(_editorShell.EditorWin);
        SetWindowLong(hWndDialog, GWL_HWNDPARENT, hWndEditor);

        Microsoft.UI.Windowing.AppWindow? appWindow = dialogWin.AppWindow;
        if (appWindow != null)
        {
            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsModal = true;

                EnableWindow(hWndEditor, false);

                dialogWin.Closed += (sender, e) =>
                {
                    // When the dialog is closed, re-enable the editor window.
                    EnableWindow(hWndEditor, true);

                    // Activate the editor window again.
                    _editorShell.EditorWin.Activate();
                };

                // Close the dialog when the editor window is closed.
                _editorShell.EditorWin.Closed += (sender, e) =>
                {
                    dialogWin.Close();
                };

                //appWindow.Show();// TODO: not working. This Show() does not re-enable the editor window.
                dialogWin.Activate();
            }
        }

    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        /*
        if ((e.Parameter is Frame) && (e.Parameter != null))
        {
            ContentFrame = e.Parameter as Frame;
        }
        */

        if ((e.Parameter is Views.Rent.Residentials.Editor.EditorShell) && (e.Parameter != null))
        {
            _editorShell = e.Parameter as Views.Rent.Residentials.Editor.EditorShell;
            ViewModel = _editorShell?.ViewModel as ViewModels.Rent.Residentials.Editor.EditorViewModel;
        }

        base.OnNavigatedTo(e);
    }


    #region == TEMP code for modal window(for setting an owner) ==

    [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

    public const int GWL_HWNDPARENT = (-8);

    public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        if (IntPtr.Size == 4)
        {
            return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
        }
        return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
    }

    // Import the Windows API function SetWindowLong for modifying window properties on 32-bit systems.
    [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLong")]
    public static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    // Import the Windows API function SetWindowLongPtr for modifying window properties on 64-bit systems.
    [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtr")]
    public static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    #endregion
}
