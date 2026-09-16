using Microsoft.UI.Xaml;

namespace ZumenSearch.Views.Rent.Residentials.Listing;

public sealed partial class EditorWindow : Window
{
    public string? Id { get; private set; } = string.Empty;

    public bool IsAutoClose { get; set; }

    public ViewModels.Rent.Residentials.Listing.ListingViewModel? ViewModel { get; private set; }

    public EditorWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;

        //this.AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets\\App.ico"));
        this.AppWindow.SetIcon("Assets/App.ico");
    }

    public void SetListingIdToWindow(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentNullException(nameof(id));
        }

        Id = id;
    }

    public void SetViewModelToWindow(ViewModels.Rent.Residentials.Listing.ListingViewModel vm)
    {
        ViewModel = vm;
    }

}
