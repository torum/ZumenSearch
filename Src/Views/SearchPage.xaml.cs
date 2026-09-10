using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using ZumenSearch.Services.Contracts;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views;

public class CustomDataObject
{
    public string? Title
    {
        get; set;
    }
    public string? ImageLocation
    {
        get; set;
    }
    public string? Views
    {
        get; set;
    }
    public string? Likes
    {
        get; set;
    }
    public string? Description
    {
        get; set;
    }

    public CustomDataObject()
    {
    }


    // ... Methods ...
}

public sealed partial class SearchPage : Page
{
    public ViewModels.MainViewModel ViewModel { get;}

    private readonly INavigationService _navigationService;

    public SearchPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        _navigationService = App.GetService<INavigationService>();

        InitializeComponent();

        this.Loaded += Page_Loaded;

        //BreadcrumbBarMain.ItemsSource = new string[] { "ëççáåüçı" };
        /*
        BreadcrumbBarMain.ItemsSource = new ObservableCollection<Breadcrumb>{
        new() { Name = "ëççáåüçı", Page = typeof(IntegratedSearchPage).FullName!},
    };
        */

        var Items = new ObservableCollection<CustomDataObject>();

        var temp = new CustomDataObject
        {
            Title = "test"
        };

        Items.Add(temp);



        Items.Add(temp);
        Items.Add(temp);
        Items.Add(temp);
        Items.Add(temp);
        Items.Add(temp);
        Items.Add(temp);
        Items.Add(temp);
        Items.Add(temp);
        Items.Add(temp);
        Items.Add(temp);
        Items.Add(temp);
        Items.Add(temp);
        BasicGridView.ItemsSource = Items;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        this.SearchAutoSuggestBox.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
    }

    private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (ViewModel.SearchRentResidentialBldgCommand.CanExecute(args.QueryText))
        {
            ViewModel.SearchRentResidentialBldgCommand.Execute(args.QueryText);
        }
    }


}
