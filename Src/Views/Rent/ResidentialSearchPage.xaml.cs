using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using ZumenSearch.Models;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views.Rent;

internal sealed partial class ResidentialSearchPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    private Frame? ContentFrame;

    public ResidentialSearchPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();

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
