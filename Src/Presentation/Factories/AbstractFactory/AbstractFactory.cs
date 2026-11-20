using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

namespace ZumenSearch.Presentation.Factories.AbstractFactory;

/*
public class AbstractFactory<T>(Func<T> factory) : IAbstractFactory<T>
{
    private readonly Func<T> _factory = factory;

    public T Create()
    {
        return _factory();
    }
}
*/
/*
public class AbstractFactory<TParam, T> : IAbstractFactory<TParam, T>
{
    private readonly Func<TParam, T> _factory;
    public AbstractFactory(Func<TParam, T> factory) => _factory = factory;
    public T Create(TParam param) => _factory(param);
}
*/
public sealed partial class AbstractFactory<TParam, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T> : IAbstractFactory<TParam, T>
{
    private readonly IServiceProvider _serviceProvider;

    public AbstractFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public T Create(TParam param) => ActivatorUtilities.CreateInstance<T>(_serviceProvider, param!);
}

public partial class AbstractFactory<TParam1, TParam2, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TResult> : IAbstractFactory<TParam1, TParam2, TResult>
    where TResult : class
{
    private readonly IServiceProvider _serviceProvider;

    public AbstractFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public TResult Create(TParam1 param1, TParam2 param2) => ActivatorUtilities.CreateInstance<TResult>(_serviceProvider, param1!, param2!);
}

public partial class AbstractFactory<TParam1, TParam2, TParam3, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TResult> : IAbstractFactory<TParam1, TParam2, TParam3, TResult>
    where TResult : class
{
    private readonly IServiceProvider _serviceProvider;

    public AbstractFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public TResult Create(TParam1 param1, TParam2 param2, TParam3 param3) => ActivatorUtilities.CreateInstance<TResult>(_serviceProvider, param1!, param2!, param3!);
}

public partial class AbstractFactory<TParam1, TParam2, TParam3, TParam4, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TResult> : IAbstractFactory<TParam1, TParam2, TParam3, TParam4, TResult>
    where TResult : class
{
    private readonly IServiceProvider _serviceProvider;

    public AbstractFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public TResult Create(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4) => ActivatorUtilities.CreateInstance<TResult>(_serviceProvider, param1!, param2!, param3!, param4!);
}
