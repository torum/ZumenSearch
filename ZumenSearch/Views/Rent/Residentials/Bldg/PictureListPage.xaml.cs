using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using Windows.Storage;
using Windows.Storage.Pickers;
using ZumenSearch.Models;
using ZumenSearch.ViewModels.Rent.Residentials;

namespace ZumenSearch.Views.Rent.Residentials.Bldg;

public sealed partial class PictureListPage : Page
{
    //private Views.Rent.Residentials.Editor.EditorShell? _editorShell;

    private ViewModels.Rent.Residentials.ResidentialsViewModel? _viewModel;
    public ViewModels.Rent.Residentials.ResidentialsViewModel? ViewModel
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

    }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.ResidentialsViewModel) && (e.Parameter != null))
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.Editor.EditorShell;
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.ResidentialsViewModel;
        }

        base.OnNavigatedTo(e);
    }

    private void ItemsView_SelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs args)
    {
        if (ViewModel is null)
        {
            return;
        }

        if (sender.SelectedItem is not Models.Rent.Residentials.PictureBuilding picbldg)
        {
            return;
        }

        Debug.WriteLine($"PictureListPage ItemsView_SelectionChanged SelectedItem Changed to {picbldg.PictureType.Label}");

        ViewModel.SelectedBuildingPicture = picbldg;//sender.SelectedItem as Models.Rent.Residentials.PictureBuilding;
    }
}
