using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using ZumenSearch.Models;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views.Rent.Residentials.Editor.Modal;

public sealed partial class SummaryPage : Page
{
    private ViewModels.Rent.Residentials.Editor.Modal.ModalViewModel? _viewModel;
    public ViewModels.Rent.Residentials.Editor.Modal.ModalViewModel? ViewModel
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

    public SummaryPage()
    {
        //ViewModel = new ViewModels.Rent.Residentials.Editor.Modal.SummaryViewModel();
        InitializeComponent();

        BreadcrumbBar1.ItemsSource = new ObservableCollection<Breadcrumb>{
            new() { Name = "äTóv", Page = typeof(SummaryPage).FullName!},
            //new() { Name = "èZãèóp", Page = typeof(ResidentialsPage).FullName! },
        };
        BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;
    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Index == 0)
        {
            _viewModel?.GoBackToSummary();
            //_dialogShell?.NavFrame.Navigate(typeof(SummaryPage), _dialogShell, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is ViewModels.Rent.Residentials.Editor.Modal.ModalViewModel) && (e.Parameter != null))
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.Editor.EditorShell;
            ViewModel = e.Parameter as ViewModels.Rent.Residentials.Editor.Modal.ModalViewModel;
        }

        base.OnNavigatedTo(e);
    }
}
