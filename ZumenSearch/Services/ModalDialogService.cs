using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ZumenSearch.Services;
using Microsoft.UI.Windowing;

namespace ZumenSearch.Services;

public class ModalDialogService : IModalDialogService
{
    public ModalDialogService()
    {
       
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
