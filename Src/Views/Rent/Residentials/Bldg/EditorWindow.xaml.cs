using Microsoft.UI.Xaml;

namespace ZumenSearch.Views.Rent.Residentials.Bldg;

public sealed partial class EditorWindow : Window
{
    public string? Id { get; private set; } = string.Empty;

    // TODO: do I need this?
    public bool IsAutoClose { get; set; }

    public ViewModels.Rent.Residentials.Bldg.PropertyViewModel? ViewModel { get; private set; }

    public EditorWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        //AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/XmlClients.ico"));
        //Content = null;
        //Title = "AppDisplayName".GetLocalized();

    }

    public void SetPropertyIdToWindow(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentNullException(nameof(id));
        }

        Id = id;
    }

    public void SetViewModelToWindow(ViewModels.Rent.Residentials.Bldg.PropertyViewModel vm)
    {
        ViewModel = vm;
    }

}
