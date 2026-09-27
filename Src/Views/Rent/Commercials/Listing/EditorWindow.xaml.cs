using Microsoft.UI.Xaml;

namespace ZumenSearch.Views.Rent.Commercials.Listing;

public sealed partial class EditorWindow : Window
{
    public string Id { get; }
    public ViewModels.Rent.Commercials.Listing.ListingViewModel ViewModel { get; }

    public EditorWindow(
        string id,
        ViewModels.Rent.Commercials.Listing.ListingViewModel viewModel)
    {
        Id = id;
        ViewModel = viewModel;

        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        AppWindow.SetIcon("Assets/App.ico");
    }
}