using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.Services;

public class NavigationResidentialService : INavigationResidentialService
{
    private Frame? _frame;
    private Window? _window;

    public void Initialize(Frame frame, Window win)
    {
        _frame = frame;
        _window = win;
    }

    public Frame? GetFrame()
    {
        return _frame;
    }

    public Window? GetWindow()
    {
        return _window;
    }
}
