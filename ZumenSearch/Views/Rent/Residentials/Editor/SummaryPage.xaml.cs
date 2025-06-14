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

namespace ZumenSearch.Views.Rent.Residentials.Editor;

public sealed partial class SummaryPage : Page
{
    //private Views.Rent.Residentials.Editor.EditorShell? _editorShell;

    private ViewModels.Rent.Residentials.Editor.EditorViewModel? _viewModel;
    public ViewModels.Rent.Residentials.Editor.EditorViewModel? ViewModel
    {
        get => _viewModel; 
        private set 
        {
            if (value != null)
            {
                _viewModel = value;

            }
        }
    }

    public enum RentLivingKinds
    {

        Unspecified, Apartment, Mansion, House, TerraceHouse, TownHouse, ShareHouse, Dormitory
    }

    public SummaryPage()
    {
        //Debug.WriteLine("Views.Rent.Residentials.Editor.SummaryPage init!");

        //ViewModel = new ViewModels.Rent.Residentials.Editor.SummaryViewModel();//App.GetService<RentLivingEditBuildingViewModel>();

        InitializeComponent();

        BreadcrumbBar1.ItemsSource = new ObservableCollection<Breadcrumb>{
            new() { Name = "概要", Page = typeof(Views.Rent.Residentials.Editor.SummaryPage).FullName! },
        };

        BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;
        /*
        ViewModel.EventEditStructure += (sender, arg) => OnEventEditStructure(arg);
        ViewModel.EventEditLocation += (sender, arg) => OnEventEditLocation(arg);
        ViewModel.EventEditTransportation += (sender, arg) => OnEventEditTransportation(arg);
        ViewModel.EventEditAppliance += (sender, arg) => OnEventEditAppliance(arg);
        */


    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Index == 0)
        {
            //_editorShell?.NavFrame.Navigate(typeof(Views.Rent.Residentials.Editor.SummaryPage), _editorShell, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.Editor.EditorViewModel) && (e.Parameter != null))
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.Editor.EditorShell;
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.Editor.EditorViewModel;
        }

        base.OnNavigatedTo(e);
    }

}
