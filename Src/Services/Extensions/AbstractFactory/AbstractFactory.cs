using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

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
public sealed partial class AbstractFactory<TParam, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T> : IAbstractFactory<TParam, T>, IDisposable
{
    private readonly IServiceProvider _serviceProvider;

    // Ownership tracking.
    private readonly Lock _gate = new();
    private readonly List<IDisposable> _instances = [];
    private bool _disposed;

    public AbstractFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public T Create(TParam param)
    {
        // The logic lives here instead of in the Program.cs registration
        //return ActivatorUtilities.CreateInstance<T>(_serviceProvider, param!);

        // Ownership tracking.
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var instance = ActivatorUtilities.CreateInstance<T>(_serviceProvider, param!);

            if (instance is IDisposable disposable)
            {
                _instances.Add(disposable);
            }

            return instance;
        }
    }

    public void Dispose()
    {
        IDisposable[] instances;

        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            instances = [.. _instances];
            _instances.Clear();
        }

        foreach (var instance in instances)
        {
            instance.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}

public partial class AbstractFactory<TParam1, TParam2, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TResult> : IAbstractFactory<TParam1, TParam2, TResult>, IDisposable
    where TResult : class
{
    private readonly IServiceProvider _serviceProvider;

    // Ownership tracking.
    private readonly Lock _gate = new();
    private readonly List<IDisposable> _instances = [];
    private bool _disposed;

    public AbstractFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public TResult Create(TParam1 param1, TParam2 param2)
    {
        // ActivatorUtilities will match param1 and param2 by their types to the constructor
        //return ActivatorUtilities.CreateInstance<TResult>(_serviceProvider, param1!, param2!);

        // Ownership tracking.
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var instance = ActivatorUtilities.CreateInstance<TResult>(_serviceProvider, param1!, param2!);

            if (instance is IDisposable disposable)
            {
                _instances.Add(disposable);
            }

            return instance;
        }
    }

    public void Dispose()
    {
        IDisposable[] instances;

        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            instances = [.. _instances];
            _instances.Clear();
        }

        foreach (var instance in instances)
        {
            instance.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}

public partial class AbstractFactory<TParam1, TParam2, TParam3, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TResult> : IAbstractFactory<TParam1, TParam2, TParam3, TResult>, IDisposable
    where TResult : class
{
    private readonly IServiceProvider _serviceProvider;

    // Ownership tracking.
    private readonly Lock _gate = new();
    private readonly List<IDisposable> _instances = [];
    private bool _disposed;

    public AbstractFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public TResult Create(TParam1 param1, TParam2 param2, TParam3 param3)
    {
        // ActivatorUtilities will match param1 and param2 by their types to the constructor
        //return ActivatorUtilities.CreateInstance<TResult>(_serviceProvider, param1!, param2!, param3!);

        // Ownership tracking.
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var instance = ActivatorUtilities.CreateInstance<TResult>(_serviceProvider, param1!, param2!, param3!);

            if (instance is IDisposable disposable)
            {
                _instances.Add(disposable);
            }

            return instance;
        }
    }

    public void Dispose()
    {
        IDisposable[] instances;

        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            instances = [.. _instances];
            _instances.Clear();
        }

        foreach (var instance in instances)
        {
            instance.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}
