using System.Linq.Expressions;

namespace Aplicada1.Core;

public interface IServiceResult<T, TKey> where T : class
{
    Task<Result> Guardar(T entidad);
    Task<Result<T?>> Buscar(TKey id);
    Task<Result> Eliminar(TKey id);
    Task<Result<List<T>>> GetList(Expression<Func<T, bool>> criterio);
}