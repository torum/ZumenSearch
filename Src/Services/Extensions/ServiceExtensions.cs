using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using ZumenSearch.Services.Extensions.AbstractFactory;

namespace ZumenSearch.Services.Extensions;

public static class ServiceExtensions
{
    /*
    public static void AddEditorFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TEditor>(this IServiceCollection services)
        where TEditor : class
    {
        services.AddTransient<TEditor>();
        services.AddSingleton<Func<TEditor>>(x => () => x.GetService<TEditor>()!);
        services.AddSingleton<IAbstractFactory<TEditor>, AbstractFactory<TEditor>>();
    }
    */
    /*
    public static void AddEditorFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TEditor, TParam>(this IServiceCollection services)
        where TEditor : class
    {
        // 1. Register the Func delegate using ActivatorUtilities
        // This handles the "mixing" of DI services and your manual parameter
        services.AddSingleton<Func<TParam, TEditor>>(sp => param =>
        {
            return ActivatorUtilities.CreateInstance<TEditor>(sp, param!);
        });

        // 2. Register the IAbstractFactory implementation
        // This wraps the Func into your clean interface
        services.AddSingleton<IAbstractFactory<TParam, TEditor>, AbstractFactory<TParam, TEditor>>();
    }
    */

    public static void AddEditorFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TEditor, TParam>(this IServiceCollection services)
    where TEditor : class
    {

        // Just register the factory. It will handle the ActivatorUtilities internally.
        services.AddSingleton<IAbstractFactory<TParam, TEditor>, AbstractFactory<TParam, TEditor>>();
    }
}
