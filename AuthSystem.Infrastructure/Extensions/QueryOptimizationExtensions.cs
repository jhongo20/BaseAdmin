using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace AuthSystem.Infrastructure.Extensions
{
    /// <summary>
    /// Extensiones para optimizar consultas SQL
    /// </summary>
    public static class QueryOptimizationExtensions
    {
        /// <summary>
        /// Aplica optimizaciones a una consulta IQueryable
        /// </summary>
        /// <typeparam name="T">Tipo de entidad</typeparam>
        /// <param name="query">Consulta a optimizar</param>
        /// <returns>Consulta optimizada</returns>
        public static IQueryable<T> OptimizeQuery<T>(this IQueryable<T> query) where T : class
        {
            // Aplicar AsNoTracking para consultas de solo lectura
            return query.AsNoTracking();
        }

        /// <summary>
        /// Aplica paginación optimizada a una consulta
        /// </summary>
        /// <typeparam name="T">Tipo de entidad</typeparam>
        /// <param name="query">Consulta a paginar</param>
        /// <param name="page">Número de página (base 1)</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <returns>Consulta paginada</returns>
        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int page, int pageSize) where T : class
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            return query.Skip((page - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Aplica ordenamiento optimizado a una consulta
        /// </summary>
        /// <typeparam name="T">Tipo de entidad</typeparam>
        /// <typeparam name="TKey">Tipo de la clave de ordenamiento</typeparam>
        /// <param name="query">Consulta a ordenar</param>
        /// <param name="keySelector">Selector de la clave de ordenamiento</param>
        /// <param name="ascending">Indica si el ordenamiento es ascendente</param>
        /// <returns>Consulta ordenada</returns>
        public static IQueryable<T> ApplySorting<T, TKey>(this IQueryable<T> query, 
            Expression<Func<T, TKey>> keySelector, bool ascending = true) where T : class
        {
            return ascending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
        }

        /// <summary>
        /// Aplica filtrado optimizado por texto a una consulta
        /// </summary>
        /// <typeparam name="T">Tipo de entidad</typeparam>
        /// <param name="query">Consulta a filtrar</param>
        /// <param name="predicate">Predicado de filtrado</param>
        /// <returns>Consulta filtrada</returns>
        public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> query, 
            Expression<Func<T, bool>> predicate) where T : class
        {
            return query.Where(predicate);
        }

        /// <summary>
        /// Aplica una proyección optimizada a una consulta
        /// </summary>
        /// <typeparam name="T">Tipo de entidad de origen</typeparam>
        /// <typeparam name="TResult">Tipo de entidad de resultado</typeparam>
        /// <param name="query">Consulta a proyectar</param>
        /// <param name="selector">Selector de proyección</param>
        /// <returns>Consulta proyectada</returns>
        public static IQueryable<TResult> ApplyProjection<T, TResult>(this IQueryable<T> query, 
            Expression<Func<T, TResult>> selector) where T : class
        {
            return query.Select(selector);
        }

        /// <summary>
        /// Aplica una carga anticipada (eager loading) optimizada a una consulta
        /// </summary>
        /// <typeparam name="T">Tipo de entidad</typeparam>
        /// <param name="query">Consulta a optimizar</param>
        /// <param name="includeExpressions">Expresiones de inclusión</param>
        /// <returns>Consulta con carga anticipada</returns>
        public static IQueryable<T> ApplyIncludes<T>(this IQueryable<T> query, 
            params Expression<Func<T, object>>[] includeExpressions) where T : class
        {
            if (includeExpressions == null) return query;

            foreach (var includeExpression in includeExpressions)
            {
                query = query.Include(includeExpression);
            }

            return query;
        }

        /// <summary>
        /// Aplica una carga anticipada (eager loading) optimizada con filtrado a una consulta
        /// </summary>
        /// <typeparam name="T">Tipo de entidad</typeparam>
        /// <typeparam name="TProperty">Tipo de la propiedad a incluir</typeparam>
        /// <param name="query">Consulta a optimizar</param>
        /// <param name="navigationPropertyPath">Expresión de la propiedad de navegación</param>
        /// <param name="filterExpression">Expresión de filtrado para la propiedad incluida</param>
        /// <returns>Consulta con carga anticipada filtrada</returns>
        public static IQueryable<T> ApplyFilteredInclude<T, TProperty>(this IQueryable<T> query, 
            Expression<Func<T, IEnumerable<TProperty>>> navigationPropertyPath, 
            Expression<Func<TProperty, bool>> filterExpression) where T : class where TProperty : class
        {
            return query.Include(navigationPropertyPath.ApplyFilter(filterExpression));
        }

        /// <summary>
        /// Aplica un filtro a una expresión de inclusión
        /// </summary>
        private static Expression<Func<T, IEnumerable<TProperty>>> ApplyFilter<T, TProperty>(
            this Expression<Func<T, IEnumerable<TProperty>>> navigationPropertyPath,
            Expression<Func<TProperty, bool>> filterExpression) where T : class where TProperty : class
        {
            var parameter = Expression.Parameter(typeof(T), "e");
            var memberExpression = navigationPropertyPath.Body as MemberExpression;
            
            if (memberExpression == null)
                throw new ArgumentException("La expresión debe ser una propiedad de navegación", nameof(navigationPropertyPath));

            var propertyExpression = Expression.Property(parameter, memberExpression.Member.Name);
            var whereCall = Expression.Call(
                typeof(Enumerable),
                "Where",
                new[] { typeof(TProperty) },
                propertyExpression,
                filterExpression);

            return Expression.Lambda<Func<T, IEnumerable<TProperty>>>(whereCall, parameter);
        }
    }
}
