using System.Linq.Expressions;

namespace SmartPharmacySystem.Infrastructure.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int page, int pageSize)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        return query.Skip((page - 1) * pageSize).Take(pageSize);
    }

    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string sortBy, string sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query;

        var param = Expression.Parameter(typeof(T), "x");
        var property = typeof(T).GetProperty(sortBy, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        if (property == null)
            return query; // Or throw exception, but ignoring invalid sort is safer for API

        var selector = Expression.MakeMemberAccess(param, property);
        var lambda = Expression.Lambda(selector, param);

        string methodName = sortDirection.ToLower() == "desc" ? "OrderByDescending" : "OrderBy";
        
        var resultExpression = Expression.Call(
            typeof(Queryable), 
            methodName, 
            new Type[] { typeof(T), property.PropertyType },
            query.Expression, 
            Expression.Quote(lambda)
        );

        return query.Provider.CreateQuery<T>(resultExpression);
    }
}
