using Microsoft.UI.Xaml;

namespace ZumenSearch.Views.Rent.Commercials.Listing;

public sealed partial class EditorWindow : Window
{
    public string Id { get; } = string.Empty;
    public string PropertyId { get; } = string.Empty;
    public ViewModels.Rent.Commercials.ListingViewModel ViewModel { get; }

    public EditorWindow(
        string id,
        string propertyId,
        ViewModels.Rent.Commercials.ListingViewModel viewModel)
    {
        Id = id;
        PropertyId = propertyId;
        ViewModel = viewModel;

        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        AppWindow.SetIcon("Assets/App.ico");
    }
}