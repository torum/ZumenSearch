using Microsoft.UI.Xaml;

namespace ZumenSearch.Presentation.Features.Rent.Lessors.Views;

public sealed partial class EditorWindow : Window
{
    public string? Id { get; private set; } = string.Empty;

    public ZumenSearch.Presentation.Features.Rent.Lessors.ViewModels.LessorViewModel? ViewModel { get; private set; }

    public EditorWindow(string id, ZumenSearch.Presentation.Features.Rent.Lessors.ViewModels.LessorViewModel vm)
    {
        Id = id;
        ViewModel = vm;

        InitializeComponent();

        ExtendsContentIntoTitleBar = true;

        //this.AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets\\App.ico"));
        this.AppWindow.SetIcon("Assets/App.ico");
    }

}
