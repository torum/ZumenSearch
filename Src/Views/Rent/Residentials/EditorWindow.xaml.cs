using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace ZumenSearch.Views.Rent.Residentials;

internal sealed partial class EditorWindow : Window
{
    public string? Id { get; private set; } = string.Empty;

    public bool IsAutoClose { get; set; }

    public ViewModels.Rent.Residentials.MainViewModel? ViewModel { get; private set; }

    public EditorWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        //AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/XmlClients.ico"));
        //Content = null;
        //Title = "AppDisplayName".GetLocalized();

    }

    public void SetEntryIdToWindow(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentNullException(nameof(id));
        }

        Id = id;
    }

    public void SetViewModelToWindow(ViewModels.Rent.Residentials.MainViewModel vm)
    {
        ViewModel = vm;
    }

}
