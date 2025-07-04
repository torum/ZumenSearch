﻿using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using Windows.Storage;
using Windows.Storage.Pickers;
using ZumenSearch.Models;
using ZumenSearch.ViewModels.Rent.Residentials.Editor;

namespace ZumenSearch.Views.Rent.Residentials.Editor;

public sealed partial class PictureListPage : Page
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

    public PictureListPage()
    {
        InitializeComponent();

        //ViewModel = new PictureListViewModel();//App.GetService<RentLivingEditPictureShellViewModel>();
        BreadcrumbBar1.ItemsSource = new ObservableCollection<Breadcrumb>{
            new() { Name = "写真", Page = typeof(Views.Rent.Residentials.Editor.PictureListPage).FullName! },
        };
        BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;
    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Index == 0)
        {
            //_viewModel?.GoBackToSummary();
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

    private void ItemsView_SelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs args)
    {
        if (ViewModel != null)
        {
            ViewModel.SelectedBuildingPicture = sender.SelectedItem as Models.Rent.Residentials.PictureBuilding;
        }
    }
}
