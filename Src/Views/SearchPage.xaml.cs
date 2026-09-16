using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using ZumenSearch.Services.Contracts;
using ZumenSearch.ViewModels;
using System.Diagnostics;

namespace ZumenSearch.Views;

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

    private void BasicGridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not Models.Common.PropertySearchResultItem item)
        {
            return;
        }

        if (item.PropertyKind == Models.Base.EnumPropertyKind.RentResidential)
        {
            if (ViewModel.EditRentResidentialBldgCommand.CanExecute(item))
            {
                ViewModel.EditRentResidentialBldgCommand.Execute(item);
            }
        }
        else
        {
            // TODO: RentResidentialBldg only for now.
            Debug.WriteLine($"EnumPropertyKind is not RentResidential @BasicGridView_ItemClick {item.PropertyKind}");
        }
    }

    private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
    {
        if (sender is Microsoft.UI.Xaml.Controls.Image img)
        {
            System.Diagnostics.Debug.WriteLine($"Image failed to load: {img.Source}"); 
            /*
            img.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(
                new Uri("ms-appx:///Assets/FallbackPlaceholder.png")
            );
            */
        }
    }
}
