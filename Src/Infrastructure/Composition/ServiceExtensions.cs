using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using ZumenSearch.Services.Extensions.AbstractFactory;

namespace ZumenSearch.Services.Extensions;

public static class ServiceExtensions
{
    // One param.
    public static void AddGenericFactory<TParam, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(this IServiceCollection services)
    where T : class
    {
        // Just register the factory. It will handle the ActivatorUtilities internally.
        services.AddSingleton<IAbstractFactory<TParam, T>, AbstractFactory<TParam, T>>();
    }

    // Two params.
    public static void AddGenericFactory<TParam1, TParam2, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(this IServiceCollection services)
    where T : class
    {
        // Just register the factory. It will handle the ActivatorUtilities internally.
        services.AddSingleton<IAbstractFactory<TParam1, TParam2, T>, AbstractFactory<TParam1, TParam2, T>>();
    }

    // Three params
    public static void AddGenericFactory<TParam1, TParam2, TParam3, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(this IServiceCollection services)
    where T : class
    {
        // Just register the factory. It will handle the ActivatorUtilities internally.
        services.AddSingleton<IAbstractFactory<TParam1, TParam2, TParam3, T>, AbstractFactory<TParam1, TParam2, TParam3, T>>();
    }
}
