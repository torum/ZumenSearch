using Microsoft.UI.Xaml;

namespace ZumenSearch.Views.Rent.Residentials.Listing;

public sealed partial class EditorWindow : Window
{
    public string? Id { get; private set; } = string.Empty;

    public ViewModels.Rent.Residentials.Listing.ListingViewModel? ViewModel { get; private set; }

    public EditorWindow(string id, ViewModels.Rent.Residentials.Listing.ListingViewModel vm)
    {
        Id = id;
        ViewModel = vm;

        InitializeComponent();

        ExtendsContentIntoTitleBar = true;

        //this.AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets\\App.ico"));
        this.AppWindow.SetIcon("Assets/App.ico");
    }

}
