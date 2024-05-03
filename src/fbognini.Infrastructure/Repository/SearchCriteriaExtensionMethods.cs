using fbognini.Core.Domain.Query;
using fbognini.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace fbognini.Infrastructure.Repository
{
    public static class SearchCriteriaExtensionMethods
    {
        public static IQueryable<T> QueryArgs<T>(this IQueryable<T> query, SelectArgs<T>? criteria)
            where T : class
        {
            criteria ??= new SelectArgs<T>();

            if (!criteria.Track)
            {
                query = query.AsNoTracking();
            }

            if (criteria.IgnoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            if (criteria.IgnoreAutoIncludes)
            {
                query = query.IgnoreAutoIncludes();
            }

            query = query.IncludeViews(criteria);

            return query;
        }

        public static IQueryable<T> IncludeViews<T>(this IQueryable<T> list, IHasViews<T> includes)
            where T : class
        {
            if (includes == null)
            {
                return list;
            }

            foreach (var view in includes.Includes)
            {
                var path = view.GetPropertyPath(true); // it must works even with x.OrderLines.First().Product.Retailer.DisplayName
                list = list.Include(path);
            }

            foreach (var view in includes.IncludeStrings)
            {
                list = list.Include(view);
            }

            return list;
        }

        public static IQueryable<T> IncludeViews<T>(this IQueryable<T> list, List<Expression<Func<T, object>>> includes)
            where T : class
        {
            if (includes == null)
            {
                return list;
            }

            foreach (var view in includes)
            {
                list = list.Include(view);
            }

            return list;
        }


        public static string GetArgsKey<TEntity>(this IArgs args)
        {
            var builder = new StringBuilder();
            builder.Append(typeof(TEntity).Name);

            if (args is IHasViews<TEntity> hasViews)
            {
                builder.Append($"|v:");
                builder.Append(string.Join(',', hasViews.AllIncludes.OrderBy(x => x)));
            }

            if (args is IHasFilter<TEntity> hasFilter)
            {
                var filter = hasFilter.ResolveFilter();
                var key = filter.GenerateHumanReadableKey();
                builder.Append($"|f:{key}");
            }

            if (args is IHasSearch<TEntity> hasSearch)
            {
                if (hasSearch.Search != null && hasSearch.Search.AllFields.Count != 0)
                {
                    builder.Append($"|q:");
                    builder.Append($"q={hasSearch.Search.Keyword}&t=");
                    builder.Append(string.Join(',', hasSearch.Search.AllFields.OrderBy(x => x)));
                }
            }

            if (args is IHasSorting<TEntity> hasSorting && hasSorting.Sorting.Any())
            {
                builder.Append($"|s:");
                builder.Append(string.Join(',', hasSorting.Sorting.OrderBy(x => x.Key).Select(s => $"{s.Key}x{s.Value}")));
            }

            return builder.ToString();
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
}
