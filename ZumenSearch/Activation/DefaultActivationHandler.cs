﻿using Microsoft.UI.Xaml;

using ZumenSearch.Contracts.Services;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Activation;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService _navigationService;

    public DefaultActivationHandler(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        // None of the ActivationHandlers has handled the activation.
        return _navigationService.Frame?.Content == null;
    }

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        // not working when navi view is hiera.. must be a bug...
        //_navigationService.NavigateTo(typeof(RentMainViewModel).FullName!, args.Arguments);

        await Task.CompletedTask;
    }
}
