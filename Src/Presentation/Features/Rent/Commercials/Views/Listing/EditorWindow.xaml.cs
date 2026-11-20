using Microsoft.UI.Xaml;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Views.Listing;

public sealed partial class EditorWindow : Window
{
    public string Id { get; } = string.Empty;
    public string PropertyId { get; } = string.Empty;
    public ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel ViewModel { get; }

    public EditorWindow(
        string id,
        string propertyId,
        ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel viewModel)
    {
        Id = id;
        PropertyId = propertyId;
        ViewModel = viewModel;

        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        AppWindow.SetIcon("Assets/App.ico");
    }
}