using Microsoft.UI.Xaml;

namespace ZumenSearch.Views.Brokers;

public sealed partial class EditorWindow : Window
{
    public string? Id { get; private set; } = string.Empty;

    // TODO: do I need this?
    public bool IsAutoClose { get; set; }

    public ViewModels.Brokers.BrokerViewModel? ViewModel { get; private set; }

    public EditorWindow(string id, ViewModels.Brokers.BrokerViewModel vm)
    {
        Id = id;
        ViewModel = vm;

        InitializeComponent();

        ExtendsContentIntoTitleBar = true;

        //this.AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets\\App.ico"));
        this.AppWindow.SetIcon("Assets/App.ico");
    }

}
