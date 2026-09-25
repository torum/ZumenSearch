using Microsoft.UI.Xaml;

namespace ZumenSearch.Views.Rent.Commercials;

public sealed partial class EditorWindow : Window
{
    public string Id { get; }

    public ViewModels.Rent.Commercials.PropertyViewModel ViewModel
    {
        get;
    }

    public EditorWindow(
        string id,
        ViewModels.Rent.Commercials.PropertyViewModel viewModel)
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