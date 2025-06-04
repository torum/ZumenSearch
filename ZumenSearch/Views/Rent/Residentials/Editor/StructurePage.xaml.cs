using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ZumenSearch.Models;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views.Rent.Residentials.Editor;

public sealed partial class StructurePage : Page
{
    public ViewModels.Rent.Residentials.Editor.StructureViewModel ViewModel
    {
        get;
    }

    private Views.Rent.Residentials.Editor.EditorShell? _editorShell;

    public StructurePage()
    {
        ViewModel = new ViewModels.Rent.Residentials.Editor.StructureViewModel();//App.GetService<RentLivingEditBuildingViewModel>();
        InitializeComponent();

        BreadcrumbBar1.ItemsSource = new ObservableCollection<Breadcrumbs>{
            new() { Name = "概要", Page = typeof(Views.Rent.Residentials.Editor.SummaryPage).FullName!},
            new() { Name = "構造", Page = typeof(Views.Rent.Residentials.Editor.StructurePage).FullName! },
        };
        BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;


        ViewModel.EventGoBack += (sender, arg) => OnEventGoBack(arg);
    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Index == 0)
        {
            //_editorShell?.NavFrame.Navigate(typeof(BuildingPage), _editorShell, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
            _editorShell?.NavFrame.GoBack();
        }
    }

    public void OnEventGoBack(string arg)
    {
        //_editorShell?.NavFrame.Navigate(typeof(Views.Rent.Residentials.Editor.BuildingPage), _editorShell, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        _editorShell?.NavFrame.GoBack();
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
