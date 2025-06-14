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

namespace ZumenSearch.Views.Rent.Residentials.Editor;

public sealed partial class UnitListPage : Page
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
                //_viewModel.EventAddNew += (sender, arg) => OnEventAddNew(arg);
            }
        }
    }

    public UnitListPage()
    {
        //ViewModel = new ViewModels.Rent.Residentials.Editor.UnitListViewModel();//App.GetService<RentLivingEditUnitShellViewModel>();

        InitializeComponent();

        BreadcrumbBar1.ItemsSource = new ObservableCollection<Breadcrumb>{
            new() { Name = "部屋", Page = typeof(ZumenSearch.Views.Rent.Residentials.Editor.UnitListPage).FullName! },
        };
        BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;

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
