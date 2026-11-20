namespace ZumenSearch.Presentation.Factories.AbstractFactory;

/*
public interface IAbstractFactory<T>
{
    T Create();
}
*/
/*
public interface IAbstractFactory<in TParam, out T>
{
    T Create(TParam param);
}
*/
public interface IAbstractFactory<TParam, T>
{
    public T Create(TParam param);
}

public interface IAbstractFactory<TParam1, TParam2, TResult>
{
    TResult Create(TParam1 param1, TParam2 param2);
}

public interface IAbstractFactory<TParam1, TParam2, TParam3, TResult>
{
    TResult Create(TParam1 param1, TParam2 param2, TParam3 param3);
}

public interface IAbstractFactory<TParam1, TParam2, TParam3, TParam4, TResult>
{
    TResult Create(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4);
}