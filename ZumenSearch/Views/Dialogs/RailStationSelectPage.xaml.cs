using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using ZumenSearch.ViewModels.Transportation;

namespace ZumenSearch.Views.Dialogs;

public sealed partial class RailStationSelectPage : Page
{
    public RailStationSelectViewModel ViewModel
    {
        get;
    }

    public RailStationSelectPage(RailStationSelectViewModel vm)
    {
        InitializeComponent();

        ViewModel = vm;
        //ViewModel = new RailStationSelectViewModel();
    }
}
