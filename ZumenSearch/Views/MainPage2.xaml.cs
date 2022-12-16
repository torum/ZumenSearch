using Microsoft.UI.Xaml.Controls;

using ZumenSearch.ViewModels;

namespace ZumenSearch.Views;

public sealed partial class MainPage2 : Page
{
    public MainViewModel2 ViewModel
    {
        get;
    }

    public MainPage2()
    {
        ViewModel = App.GetService<MainViewModel2>();
        InitializeComponent();
    }
}
