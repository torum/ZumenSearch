using Microsoft.UI.Xaml;
using System.Diagnostics;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;
using Windows.UI.Core;
using ZumenSearch.ViewModels.RentLivingEdit;

namespace ZumenSearch.Views.RentLivingEdit;

public sealed partial class RentLivingEditUnitListPage : Page
{
    public RentLivingEditUnitListViewModel ViewModel
    {
        get;
    }

    private Frame? ContentFrame;

    public RentLivingEditUnitListPage()
    {
        ViewModel = new RentLivingEditUnitListViewModel();//App.GetService<RentLivingEditUnitShellViewModel>();
        InitializeComponent();

        BreadcrumbBarRoom.ItemsSource = new string[] { "一覧" };

        ViewModel.eventAddNew += (sender, arg) => OnEventAddNew(arg);
    }

    public void OnEventAddNew(string arg)
    {
        // TODO: arg is temp.

        if (ContentFrame == null)
            return;

        // 
        ContentFrame.Navigate(typeof(RentLivingEdit.RentLivingEditUnitEditShellPage), ContentFrame, new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());
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
