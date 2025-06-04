using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using ZumenSearch.Models;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views.Rent.Residentials.Editor.Modal;

public sealed partial class SummaryPage : Page
{
    public ViewModels.Rent.Residentials.Editor.Modal.SummaryViewModel ViewModel
    {
        get;
    }

    private Views.Rent.Residentials.Editor.Modal.ModalShell? _dialogShell;

    public SummaryPage()
    {
        ViewModel = new ViewModels.Rent.Residentials.Editor.Modal.SummaryViewModel();
        InitializeComponent();

        BreadcrumbBar1.ItemsSource = new ObservableCollection<Breadcrumbs>{
            new() { Name = "äTóv", Page = typeof(SummaryPage).FullName!},
            //new() { Name = "èZãèóp", Page = typeof(ResidentialsPage).FullName! },
        };
        BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;
    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Index == 0)
        {
            //_dialogShell?.NavFrame.Navigate(typeof(SummaryPage), _dialogShell, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {

        if ((e.Parameter is Views.Rent.Residentials.Editor.Modal.ModalShell) && (e.Parameter != null))
        {
            _dialogShell = e.Parameter as Views.Rent.Residentials.Editor.Modal.ModalShell;
        }

        base.OnNavigatedTo(e);
    }
}
