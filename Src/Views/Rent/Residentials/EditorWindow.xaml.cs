using Microsoft.UI.Xaml;

namespace ZumenSearch.Views.Rent.Residentials;

public sealed partial class EditorWindow : Window
{
    public string Id { get; private set; } = string.Empty;

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

}
