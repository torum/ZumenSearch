using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.UI.WindowManagement;

namespace ZumenSearch.Views.Rent.Residentials.Editor.Modal;  

public sealed partial class ModalWindow : Window
{
    public ModalWindow()
    {
        this.InitializeComponent();
        this.Title = "Editor Dialog Window";
        //ExtendsContentIntoTitleBar = true;

        // TEMP: Resize the window to a specific size.
        this.AppWindow.Resize(new Windows.Graphics.SizeInt32(1024, 768));
        if (this.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
        }

        // Center the window on the screen.
        CenterWindow();

    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        //
    }

    private void CenterWindow()
    {
        var area = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest)?.WorkArea;
        if (area == null) return;
        this.AppWindow.Move(new PointInt32((area.Value.Width - AppWindow.Size.Width) / 2, (area.Value.Height - AppWindow.Size.Height) / 2));
    }
}
