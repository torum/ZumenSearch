using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ZumenSearch.Services;
using Microsoft.UI.Windowing;
using ZumenSearch.ViewModels.Rent.Residentials.Editor;
using ZumenSearch.Views.Rent.Residentials.Editor;

namespace ZumenSearch.Services;

public class ModalDialogService : IModalDialogService
{
    public ModalDialogService()
    {
       
    }

    public void ShowUnitDialog(EditorViewModel editVM, EditorWindow editWin)
    {
        Debug.WriteLine("ModalDialogService: ShowUnitDialog method called.");

        if (editWin == null)
        {
            // EditorWin should be initialized in the EditorShell constructor.
            Debug.WriteLine("EditorWin should be initialized in the EditorShell constructor.");
            return;
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
        IntPtr hWndEditor = WinRT.Interop.WindowNative.GetWindowHandle(editWin);
        SetWindowLong(hWndDialog, GWL_HWNDPARENT, hWndEditor);

        Microsoft.UI.Windowing.AppWindow? appWindow = dialogWin.AppWindow;
        if (appWindow != null)
        {
            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsModal = true;

                // Don't use EnableWindow. This causes all sorts of problems. (as of WinAppSDK 1.7.25)
                //EnableWindow(hWndEditor, false);

                dialogWin.Closed += (sender, e) =>
                {
                    // Don't use EnableWindow. This causes all sorts of problems. (as of WinAppSDK 1.7.25)
                    //EnableWindow(hWndEditor, true);

                    // Activate the editor window again.
                    editWin.Activate();
                };

                // Close the dialog when the editor window is closed.
                editWin.Closed += (sender, e) =>
                {
                    dialogWin.Close();
                };

                //Task.Delay(30).ConfigureAwait(false);

                // Same as EnableWindow. This causes all sorts of problems. (as of WinAppSDK 1.7.25)
                //appWindow.Show(true);

                dialogWin.Activate();
                //dialogWin.Show();
            }
        }
    }


    public void Test()
    {
        Debug.WriteLine("ModalDialogService: Test method called.");
        /*
        var modalWindow = new Views.Modal.ModalWindow();

        var mainWindow = App.MainWindow;
        var hWndParent = WinRT.Interop.WindowNative.GetWindowHandle(mainWindow);
        var hWndDialog = WinRT.Interop.WindowNative.GetWindowHandle(modalWindow);
        SetWindowLong(hWndDialog, GWL_HWNDPARENT, hWndParent);

        var appWindow = modalWindow.AppWindow;
        if (appWindow != null)
        {
            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsModal = true;

                modalWindow.Closed += (sender, e) =>
                {
                    EnableWindow(hWndParent, true);
                };

                if (mainWindow != null)
                {
                    mainWindow.Closed += (sender, e) =>
                    {
                        modalWindow.Close();
                    };
                }

                EnableWindow(hWndParent, false);

                //modalWindow.Show();// not working. This Show() does not re-enable the editor window.
                modalWindow.Activate();
            }
        }
        */
    }


    #region == TEMP code for modal window(for setting an owner) ==

#pragma warning disable IDE0079
#pragma warning disable SYSLIB1054

    [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]

    internal static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

    internal const int GWL_HWNDPARENT = (-8);

    internal static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        if (IntPtr.Size == 4)
        {
            return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
        }
        return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
    }

    // Import the Windows API function SetWindowLong for modifying window properties on 32-bit systems.
    [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLong")]
    internal static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    // Import the Windows API function SetWindowLongPtr for modifying window properties on 64-bit systems.
    [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtr")]
    internal static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

#pragma warning restore SYSLIB1054
#pragma warning restore IDE0079

    #endregion
}
