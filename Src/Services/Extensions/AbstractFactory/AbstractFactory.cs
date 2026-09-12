using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace ZumenSearch.Services.Extensions.AbstractFactory;

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
public sealed class AbstractFactory<TParam, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T> : IAbstractFactory<TParam, T>
{
    private readonly IServiceProvider _serviceProvider;

    public AbstractFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public T Create(TParam param)
    {
        // The logic lives here instead of in the Program.cs registration
        return ActivatorUtilities.CreateInstance<T>(_serviceProvider, param!);
    }
}

public class AbstractFactory<TParam1, TParam2, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TResult> : IAbstractFactory<TParam1, TParam2, TResult>
    where TResult : class
{
    private readonly IServiceProvider _serviceProvider;

    public AbstractFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public TResult Create(TParam1 param1, TParam2 param2)
    {
        // ActivatorUtilities will match param1 and param2 by their types to the constructor
        return ActivatorUtilities.CreateInstance<TResult>(_serviceProvider, param1!, param2!);
    }
}

public class AbstractFactory<TParam1, TParam2, TParam3, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TResult> : IAbstractFactory<TParam1, TParam2, TParam3, TResult>
    where TResult : class
{
    private readonly IServiceProvider _serviceProvider;

    public AbstractFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public TResult Create(TParam1 param1, TParam2 param2, TParam3 param3)
    {
        // ActivatorUtilities will match param1 and param2 by their types to the constructor
        return ActivatorUtilities.CreateInstance<TResult>(_serviceProvider, param1!, param2!, param3!);
    }
}
