using Microsoft.UI.Xaml;

namespace ZumenSearch.Views.Rent.Residentials;

public sealed partial class EditorWindow : Window
{
    public string? Id { get; private set; } = string.Empty;

    // TODO: do I need this?
    public bool IsAutoClose { get; set; }

    public ViewModels.Rent.Residentials.PropertyViewModel? ViewModel { get; private set; }

    //public EditorWindow(){  }

    public EditorWindow(string id, ViewModels.Rent.Residentials.PropertyViewModel vm) 
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentNullException(nameof(id));
        }

        Id = id;
        ViewModel = vm;

        InitializeComponent();

        ExtendsContentIntoTitleBar = true;

        //this.AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets\\App.ico"));
        this.AppWindow.SetIcon("Assets/App.ico");
    }

}
