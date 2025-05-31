using System;

namespace ZumenSearch.Extensions.AbstractFactory;

public class AbstractFactory<T>(Func<T> factory) : IAbstractFactory<T>
{
    private readonly Func<T> _factory = factory;

    public T Create()
    {
        return _factory();
    }
}


