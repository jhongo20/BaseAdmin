using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;
using AuthSystem.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AuthSystem.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Implementación genérica del repositorio
    /// </summary>
    /// <typeparam name="T">Tipo de entidad</typeparam>
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        public Repository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        /// <summary>
        /// Obtiene todas las entidades
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de entidades</returns>
        public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene una entidad por su ID
        /// </summary>
        /// <param name="id">ID de la entidad</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Entidad encontrada o null</returns>
        public virtual async Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        /// <summary>
        /// Obtiene una entidad por su ID con inclusiones específicas
        /// </summary>
        /// <param name="id">ID de la entidad</param>
        /// <param name="includes">Expresiones de inclusión</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Entidad encontrada o null</returns>
        public virtual async Task<T> GetByIdAsync(Guid id, List<Expression<Func<T, object>>> includes, CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;

            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            return await query.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        /// <summary>
        /// Agrega una nueva entidad
        /// </summary>
        /// <param name="entity">Entidad a agregar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Entidad agregada</returns>
        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            return entity;
        }

        /// <summary>
        /// Actualiza una entidad existente
        /// </summary>
        /// <param name="entity">Entidad a actualizar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            _context.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Elimina una entidad
        /// </summary>
        /// <param name="entity">Entidad a eliminar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Obtiene entidades que cumplen una condición
        /// </summary>
        /// <param name="predicate">Condición a cumplir</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de entidades que cumplen la condición</returns>
        public virtual async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene entidades con ordenamiento y paginación
        /// </summary>
        /// <param name="predicate">Condición a cumplir</param>
        /// <param name="orderBy">Función de ordenamiento</param>
        /// <param name="includeProperties">Propiedades a incluir</param>
        /// <param name="skip">Cantidad de elementos a omitir</param>
        /// <param name="take">Cantidad de elementos a tomar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de entidades que cumplen los criterios</returns>
        public virtual async Task<IReadOnlyList<T>> GetAsync(
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = null,
            int? skip = null,
            int? take = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return await query.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene entidades que cumplen con un predicado
        /// </summary>
        /// <param name="predicate">Predicado de filtro</param>
        /// <param name="orderBy">Función de ordenamiento (opcional)</param>
        /// <param name="includeString">Cadenas de inclusión separadas por comas (opcional)</param>
        /// <param name="disableTracking">Indica si se debe deshabilitar el tracking (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de entidades que cumplen el predicado</returns>
        public virtual async Task<IReadOnlyList<T>> GetAsync(
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeString = null,
            bool disableTracking = true,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (!string.IsNullOrWhiteSpace(includeString))
            {
                foreach (var include in includeString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(include.Trim());
                }
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync(cancellationToken);
            }

            return await query.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene entidades que cumplen con un predicado con inclusiones específicas
        /// </summary>
        /// <param name="predicate">Predicado de filtro</param>
        /// <param name="orderBy">Función de ordenamiento (opcional)</param>
        /// <param name="includes">Lista de expresiones de inclusión (opcional)</param>
        /// <param name="disableTracking">Indica si se debe deshabilitar el tracking (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de entidades que cumplen el predicado</returns>
        public virtual async Task<IReadOnlyList<T>> GetAsync(
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            List<Expression<Func<T, object>>> includes = null,
            bool disableTracking = true,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync(cancellationToken);
            }

            return await query.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene la primera entidad que cumple una condición
        /// </summary>
        /// <param name="predicate">Condición a cumplir</param>
        /// <param name="includeProperties">Propiedades a incluir</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Entidad encontrada o null</returns>
        public virtual async Task<T> GetFirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            string includeProperties = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            return await query.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// Verifica si existe alguna entidad que cumpla una condición
        /// </summary>
        /// <param name="predicate">Condición a cumplir</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si existe alguna entidad que cumpla la condición</returns>
        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// Verifica si existe alguna entidad que cumpla con un predicado
        /// </summary>
        /// <param name="predicate">Predicado de filtro</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si existe al menos una entidad que cumple el predicado</returns>
        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// Cuenta las entidades que cumplen una condición
        /// </summary>
        /// <param name="predicate">Condición a cumplir</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de entidades que cumplen la condición</returns>
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default)
        {
            if (predicate == null)
            {
                return await _dbSet.CountAsync(cancellationToken);
            }
            else
            {
                return await _dbSet.CountAsync(predicate, cancellationToken);
            }
        }

        /// <summary>
        /// Obtiene entidades paginadas
        /// </summary>
        /// <param name="predicate">Condición a cumplir</param>
        /// <param name="orderBy">Función de ordenamiento</param>
        /// <param name="includeProperties">Propiedades a incluir</param>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tupla con la lista de entidades y el total de elementos</returns>
        public virtual async Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            var totalCount = await query.CountAsync(cancellationToken);

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
