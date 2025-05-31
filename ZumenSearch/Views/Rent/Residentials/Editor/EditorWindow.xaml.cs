
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.UI.WindowManagement;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views.Rent.Residentials.Editor;

public sealed partial class EditorWindow : Window
{
    private readonly MainViewModel MainVM = App.GetService<MainViewModel>();

    // Flag that tellls if this window is being closed automatically by closing the MainWindow.
    public bool IsAutoClose { get; set; } = false;

    public EditorWindow()
    {
        InitializeComponent();

        //AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/XmlClients.ico"));
        //Content = null;
        //Title = "AppDisplayName".GetLocalized();

    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        SaveWindowSizeAndPosition();

        // Remove this window from the list of open editor windows if not Main window is closing.
        if (!IsAutoClose)
        {
            // Removes this window from the list of open editor windows. 
            MainVM.EditorList.Remove(this);
        }
    }

    private void SaveWindowSizeAndPosition()
    {
        // Save window size and position.
        Microsoft.UI.Windowing.AppWindow? appWindow = this.AppWindow;
        if (appWindow != null)
        {
            if (appWindow.Presenter is OverlappedPresenter)
            {
                MainVM.EditorWinHeight = (int)appWindow.Size.Height;
                MainVM.EditorWinWidth = (int)appWindow.Size.Width;
                MainVM.EditorWinTop = (int)appWindow.Position.Y;
                MainVM.EditorWinLeft = (int)appWindow.Position.X;
            }
        }
        else
        {
            //Debug.WriteLine("appWindow is null");
        }
    }
}
