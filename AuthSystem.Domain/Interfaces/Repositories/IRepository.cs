using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz genérica para repositorios
    /// </summary>
    /// <typeparam name="T">Tipo de entidad</typeparam>
    public interface IRepository<T> where T : BaseEntity
    {
        /// <summary>
        /// Obtiene todas las entidades
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de entidades</returns>
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene entidades según una condición
        /// </summary>
        /// <param name="predicate">Condición de filtrado</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de entidades que cumplen la condición</returns>
        Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene entidades con ordenamiento
        /// </summary>
        /// <param name="predicate">Condición de filtrado</param>
        /// <param name="orderBy">Función de ordenamiento</param>
        /// <param name="includeString">Propiedades a incluir (eager loading)</param>
        /// <param name="disableTracking">Indica si se debe deshabilitar el tracking de EF</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de entidades</returns>
        Task<IReadOnlyList<T>> GetAsync(
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeString = null,
            bool disableTracking = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene entidades con múltiples includes
        /// </summary>
        /// <param name="predicate">Condición de filtrado</param>
        /// <param name="orderBy">Función de ordenamiento</param>
        /// <param name="includes">Propiedades a incluir (eager loading)</param>
        /// <param name="disableTracking">Indica si se debe deshabilitar el tracking de EF</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de entidades</returns>
        Task<IReadOnlyList<T>> GetAsync(
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            List<Expression<Func<T, object>>> includes = null,
            bool disableTracking = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene una entidad por su ID
        /// </summary>
        /// <param name="id">ID de la entidad</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Entidad encontrada o null</returns>
        Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene una entidad por su ID con includes
        /// </summary>
        /// <param name="id">ID de la entidad</param>
        /// <param name="includes">Propiedades a incluir (eager loading)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Entidad encontrada o null</returns>
        Task<T> GetByIdAsync(Guid id, List<Expression<Func<T, object>>> includes, CancellationToken cancellationToken = default);

        /// <summary>
        /// Agrega una nueva entidad
        /// </summary>
        /// <param name="entity">Entidad a agregar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Entidad agregada</returns>
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza una entidad existente
        /// </summary>
        /// <param name="entity">Entidad a actualizar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina una entidad
        /// </summary>
        /// <param name="entity">Entidad a eliminar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si existe alguna entidad que cumpla la condición
        /// </summary>
        /// <param name="predicate">Condición a verificar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si existe al menos una entidad que cumple la condición</returns>
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cuenta el número de entidades que cumplen una condición
        /// </summary>
        /// <param name="predicate">Condición a verificar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de entidades que cumplen la condición</returns>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default);
    }
}
