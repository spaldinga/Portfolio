using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp;
/// <summary>
/// Provides predicate composition extension methods.
/// </summary>
public static class PredicateCompositionExtensions
{
    /// <summary>
    /// Helper for fluent predicate composition (AND logic).
    /// </summary>
    public static Expression<Func<T, bool>> AndIf<T>(this Expression<Func<T, bool>> predicate, bool condition, Expression<Func<T, bool>> additionalPredicate)
    {
        if (!condition) return predicate;
        var param = Expression.Parameter(typeof(T));
        var body = Expression.AndAlso(
            Expression.Invoke(predicate, param),
            Expression.Invoke(additionalPredicate, param)
        );
        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    /// <summary>
    /// Helper for fluent predicate composition (AND logic) with nullable string.
    /// </summary>
    public static Expression<Func<T, bool>> AndIfString<T>(this Expression<Func<T, bool>> predicate, string? value, Expression<Func<T, bool>> additionalPredicate) =>
        predicate.AndIf(!string.IsNullOrWhiteSpace(value), additionalPredicate);

    /// <summary>
    /// Helper for fluent predicate composition (AND logic) with nullable string.
    /// </summary>
    public static Expression<Func<T, bool>> AndIfStringPair<T>(this Expression<Func<T, bool>> predicate, string? value1, string? value2, Expression<Func<T, bool>> additionalPredicate) =>
        predicate.AndIf(!string.IsNullOrWhiteSpace(value1) && !string.IsNullOrWhiteSpace(value2), additionalPredicate);
}