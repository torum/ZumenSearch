using Microsoft.UI.Xaml;

namespace ZumenSearch.Views.Sale.Residentials;

public sealed partial class EditorWindow : Window
{
    public string Id { get; }

    public ViewModels.Sale.Residentials.PropertyViewModel ViewModel
    {
        get;
    }

    public EditorWindow(
        string id,
        ViewModels.Sale.Residentials.PropertyViewModel viewModel)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException(
                "The property ID cannot be empty.",
                nameof(id));
        }

        Id = id;
        ViewModel = viewModel;

        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        AppWindow.SetIcon("Assets/App.ico");
    }
}