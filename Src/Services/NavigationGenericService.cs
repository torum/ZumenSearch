using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System.Diagnostics;
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

    public bool NavigateTo(object? selectedPage, SlideNavigationTransitionEffect effect)
    {
        if (_frame is null) return false;

        string? tag = null;
        if (selectedPage is NavigationViewItem navItem)
        {
            tag = navItem.Tag as string;
        }
        else if (selectedPage is string str)
        {
            tag = str;
        }

        // TODO:

        // var item = _pages.FirstOrDefault(p => p.Tag.Equals("summary"));

        /*
        if (tag != null && _pageMap.TryGetValue(tag, out var pageType) && _frame.CurrentSourcePageType != pageType)
        {
            return _frame.Navigate(pageType, _frame, new SlideNavigationTransitionInfo() { Effect = effect });//new SuppressNavigationTransitionInfo()
        }
        else
        {
            Debug.WriteLine("NavigationService.NavigateTo: No valid page found for " + tag);
            return false;
        }
        */

        return true;//tmp
    }
}
