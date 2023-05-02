﻿
using fbognini.Core.Data.Pagination;
using fbognini.Core.Entities;
using fbognini.Core.Utilities;
using LinqKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace fbognini.Core.Data
{
    public static class SearchCriteriaExtensionMethods
    {
        public static IQueryable<T> QuerySelect<T>(this IQueryable<T> query, SelectCriteria<T> criteria = null)
            where T : class
        {
            if (criteria == null)
            {
                return query;
            }

            query = query
                    .Where(criteria.ResolveFilter().Expand())
                    .AdvancedSearch(criteria)
                    .OrderByDynamic(criteria);

            if (criteria.QueryProcessing != null)
            {
                query = criteria.QueryProcessing(query);
            }

            return query;
        }

        public static IQueryable<T> QuerySearch<T>(this IQueryable<T> query, SelectCriteria<T> criteria, out PaginationResult pagination)
            where T : class 
            => query.QueryPagination(criteria, out pagination);

        public static IQueryable<T> QuerySearch<T>(this IQueryable<T> query, SearchCriteria<T> criteria, out PaginationResult pagination)
            where T : class, IAuditableEntity 
            => query.QueryPagination(criteria, out pagination);

        public static IOrderedQueryable<TEntity> OrderBy<TEntity>(
            this IQueryable<TEntity> source
            , string property
            , bool desc)
        {
            var param = Expression.Parameter(typeof(TEntity), "p");
            var parts = property.Split('.');

            Expression parent = parts.Aggregate<string, Expression>(param, Expression.Property);
            Expression conversion = Expression.Convert(parent, typeof(object));

            var lambda = Expression.Lambda<Func<TEntity, object>>(conversion, param);

            if (desc)
            {
                return source.OrderByDescending(lambda);
            }
            else
            {
                return source.OrderBy(lambda);
            }
        }

        public static IOrderedQueryable<TEntity> ThenBy<TEntity>(
            this IOrderedQueryable<TEntity> source
            , string property
            , bool desc)
        {
            var param = Expression.Parameter(typeof(TEntity), "p");
            var parts = property.Split('.');

            Expression parent = parts.Aggregate<string, Expression>(param, Expression.Property);
            Expression conversion = Expression.Convert(parent, typeof(object));

            var lambda = Expression.Lambda<Func<TEntity, object>>(conversion, param);

            if (desc)
            {
                return source.ThenByDescending(lambda);
            }
            else
            {
                return source.ThenBy(lambda);
            }
        }

        public static IQueryable<T> OrderByDynamic<T>(
            this IQueryable<T> query
            , string orderByMember
            , SortingDirection direction)
        {
            var property = typeof(T).GetProperty(orderByMember, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);

            query = direction == SortingDirection.ASCENDING ?
                query.OrderBy(x => property.GetValue(x, null)) :
                query.OrderByDescending(x => property.GetValue(x, null));

            return query;
        }

        public static IQueryable<T> OrderByDynamic<T>(
            this IQueryable<T> query
            , IHasSorting searchCriteria)
        {
            if (searchCriteria == null || searchCriteria.Sorting == null || searchCriteria.Sorting.Count == 0)
                return query;

            var orderedquery = query.OrderBy(searchCriteria.Sorting[0].Key, searchCriteria.Sorting[0].Value == SortingDirection.DESCENDING);
            for (int i = 1; i < searchCriteria.Sorting.Count(); i++)
            {
                orderedquery = orderedquery.ThenBy(searchCriteria.Sorting[i].Key, searchCriteria.Sorting[i].Value == SortingDirection.DESCENDING);
            }

            return orderedquery;
        }

        public static IQueryable<T> AdvancedSearch<T>(this IQueryable<T> query, IHasSearch<T> searchCriteria)
            where T: class
        {
            var predicate = GetSearchPredicate(searchCriteria);
            if (predicate == null)
                return query;

            return query.Where(predicate);
        }

        public static Expression<Func<T, bool>> BuildPredicate<T>(
            this List<Expression<Func<T, bool>>> expressions
            , LogicalOperator logicalOperator)
        {
            if (expressions == null || expressions.Count == 0)
            {
                return PredicateBuilder.New<T>(true);
            }

            Expression<Func<T, bool>> expression = (logicalOperator == LogicalOperator.OR) ? PredicateBuilder.New<T>(false) : PredicateBuilder.New<T>(true);
            foreach (Expression<System.Func<T, bool>> current in expressions)
            {
                if (logicalOperator == LogicalOperator.OR)
                {
                    expression = expression.Or(current);
                }
                else
                {
                    expression = expression.And(current);
                }
            }
            return expression;
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
                if (hasSearch.Search != null && hasSearch.Search.AllFields.Any())
                {
                    builder.Append($"|q:");
                    builder.Append($"q={hasSearch.Search.Keyword}&t=");
                    builder.Append(string.Join(',', hasSearch.Search.AllFields.OrderBy(x => x)));
                }
            }

            if (args is IHasSorting hasSorting && hasSorting.Sorting.Any())
            {
                builder.Append($"|s:");
                builder.Append(string.Join(',', hasSorting.Sorting.OrderBy(x => x.Key).Select(s => $"{s.Key}x{s.Value}")));
            }

            if (args is IHasOffset hasOffset && hasOffset.PageSize.HasValue)
            {
                builder.Append($"|p:");
                builder.Append($"n={hasOffset.PageNumber}&s={hasOffset.PageSize}");

                if (args is IHasSinceOffset hasSinceOffset && hasSinceOffset.Since.HasValue)
                {
                    builder.Append($"&since={hasSinceOffset.Since}&after={hasSinceOffset.AfterId}");
                }
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
                            object obj = Expression.Lambda(expression).Compile().DynamicInvoke();
                            if (obj != null)
                            {
                                string text2 = obj.ToString();
                                replacements.Add(text, text2.ToString());
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
                        BinaryExpression binaryExpression = expression as BinaryExpression;
                        WalkExpression(replacements, binaryExpression.Left);
                        WalkExpression(replacements, binaryExpression.Right);
                        break;
                    }
                case ExpressionType.Call:
                    {
                        MethodCallExpression methodCallExpression = expression as MethodCallExpression;
                        foreach (Expression argument in methodCallExpression.Arguments)
                        {
                            WalkExpression(replacements, argument);
                        }

                        break;
                    }
                case ExpressionType.Lambda:
                    {
                        LambdaExpression lambdaExpression = expression as LambdaExpression;
                        WalkExpression(replacements, lambdaExpression.Body);
                        break;
                    }
            }
        }

        private static Expression<Func<T, bool>> GetSearchPredicate<T>(IHasSearch<T> searchCriteria)
        {
            if (searchCriteria.Search == null || searchCriteria.Search.Keyword == null)
                return null;

            var predicate = PredicateBuilder.New<T>(false);

            foreach (var field in searchCriteria.Search.Fields)
            {
                var names = field.GetPropertyNames(true).ToArray();
                LambdaExpression lambda = InnerSearch(typeof(T), names, searchCriteria.Search.Keyword);
                predicate = predicate.Or((Expression<Func<T, bool>>)lambda);
            }

            foreach (var field in searchCriteria.Search.FieldStrings)
            {
                var names = field.Split('.');
                LambdaExpression lambda = InnerSearch(typeof(T), names, searchCriteria.Search.Keyword);
                predicate = predicate.Or((Expression<Func<T, bool>>)lambda);
            }

            return predicate;

            static LambdaExpression InnerSearch(Type type, string[] names, string keyword, int i = 0, ParameterExpression parameter = null, Expression property = null)
            {
                if (parameter == null)
                {
                    parameter = Expression.Parameter(type, $"x{i}");
                }

                if (property == null)
                {
                    property = parameter;
                }

                if (i == names.Length)
                {
                    var lambda = StringContains(keyword, parameter, property);
                    return lambda;
                }

                property = Expression.PropertyOrField(property, names[i]);
                if (property.Type != typeof(String) && typeof(IEnumerable).IsAssignableFrom(property.Type))
                {
                    var innertype = property.Type.GetGenericArguments().Single();
                    var innerLambda = InnerSearch(innertype, names, keyword, i + 1);

                    var propertyAsObject = Expression.Convert(property, typeof(object));
                    var nullCheck = Expression.NotEqual(propertyAsObject, Expression.Constant(null, typeof(object)));
                    var any = Expression.Call(typeof(Enumerable), "Any", new[] { innertype }, property, innerLambda);

                    var lambda = Expression.Lambda(any, parameter);

                    return lambda;

                }
                else
                {
                    return InnerSearch(type, names, keyword, i + 1, parameter, property);
                }
            }

            static LambdaExpression StringContains(string keyword, ParameterExpression parameter, Expression property)
            {
                var propertyAsObject = Expression.Convert(property, typeof(object));
                var nullCheck = Expression.NotEqual(propertyAsObject, Expression.Constant(null, typeof(object)));
                var propertyAsString = Expression.Call(property, "ToString", null, null);
                var keywordExpression = Expression.Constant(keyword);
                var contains = property.Type == typeof(string) ? Expression.Call(property, "Contains", null, keywordExpression) : Expression.Call(propertyAsString, "Contains", null, keywordExpression);
                var lambda = Expression.Lambda(Expression.AndAlso(nullCheck, contains), parameter);
                return lambda;
            }
        }

    }
}
