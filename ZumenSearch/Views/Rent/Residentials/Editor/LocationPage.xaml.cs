﻿using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ZumenSearch.Models;

namespace ZumenSearch.Views.Rent.Residentials.Editor;

public sealed partial class LocationPage : Page
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

                //_viewModel.EventBackToSummary += (sender, arg) => OnEventBackToSummary(arg);
            }
        }
    }

    public LocationPage()
    {
        //ViewModel = new LocationViewModel();//App.GetService<RentLivingEditLocationViewModel>();
        InitializeComponent();

        BreadcrumbBar1.ItemsSource = new ObservableCollection<Breadcrumb>{
            new() { Name = "概要", Page = typeof(Views.Rent.Residentials.Editor.SummaryPage).FullName!},
            new() { Name = "所在地", Page = typeof(Views.Rent.Residentials.Editor.LocationPage).FullName! },
        };
        BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;

    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Index == 0)
        {
            //_editorShell?.NavFrame.Navigate(typeof(Views.Rent.Residentials.Editor.SummaryPage), _editorShell, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            ViewModel?.GoBackToSummary();
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.Editor.EditorViewModel) && (e.Parameter != null))
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.Editor.EditorShell;
            //ViewModel = _editorShell?.ViewModel as ViewModels.Rent.Residentials.Editor.EditorViewModel;
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.Editor.EditorViewModel;
        }

        base.OnNavigatedTo(e);
    }
}
