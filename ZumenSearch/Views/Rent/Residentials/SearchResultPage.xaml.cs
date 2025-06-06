using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ZumenSearch.Models;
using ZumenSearch.ViewModels;
using ZumenSearch.Views;

namespace ZumenSearch.Views.Rent.Residentials;

public sealed partial class SearchResultPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    //private MainShell? Shell => App.GetService<MainShell>();

    private Frame? ContentFrame;

    public SearchResultPage()
    {
        ViewModel = App.GetService<MainViewModel>();

        InitializeComponent();

        BreadcrumbBar1.ItemsSource = new ObservableCollection<Breadcrumbs>{
            //new() { Name = "賃貸", Page = typeof(Views.Rent.RentSearchPage).FullName!},
            new() { Name = "住居用", Page = typeof(Views.Rent.Residentials.SearchPage).FullName! },
            new() { Name = "検索結果", Page = typeof(Views.Rent.Residentials.SearchResultPage).FullName! },
        };
        BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;
    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        //ViewModel.NavigationService.NavigateTo(items[args.Index].Page!);
        
        //MainShell shell = App.GetService<MainShell>();

        if (ContentFrame is null) return;

        if (args.Index == 0)
        {
            ContentFrame.Navigate(typeof(Views.Rent.Residentials.SearchPage), ContentFrame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }
        else if ( args.Index == 1)
        {
            //shell.NavFrame.Navigate(typeof(Views.Rent.Residentials.SearchPage), shell.NavFrame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {

        if ((e.Parameter is Frame) && (e.Parameter != null))
        {
            ContentFrame = e.Parameter as Frame;
        }

        base.OnNavigatedTo(e);
    }
}
