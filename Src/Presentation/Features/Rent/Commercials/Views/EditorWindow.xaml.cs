using Microsoft.UI.Xaml;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Views;

public sealed partial class EditorWindow : Window
{
    public string? Id { get; private set; } = string.Empty;

    public ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyViewModel? ViewModel { get; private set; }

    //public EditorWindow(){  }

    public EditorWindow(string id, ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyViewModel vm)
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
