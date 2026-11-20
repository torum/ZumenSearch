using Microsoft.UI.Xaml;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Views;

public sealed partial class EditorWindow : Window
{
    public string Id { get; }

    public ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyViewModel ViewModel
    {
        get;
    }

    public EditorWindow(
        string id,
        ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyViewModel viewModel)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException(
                "The commercial property ID cannot be empty.",
                nameof(id));
        }

        Id = id;
        ViewModel = viewModel;

        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        AppWindow.SetIcon("Assets/App.ico");
    }
}