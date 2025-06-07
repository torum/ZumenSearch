using System;

namespace ZumenSearch.Services.Extensions.AbstractFactory;

public interface IAbstractFactory<T>
{
    T Create();
}