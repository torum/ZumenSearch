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
using ZumenSearch.ViewModels;
using ZumenSearch.ViewModels.Transportation;

namespace ZumenSearch.Views.Dialogs;

public sealed partial class RailLineSelectPage : Page
{
    /*
    public ListView SelectionList
    {
        get;set;
    }
    */

    public RailLineSelectViewModel ViewModel
    {
        get;
    }

    public RailLineSelectPage(RailLineSelectViewModel vm)
    {
        InitializeComponent();

        ViewModel = vm;

        //SelectionList = this.SelectionListView;
    }

}
