using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using ZumenSearch.Services.Extensions.AbstractFactory;

namespace ZumenSearch.Services.Extensions;

public static class ServiceExtensions
{
    // One param.
    public static void AddGenericFactory<TParam, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TEditor>(this IServiceCollection services)
    where TEditor : class
    {
        // Just register the factory. It will handle the ActivatorUtilities internally.
        services.AddSingleton<IAbstractFactory<TParam, TEditor>, AbstractFactory<TParam, TEditor>>();
    }

    // Two params.
    public static void AddGenericFactory<TParam1, TParam2, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TEditor>(this IServiceCollection services)
    where TEditor : class
    {
        // Just register the factory. It will handle the ActivatorUtilities internally.
        services.AddSingleton<IAbstractFactory<TParam1, TParam2, TEditor>, AbstractFactory<TParam1, TParam2, TEditor>>();
    }

    // Three params
    public static void AddGenericFactory<TParam1, TParam2, TParam3, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TEditor>(this IServiceCollection services)
    where TEditor : class
    {
        // Just register the factory. It will handle the ActivatorUtilities internally.
        services.AddSingleton<IAbstractFactory<TParam1, TParam2, TParam3, TEditor>, AbstractFactory<TParam1, TParam2, TParam3, TEditor>>();
    }
}
