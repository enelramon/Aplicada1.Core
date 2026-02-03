namespace Aplicada1.Core;

public interface IService<T, TKey> where T : class
{
    Task<bool> Guardar(T entidad);
    Task<T?> Buscar(TKey id);
    Task<bool> Eliminar(TKey id);
    Task<List<T>> GetList(Expression<Func<T, bool>> criterio);
}
