using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZumenSearch.Services;

public interface INavigationService
{
    void NavigateTo(object? selectedPage, SlideNavigationTransitionEffect effect);
    void NavigateToMainSearch();
    void Initialize(Frame frame);
    void GoBack();
    bool CanGoBack();
}
