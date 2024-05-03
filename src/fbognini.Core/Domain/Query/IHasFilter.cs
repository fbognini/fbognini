using fbognini.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace fbognini.Core.Domain.Query;

public interface IHasFilter<TEntity>
{
    Expression<Func<TEntity, bool>> ResolveFilter();
}


public static class HasFilterExtensionMethods
{
    public static string GetArgsKey<T>(this IHasFilter<T> filter)
    {
        return $"|f:{filter.GenerateHumanReadableKey()}";
    }

    public static List<KeyValuePair<string, object?>> GetArgsKeyAsDictionary<T>(this IHasFilter<T> filter)
    {
        var names = filter.GetType()
                .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
                .Select(pi => pi.Name).ToList();

        var keys = new Dictionary<string, object>();
        foreach (var name in names)
        {
            var value = filter.GetPropertyValue(name);
            if (value is not null)
            {
                keys.Add(name, value);
            }
        }

        return new List<KeyValuePair<string, object?>>()
        {
            new("Filters", keys)
        };
    }

    public static string GenerateHumanReadableKey<T>(this IHasFilter<T> filter)
    {
        return filter.ResolveFilter().GenerateHumanReadableKey();
    }

    public static string GenerateHumanReadableKey<T>(this Expression<Func<T, bool>> expression)
    {
        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        WalkExpression(dictionary, expression);
        string text = expression.ToString();
        foreach (ParameterExpression parameter in expression.Parameters)
        {
            string name = parameter.Name;
            string typeName = parameter.Type.Name;
            text = text.Replace(name + ".", typeName + ".");
        }

        foreach (KeyValuePair<string, string> item in dictionary)
        {
            text = text.Replace(item.Key, item.Value);
        }

        text = text.Replace(" ", string.Empty);
        return text;
    }

    private static void WalkExpression(Dictionary<string, string> replacements, Expression expression)
    {
        switch (expression.NodeType)
        {
            case ExpressionType.MemberAccess:
                {
                    string text = expression.ToString();
                    if (text.Contains("value(") && !replacements.ContainsKey(text))
                    {
                        var obj = Expression.Lambda(expression).Compile().DynamicInvoke();
                        if (obj != null)
                        {
                            replacements.Add(text, obj.ToString());
                        }
                    }

                    break;
                }
            case ExpressionType.AndAlso:
            case ExpressionType.Equal:
            case ExpressionType.GreaterThan:
            case ExpressionType.GreaterThanOrEqual:
            case ExpressionType.LessThan:
            case ExpressionType.LessThanOrEqual:
            case ExpressionType.OrElse:
                {
                    BinaryExpression binaryExpression = (expression as BinaryExpression)!;
                    WalkExpression(replacements, binaryExpression.Left);
                    WalkExpression(replacements, binaryExpression.Right);
                    break;
                }
            case ExpressionType.Call:
                {
                    MethodCallExpression methodCallExpression = (expression as MethodCallExpression)!;
                    foreach (Expression argument in methodCallExpression.Arguments)
                    {
                        WalkExpression(replacements, argument);
                    }

                    break;
                }
            case ExpressionType.Lambda:
                {
                    LambdaExpression lambdaExpression = (expression as LambdaExpression)!;
                    WalkExpression(replacements, lambdaExpression.Body);
                    break;
                }
        }
    }
}