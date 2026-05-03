using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace ZumenSearch.Services.Contracts;

public interface INavigationService
{
    void NavigateTo(object? selectedPage, SlideNavigationTransitionEffect effect);
    void NavigateToMainSearch();
    void Initialize(Frame frame);
    void GoBack();
    bool CanGoBack();
}
