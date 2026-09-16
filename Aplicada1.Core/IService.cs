namespace Aplicada1.Core;

using System.Linq.Expressions;

public interface IService<T, TKey> where T : class
{
    Task<bool> Guardar(T entidad);
    Task<T?> Buscar(TKey id);
    Task<bool> Eliminar(TKey id);
    Task<List<T>> GetList(Expression<Func<T, bool>> criterio);
}

public interface IServiceResult<T, TKey> where T : class
{
    Task<Result> Guardar(T entidad);
    Task<Result<T?>> Buscar(TKey id);
    Task<Result> Eliminar(TKey id);
    Task<Result<List<T>>> GetList(Expression<Func<T, bool>> criterio);
}
