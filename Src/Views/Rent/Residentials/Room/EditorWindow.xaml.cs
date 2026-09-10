using Microsoft.UI.Xaml;

namespace ZumenSearch.Views.Rent.Residentials.Room;

public sealed partial class EditorWindow : Window
{
    public string? Id { get; private set; } = string.Empty;

    public bool IsAutoClose { get; set; }

    public ViewModels.Rent.Residentials.Room.MainViewModel? ViewModel { get; private set; }

    public EditorWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        //AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/XmlClients.ico"));
        //Content = null;
        //Title = "AppDisplayName".GetLocalized();

    }

    public void SetListingIdToWindow(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentNullException(nameof(id));
        }

        Id = id;
    }

    public void SetViewModelToWindow(ViewModels.Rent.Residentials.Room.MainViewModel vm)
    {
        ViewModel = vm;
    }

}
