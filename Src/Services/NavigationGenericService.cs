using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.Services;

public class NavigationGenericService : INavigationGenericService
{
    private List<(string Tag, string Label, Type? Page)>? _pages;

    private Frame? _frame;

    public void Initialize(Frame frame, List<(string Tag, string Label, Type? Page)> pages)
    {
        _frame = frame;
        _pages = pages;
    }

    public Frame? GetFrame()
    {
        return _frame;
    }

    public bool NavigateTo(object? selectedPage, object? param)
    {
        if (_frame is null)
        {
            Debug.WriteLine("NavigationGenericService: _frame is null. Not initialized.");
            return false; 
        }
        if (_pages is null)
        {
            Debug.WriteLine("NavigationGenericService: _pages is null. Not initialized.");
            return false;
        }

        string? tag = null;
        if (selectedPage is NavigationViewItem navItem)
        {
            tag = navItem.Tag as string;
        }
        else if (selectedPage is string str)
        {
            tag = str;
        }

        if (string.IsNullOrEmpty(tag))
        {
            return false;
        }

        var item = _pages.FirstOrDefault(p => p.Tag.Equals(tag, StringComparison.Ordinal));

        if (item.Page is null)
        {
            Debug.WriteLine("NavigationGenericService.NavigateTo: Page is null for tag " + tag);
            return false;
        }

        if (_frame.Navigate(item.Page, param, new DrillInNavigationTransitionInfo())) //new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom })SuppressNavigationTransitionInfo
        {
            return true;
        }
        else
        {
            Debug.WriteLine("NavigationGenericService.NavigateTo: No valid page found for " + tag);
            return false;
        }
    }

    public bool IsCurrentPageSameAs(object? selectedPage)
    {
        if (_frame is null)
        {
            Debug.WriteLine("NavigationGenericService: _frame is null. Not initialized.");
            return false;
        }
        if (_pages is null)
        {
            Debug.WriteLine("NavigationGenericService: _pages is null. Not initialized.");
            return false;
        }

        string? tag = null;
        if (selectedPage is NavigationViewItem navItem)
        {
            tag = navItem.Tag as string;
        }
        else if (selectedPage is string str)
        {
            tag = str;
        }

        if (string.IsNullOrEmpty(tag))
        {
            return false;
        }

        var item = _pages.FirstOrDefault(p => p.Tag.Equals(tag, StringComparison.Ordinal));

        if (item.Page is null)
        {
            Debug.WriteLine("NavigationGenericService.IsCurrentPage: Page is null for tag " + tag);
            return false;
        }
        
        if (item.Page == _frame.CurrentSourcePageType)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
