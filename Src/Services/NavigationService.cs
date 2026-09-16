using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System.Diagnostics;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.Services;

public class NavigationService : INavigationService
{
    private Frame? _frame;

    private readonly Dictionary<string, Type> _pageMap = new()
        {
            { "ZumenSearch.Views.SearchPage", typeof(Views.SearchPage) },
            { "ZumenSearch.Views.SearchResultPage", typeof(Views.SearchResultPage) },
            { "ZumenSearch.Views.Rent.ResidentialSearchPage", typeof(Views.Rent.ResidentialSearchPage) },
            { "ZumenSearch.Views.Rent.ResidentialSearchResultPage", typeof(Views.Rent.ResidentialSearchResultPage) },
            { "ZumenSearch.Views.Rent.Commercials.CommercialsPage", typeof(Views.Rent.Commercials.CommercialsPage) },
            { "ZumenSearch.Views.Rent.Parkings.ParkingsPage", typeof(Views.Rent.Parkings.ParkingsPage) },
            { "ZumenSearch.Views.Rent.LessorSearchPage", typeof(Views.Rent.LessorSearchPage) },
            { "ZumenSearch.Views.Rent.LessorSearchResultPage", typeof(Views.Rent.LessorSearchResultPage) },
            { "ZumenSearch.Views.Brokers.BrokersPage", typeof(Views.Brokers.BrokersPage) },
            { "ZumenSearch.Views.SettingsPage", typeof(Views.SettingsPage) }
        };

    public void Initialize(Frame frame)
    {
        _frame = frame;
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

    public void NavigateToMainSearch()
    {
        _frame?.Navigate(typeof(Views.SearchPage), _frame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
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
