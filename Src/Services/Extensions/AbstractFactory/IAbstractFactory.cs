namespace ZumenSearch.Services.Extensions.AbstractFactory;

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