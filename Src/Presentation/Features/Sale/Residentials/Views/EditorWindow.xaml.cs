using Microsoft.UI.Xaml;

namespace ZumenSearch.Presentation.Features.Sale.Residentials.Views;

public sealed partial class EditorWindow : Window
{
    public string Id { get; }

    public ZumenSearch.Presentation.Features.Sale.Residentials.ViewModels.PropertyViewModel ViewModel
    {
        get;
    }

    public EditorWindow(
        string id,
        ZumenSearch.Presentation.Features.Sale.Residentials.ViewModels.PropertyViewModel viewModel)
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