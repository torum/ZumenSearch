using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ZumenSearch.Models;
using ZumenSearch.ViewModels.Rent.Residentials.Editor;

namespace ZumenSearch.Views.Rent.Residentials.Editor;

public sealed partial class BuildingPage : Page
{
    public BuildingViewModel ViewModel
    {
        get;
    }

    private Views.Rent.Residentials.Editor.EditorShell? _editorShell;

    public BuildingPage()
    {
        ViewModel = new BuildingViewModel();//App.GetService<RentLivingEditBuildingViewModel>();
        InitializeComponent();

        BreadcrumbBar1.ItemsSource = new ObservableCollection<Breadcrumbs>{
            new() { Name = "建物", Page = typeof(BuildingPage).FullName!},
            //new() { Name = "住居用", Page = typeof(ResidentialsPage).FullName! },
        };
        BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;
    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Index == 0)
        {
            _editorShell?.NavFrame.Navigate(typeof(BuildingPage), _editorShell, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }
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
