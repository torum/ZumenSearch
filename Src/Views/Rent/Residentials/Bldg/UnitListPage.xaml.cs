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

namespace ZumenSearch.Views.Rent.Residentials.Bldg;

public sealed partial class UnitListPage : Page
{
    //private Views.Rent.Residentials.Editor.EditorShell? _editorShell;

    public ViewModels.Rent.ResidentialsViewModel? ViewModel
    {
        get;
        private set
        {
            if (value != null)
            {
                field = value;
                //_viewModel.EventAddNew += (sender, arg) => OnEventAddNew(arg);
            }
        }
    }

    public UnitListPage()
    {
        //ViewModel = new ViewModels.Rent.Residentials.Editor.UnitListViewModel();//App.GetService<RentLivingEditUnitShellViewModel>();

        InitializeComponent();

    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.ResidentialsViewModel) && (e.Parameter != null))
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.EditorShell;
            ViewModel = e.Parameter as ViewModels.Rent.ResidentialsViewModel;
        }
        else
        {
            Debug.WriteLine("UnitListPage.OnNavigatedTo: Invalid parameter. Expected ResidentialsViewModel.");
        }

        base.OnNavigatedTo(e);
    }

}
