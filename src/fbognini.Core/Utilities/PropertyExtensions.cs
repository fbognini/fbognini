using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace fbognini.Core.Utilities
{
    public static class PropertyExtensions
    {

        /// <summary>
        /// x => x.OrderLines.First().Product.RetailerId returns Product.RetailerId if ignoreMethods = false, otherwise OrderLines.Product.RetailerId
        /// </summary>
        public static string GetPropertyPath<T>(this Expression<Func<T, object>> expression, bool ignoreMethods = false)
        {
            return string.Join(".", GetPropertyNames(expression, ignoreMethods));
        }

        /// <summary>
        /// x => x.OrderLines.First().Product.RetailerId returns Product.RetailerId if ignoreMethods = false, otherwise OrderLines.Product.RetailerId
        /// </summary>
        public static IEnumerable<string> GetPropertyNames<T>(this Expression<Func<T, object>> expression, bool ignoreMethods = false)
        {
            var body = expression.Body as MemberExpression;

            if (body == null)
            {
                body = ((UnaryExpression)expression.Body).Operand as MemberExpression;
            }

            return GetPropertyNames(body, ignoreMethods);
        }


        public static IEnumerable<string> GetPropertyNames(MemberExpression body, bool ignoreMethods)
        {
            var names = new List<string>();

            while (body != null)
            {
                names.Add(body.Member.Name);
                var inner = body.Expression;
                switch (inner.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        body = inner as MemberExpression;
                        break;
                    case ExpressionType.Call:
                        if (ignoreMethods)
                        {
                            var call = inner as MethodCallExpression;
                            body = call.Arguments[0] as MemberExpression;
                        }
                        else
                        {
                            body = null;
                        }
                        break;
                    default:
                        body = null;
                        break;

                }
            }

            names.Reverse();

            return names;
        }


        public static string GetPropertyDisplayName<T>(this Expression<Func<T, object>> propertyExpression)
        {
            var memberInfo = GetPropertyInformation(propertyExpression.Body);
            if (memberInfo == null)
            {
                throw new ArgumentException(
                    "No property reference expression was found.",
                    "propertyExpression");
            }

            return GetDisplayName(memberInfo);
        }

        public static string GetPropertyDisplayName(this Type type, string propertyName)
        {
            var memberInfo = type.GetProperty(propertyName);
            if (memberInfo == null)
            {
                throw new ArgumentException(
                    "No property reference expression was found.", nameof(propertyName));
            }

            return GetDisplayName(memberInfo);
        }

        public static PropertyInfo GetNestedProperty(this Type type, string propertyName, BindingFlags bindingFlags = BindingFlags.Default)
        {
            return type.GetNestedProperty(propertyName.Split('.'), bindingFlags);
        }

        public static PropertyInfo GetNestedProperty(this Type type, IEnumerable<string> propertyNames, BindingFlags bindingFlags = BindingFlags.Default)
        {
            var property = type.GetProperty(propertyNames.First(), bindingFlags);
            propertyNames = propertyNames.Skip(1);
            if (propertyNames.Any())
            {
                return GetNestedProperty(property.GetType(), propertyNames, bindingFlags);
            }

            return property;
        }

        public static T GetAttribute<T>(this MemberInfo member, bool isRequired)
            where T : Attribute
        {
            var attribute = member.GetCustomAttributes(typeof(T), false).SingleOrDefault();

            if (attribute == null && isRequired)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "The {0} attribute must be defined on member {1}",
                        typeof(T).Name,
                        member.Name));
            }

            return (T)attribute;
        }
        
        public static MemberInfo GetPropertyInformation(Expression propertyExpression)
        {
            Debug.Assert(propertyExpression != null, "propertyExpression != null");
            MemberExpression memberExpr = propertyExpression as MemberExpression;
            if (memberExpr == null)
            {
                UnaryExpression unaryExpr = propertyExpression as UnaryExpression;
                if (unaryExpr != null && unaryExpr.NodeType == ExpressionType.Convert)
                {
                    memberExpr = unaryExpr.Operand as MemberExpression;
                }
            }

            if (memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property)
            {
                return memberExpr.Member;
            }

            return null;
        }

        public static bool IsSubClassOfGeneric(this Type child, Type parent)
        {
            if (child == parent)
                return false;

            if (child.IsSubclassOf(parent))
                return true;

            var parameters = parent.GetGenericArguments();
            var isParameterLessGeneric = !(parameters != null && parameters.Length > 0 &&
                ((parameters[0].Attributes & TypeAttributes.BeforeFieldInit) == TypeAttributes.BeforeFieldInit));

            while (child != null && child != typeof(object))
            {
                var cur = GetFullTypeDefinition(child);
                if (parent == cur || (isParameterLessGeneric && cur.GetInterfaces().Select(i => GetFullTypeDefinition(i)).Contains(GetFullTypeDefinition(parent))))
                {
                    return true;
                }
                

                if (!isParameterLessGeneric)
                {
                    if (GetFullTypeDefinition(parent) == cur && !cur.IsInterface)
                    {
                        if (VerifyGenericArguments(GetFullTypeDefinition(parent), cur))
                            if (VerifyGenericArguments(parent, child))
                                return true;
                    }
                    else
                    {
                        foreach (var item in child.GetInterfaces().Where(i => GetFullTypeDefinition(parent) == GetFullTypeDefinition(i)))
                            if (VerifyGenericArguments(parent, item))
                                return true;
                    }
                }

                child = child.BaseType;
            }

            return false;
        }

        public static object GetPropertyValue(this object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        private static Type GetFullTypeDefinition(Type type)
        {
            return type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        }

        private static bool VerifyGenericArguments(Type parent, Type child)
        {
            Type[] childArguments = child.GetGenericArguments();
            Type[] parentArguments = parent.GetGenericArguments();
            if (childArguments.Length == parentArguments.Length)
                for (int i = 0; i < childArguments.Length; i++)
                    if (childArguments[i].Assembly != parentArguments[i].Assembly || childArguments[i].Name != parentArguments[i].Name || childArguments[i].Namespace != parentArguments[i].Namespace)
                        if (!childArguments[i].IsSubclassOf(parentArguments[i]))
                            return false;

            return true;
        }

        private static string GetDisplayName(MemberInfo memberInfo)
        {
            var attr = memberInfo.GetAttribute<DisplayNameAttribute>(false);
            if (attr == null)
            {
                return memberInfo.Name;
            }

            return attr.DisplayName;
        }
    }
}
