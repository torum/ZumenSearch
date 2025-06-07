using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ZumenSearch.Models;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views.Rent.Residentials;

public sealed partial class SearchPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    private Frame? ContentFrame;

    public SearchPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();

        //BreadcrumbBar1.ItemsSource = new string[] { "条件検索","asdf"};
        BreadcrumbBar1.ItemsSource = new ObservableCollection<Breadcrumb>{
            //new() { Name = "賃貸", Page = typeof(RentSearchViewModel).FullName!},
            new() { Name = "住居用", Page = typeof(Views.Rent.Residentials.SearchPage).FullName! },
        };
        BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;
    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Index == 0)
        {/*
            MainShell shell = App.GetService<MainShell>();
            shell.NavFrame.Navigate(typeof(RentSearchPage), shell.NavFrame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            */
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
