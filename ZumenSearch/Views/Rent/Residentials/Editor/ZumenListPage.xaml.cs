﻿using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using ZumenSearch.ViewModels.Rent.Residentials.Editor;

namespace ZumenSearch.Views.Rent.Residentials.Editor;

public sealed partial class ZumenListPage : Page
{
    private Views.Rent.Residentials.Editor.EditorShell? _editorShell;

    private ViewModels.Rent.Residentials.Editor.EditorViewModel? _viewModel;
    public ViewModels.Rent.Residentials.Editor.EditorViewModel? ViewModel
    {
        get => _viewModel;
        private set
        {
            if (_editorShell != null)
            {
                _viewModel = value;
                if (_viewModel != null)
                {
                    //
                }
                else
                {
                    Debug.WriteLine("Views.Rent.Residentials.Editor.ZumenListPage ViewModel is null!");
                }
            }
            else
            {
                Debug.WriteLine("Views.Rent.Residentials.Editor.ZumenListPage _editorShell is null!");
            }
        }
    }

    public ZumenListPage()
    {
        //ViewModel = new ZumenListViewModel();//App.GetService<RentLivingEditZumenViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is Views.Rent.Residentials.Editor.EditorShell) && (e.Parameter != null))
        {
            _editorShell = e.Parameter as Views.Rent.Residentials.Editor.EditorShell;
            ViewModel = _editorShell?.ViewModel as ViewModels.Rent.Residentials.Editor.EditorViewModel;
        }

        base.OnNavigatedTo(e);
    }
}
