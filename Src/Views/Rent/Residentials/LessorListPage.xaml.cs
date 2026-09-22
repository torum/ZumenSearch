using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.ApplicationModel.Chat;
using Windows.Graphics;
using WinRT.Interop;
using ZumenSearch.Views.Dialogs;

namespace ZumenSearch.Views.Rent.Residentials;

public partial class PersonTemplateSelector : DataTemplateSelector
{
    public DataTemplate? NaturalTemplate { get; set; }
    public DataTemplate? LegalTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        if (item is Models.Rent.Lessors.PersonWrapperForPropertyViewModel lessor)
        {
            if (lessor.Person is Models.PersonNatural)
            {
                return NaturalTemplate;
            }
            else if (lessor.Person is Models.PersonLegal)
            {
                return LegalTemplate;
            }
        }
        else if (item is Models.PersonNatural)
        {
            return NaturalTemplate;
        }
        else if (item is Models.PersonLegal)
        {
            return LegalTemplate;
        }

        return base.SelectTemplateCore(item);
    }

    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
    {
        return SelectTemplateCore(item);
    }
}

public sealed partial class LessorListPage : Page
{
    public ViewModels.Rent.Residentials.PropertyViewModel? ViewModel { get; private set; }


    public LessorListPage()
    {
        //ViewModel = new KasinusiViewModel();//App.GetService<RentLivingEditZumenViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.PropertyViewModel) && (e.Parameter != null))
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.Editor.EditorShell;
            //ViewModel = _editorShell?.ViewModel as ViewModels.Rent.Residentials.Editor.EditorViewModel;
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.PropertyViewModel;
        }

        base.OnNavigatedTo(e);
    }

    private void AppBarButtonSelectLessor_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        /*
        if (ViewModel is null)
        {
            return;
        }

        if (sender is not AppBarButton button)
        {
            return;
        }

        button.IsEnabled = false;

        var modalWindow = new Views.Dialogs.SelectLessorWindow();
        //OverlappedPresenter? presenter = modalWindow.AppWindow.Presenter as OverlappedPresenter;
        OverlappedPresenter presenter = OverlappedPresenter.CreateForDialog();
        presenter.IsModal = true;
        presenter.IsResizable = true;

        var thisWindowId = this.XamlRoot.ContentIslandEnvironment.AppWindowId;
        IntPtr thisHWnd = Microsoft.UI.Win32Interop.GetWindowFromWindowId(thisWindowId);
        SetWindowOwner(ownerHwnd: thisHWnd, id: modalWindow.AppWindow.Id);

        modalWindow.AppWindow.SetPresenter(presenter);

        modalWindow.AppWindow.Resize(new Windows.Graphics.SizeInt32(600, 400));
        var thisAppWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(thisWindowId);

        CenterWindow(modalWindow, thisAppWindow);

        modalWindow.AppWindow.Show(true);

        modalWindow.Closed += ModalWindow_Closed;

        button.IsEnabled = true;
        */
    }

    private void ModalWindow_Closed(object sender, WindowEventArgs args)
    {
        // Reactivate the owner window when the modal window closes.
        // TODO: not gonna work without getting actual "Window".

        /*
        var thisWindowId = this.XamlRoot.ContentIslandEnvironment.AppWindowId;
        var thisAppWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(thisWindowId);
        thisAppWindow.MoveInZOrderAtTop();
        */

        /*
        if (sender is Window modalWindow)
        {
            var thisAppWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(modalWindow.AppWindow.OwnerWindowId);
            thisAppWindow.MoveInZOrderAtTop();
        }
        */
    }


    private static void CenterWindow(Window window, AppWindow ownerAppWindow)
    {
        //
        PointInt32 CenteredPosition = ownerAppWindow.Position;
        CenteredPosition.X += (ownerAppWindow.Size.Width - window.AppWindow.Size.Width) / 2;
        CenteredPosition.Y += (ownerAppWindow.Size.Height - window.AppWindow.Size.Height) / 2;
        window.AppWindow.Move(CenteredPosition);
    }

    private void SetWindowOwner(IntPtr ownerHwnd, WindowId id)
    {
        // Get the HWND (window handle) of the owner window (main window).
        //IntPtr ownerHwnd = WindowNative.GetWindowHandle(owner);

        // Get the HWND of the AppWindow (modal window).
        IntPtr ownedHwnd = Win32Interop.GetWindowFromWindowId(id);

        // Set the owner window using SetWindowLongPtr for 64-bit systems
        // or SetWindowLong for 32-bit systems.
        if (IntPtr.Size == 8) // Check if the system is 64-bit
        {
            SetWindowLongPtr(ownedHwnd, -8, ownerHwnd); // -8 = GWLP_HWNDPARENT
        }
        else // 32-bit system
        {
            SetWindowLong(ownedHwnd, -8, ownerHwnd); // -8 = GWL_HWNDPARENT
        }
    }

    // Import the Windows API function SetWindowLongPtr for modifying window properties on 64-bit systems.
    [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtr")]
    public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    // Import the Windows API function SetWindowLong for modifying window properties on 32-bit systems.
    [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLong")]
    public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

}
