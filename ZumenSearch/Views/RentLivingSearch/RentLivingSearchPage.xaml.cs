using Microsoft.UI.Xaml.Controls;

using ZumenSearch.ViewModels;

namespace ZumenSearch.Views;

public sealed partial class RentLivingSearchPage : Page
{
    public RentLivingSearchViewModel ViewModel
    {
        get;
    }

    public RentLivingSearchPage()
    {
        ViewModel = App.GetService<RentLivingSearchViewModel>();
        InitializeComponent();

        BreadcrumbBar1.ItemsSource = new string[] { "条件検索"};
    }
}
