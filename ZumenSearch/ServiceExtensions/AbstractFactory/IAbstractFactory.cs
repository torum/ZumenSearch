using System;

namespace ZumenSearch.Extensions.AbstractFactory;

public interface IAbstractFactory<T>
{
    T Create();
}