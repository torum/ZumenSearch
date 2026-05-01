using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ZumenSearch.Models;
using ZumenSearch.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Xml.Linq;
using ZumenSearch.Services;
using ZumenSearch.Views;

namespace ZumenSearch.Views.Rent.Residentials.Bldg;

public sealed partial class BasicPage : Page
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

            }
        }
    }

    public enum RentLivingKinds
    {

        Unspecified, Apartment, Mansion, House, TerraceHouse, TownHouse, ShareHouse, Dormitory
    }

    public BasicPage()
    {
        //Debug.WriteLine("Views.Rent.Residentials.Editor.BasicPage init!");

        //ViewModel = new ViewModels.Rent.Residentials.Editor.SummaryViewModel();//App.GetService<RentLivingEditBuildingViewModel>();

        InitializeComponent();

        /*
        ViewModel.EventEditStructure += (sender, arg) => OnEventEditStructure(arg);
        ViewModel.EventEditLocation += (sender, arg) => OnEventEditLocation(arg);
        ViewModel.EventEditTransportation += (sender, arg) => OnEventEditTransportation(arg);
        ViewModel.EventEditAppliance += (sender, arg) => OnEventEditAppliance(arg);
        */


    }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.ResidentialsViewModel) && (e.Parameter != null))
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.Editor.EditorShell;
            ViewModel = e.Parameter as ViewModels.Rent.ResidentialsViewModel;
        }

        base.OnNavigatedTo(e);
    }

}
