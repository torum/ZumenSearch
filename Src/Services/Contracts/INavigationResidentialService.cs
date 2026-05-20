using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace ZumenSearch.Services.Contracts;

public interface INavigationResidentialService
{
    void Initialize(Frame frame, Window win);

    Frame? GetFrame();

    Window? GetWindow();

}
