using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using ZumenSearch.Presentation.Factories.AbstractFactory;
namespace ZumenSearch.Presentation.Factories;

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

    // Four params
    public static void AddGenericFactory<TParam1, TParam2, TParam3, TParam4, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(this IServiceCollection services)
    where T : class
    {
        // Just register the factory. It will handle the ActivatorUtilities internally.
        services.AddSingleton<IAbstractFactory<TParam1, TParam2, TParam3, TParam4, T>, AbstractFactory<TParam1, TParam2, TParam3, TParam4, T>>();
    }

    public static void AddGenericFactory<TParam1, TParam2, TParam3, TParam4, TParam5, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(this IServiceCollection services)
    where T : class
    {
        services.AddSingleton<IAbstractFactory<TParam1, TParam2, TParam3, TParam4, TParam5, T>, AbstractFactory<TParam1, TParam2, TParam3, TParam4, TParam5, T>>();
    }
}
