using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ZumenSearch.Models;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views.Rent.Residentials.Editor;

public sealed partial class SummaryPage : Page
{
    public ViewModels.Rent.Residentials.Editor.SummaryViewModel ViewModel
    {
        get;
    }

    private Views.Rent.Residentials.Editor.EditorShell? _editorShell;

    public SummaryPage()
    {
        ViewModel = new ViewModels.Rent.Residentials.Editor.SummaryViewModel();//App.GetService<RentLivingEditBuildingViewModel>();
        InitializeComponent();

        BreadcrumbBar1.ItemsSource = new ObservableCollection<Breadcrumbs>{
            new() { Name = "概要", Page = typeof(Views.Rent.Residentials.Editor.SummaryPage).FullName! },
        };
        BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;

        ViewModel.EventEditStructure += (sender, arg) => OnEventEditStructure(arg);
        ViewModel.EventEditLocation += (sender, arg) => OnEventEditLocation(arg);
        ViewModel.EventEditTransportation += (sender, arg) => OnEventEditTransportation(arg);
        ViewModel.EventEditAppliance += (sender, arg) => OnEventEditAppliance(arg);
    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Index == 0)
        {
            //_editorShell?.NavFrame.Navigate(typeof(Views.Rent.Residentials.Editor.SummaryPage), _editorShell, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }
    }

    public void OnEventEditStructure(string arg)
    {
        _editorShell?.NavFrame.Navigate(typeof(Views.Rent.Residentials.Editor.StructurePage), _editorShell, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
    }

    public void OnEventEditLocation(string arg)
    {
        _editorShell?.NavFrame.Navigate(typeof(Views.Rent.Residentials.Editor.LocationPage), _editorShell, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
    }

    public void OnEventEditTransportation(string arg)
    {
        _editorShell?.NavFrame.Navigate(typeof(Views.Rent.Residentials.Editor.TransportationPage), _editorShell, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
    }

    public void OnEventEditAppliance(string arg)
    {
        _editorShell?.NavFrame.Navigate(typeof(Views.Rent.Residentials.Editor.AppliancePage), _editorShell, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
    }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        /*
        if (e.Parameter is string && !string.IsNullOrWhiteSpace((string)e.Parameter))
        {
            Debug.WriteLine("------------------------" + e.Parameter.ToString());
        }
        else if (e.Parameter is Frame)
        {
            Debug.WriteLine("========================");
        }
        else
        {

            Debug.WriteLine("------------------------"+e.Content);
        }
        */
        if ((e.Parameter is Views.Rent.Residentials.Editor.EditorShell) && (e.Parameter != null))
        {
            _editorShell = e.Parameter as Views.Rent.Residentials.Editor.EditorShell;
        }

        base.OnNavigatedTo(e);
    }

}
