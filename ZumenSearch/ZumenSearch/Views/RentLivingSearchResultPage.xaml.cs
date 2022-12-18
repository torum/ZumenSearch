using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.UI.Xaml.Controls;

using ZumenSearch.ViewModels;

namespace ZumenSearch.Views;

// [BreadcrumbBar] Temp
public class Folder
{
    public string? Name
    {
        get; set;
    }

    public string? Page
    {
        get; set;
    }
}

public sealed partial class RentLivingSearchResultPage : Page
{
    public RentLivingSearchResultViewModel ViewModel
    {
        get;
    }

    public RentLivingSearchResultPage()
    {
        ViewModel = App.GetService<RentLivingSearchResultViewModel>();
        InitializeComponent();

        // BreadcrumbBar1.ItemsSource = new string[] { "条件検索", "検索結果一覧" };
        BreadcrumbBar1.ItemsSource = new ObservableCollection<Folder>{
            new Folder { Name = "条件検索", Page = typeof(RentLivingSearchViewModel).FullName!},
            new Folder { Name = "検索結果一覧", Page = typeof(RentLivingSearchResultViewModel).FullName! },
        };

        BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;
    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (BreadcrumbBar1.ItemsSource is not ObservableCollection<Folder> items) return;

        if (args.Index == 0)
        {
            ViewModel.NavigationService.NavigateTo(items[args.Index].Page);
            Debug.WriteLine(items[args.Index].Page);
        }
    }


}
