using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.Services;

public class NavigationService : INavigationService
{
    private Frame? _frame;


    public void Initialize(Frame frame, Dictionary<string, Type> _pages)
    {
        _frame = frame;
        _pageMap = _pages;
    }

    private Dictionary<string, Type>? _pageMap;

    public bool NavigateTo(object? selectedPage, SlideNavigationTransitionEffect effect)
    {
        if (_frame is null) return false;
        if (_pageMap is null) return false;

        string? tag = null;
        if (selectedPage is NavigationViewItem navItem)
        {
            tag = navItem.Tag as string;
        }
        else if (selectedPage is string str)
        {
            tag = str;
        }

        if (tag != null && _pageMap.TryGetValue(tag, out var pageType) && _frame.CurrentSourcePageType != pageType)
        {
            return _frame.Navigate(pageType, _frame, new SlideNavigationTransitionInfo() { Effect = effect });//new SuppressNavigationTransitionInfo()
        }
        else
        {
            Debug.WriteLine("NavigationService.NavigateTo: No valid page found (or navigated to the same page) for " + tag);
            return false;
        }
    }


    public void GoBack()
    {
        if (_frame != null && _frame.CanGoBack)
        {
            _frame.GoBack();
        }
    }

    public bool CanGoBack()
    {
        return _frame != null && _frame.CanGoBack;
    }
}
