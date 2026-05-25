using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ZumenSearch.Services.Contracts;

public interface INavigationResidentialService
{
    void Initialize(Frame frame, Window win);

    Frame? GetFrame();

    Window? GetWindow();

}
