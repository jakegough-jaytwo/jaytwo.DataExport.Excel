using System;
using System.Linq.Expressions;

namespace jaytwo.StreamingExcelExport;

public static class ValidationHelper
{
    public static TValue EnsureNotNull<T, TValue>(T target, Expression<Func<T, TValue>> propertyExpression, bool validate)
    {
        var propertyName = GetPropertyName(propertyExpression);
        var compiled = propertyExpression.Compile();
        var value = compiled(target);

        if (value == null && validate)
        {
            throw new InvalidOperationException($"Property '{propertyName}' on {typeof(T).Name} must not be null.");
        }

        return value;
    }

    private static string GetPropertyName<T, TValue>(Expression<Func<T, TValue>> expression)
    {
        if (expression.Body is MemberExpression member)
        {
            return member.Member.Name;
        }

        if (expression.Body is UnaryExpression unary && unary.Operand is MemberExpression memberExpr)
        {
            return memberExpr.Member.Name;
        }

        throw new ArgumentException("Invalid expression: must be a property access.", nameof(expression));
    }
}
