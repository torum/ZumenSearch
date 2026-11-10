using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace ZumenSearch.Views.Services.Contracts;

public interface INavigationGenericService
{
    void Initialize(Frame frame, List<(string Tag, string Label, Type? Page)> pages);

    Frame? GetFrame();

    bool NavigateTo(object? selectedPage, object? param);

    bool IsCurrentPageSameAs(object? selectedPage);
}
