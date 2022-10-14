using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Core.Utilities
{
    public static class ExtensionMethods
    {
        public static List<T> ToList<T>(this DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }

        public static Collection<TOut> ToCollection<T, TOut>(this List<T> items)
            //where T : struct
            //where TOut : struct
        {
            Collection<TOut> collection = new Collection<TOut>();

            for (int i = 0; i < items.Count; i++)
            {
                collection.Add((TOut)Convert.ChangeType(items[i], typeof(TOut)));
            }

            return collection;
        }

        public static Collection<T> ToCollection<T>(this List<T> items)
            //where T : struct
        {
            return items.ToCollection<T, T>();
        }

        private static T GetItem<T>(this DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }

        public static DataTable CreateNestedDataTable<TOuter, TInner>(this IEnumerable<TOuter> list, string innerListPropertyName)
        {
            PropertyInfo[] outerProperties = typeof(TOuter).GetProperties().Where(pi => pi.Name != innerListPropertyName).ToArray();
            PropertyInfo[] innerProperties = typeof(TInner).GetProperties();
            MethodInfo innerListGetter = typeof(TOuter).GetProperty(innerListPropertyName).GetMethod;

            // set up columns
            DataTable table = new DataTable();
            foreach (PropertyInfo pi in outerProperties)
                table.Columns.Add(pi.Name, Nullable.GetUnderlyingType(pi.PropertyType) ?? pi.PropertyType);
            foreach (PropertyInfo pi in innerProperties)
                table.Columns.Add(pi.Name, Nullable.GetUnderlyingType(pi.PropertyType) ?? pi.PropertyType);

            // iterate through outer items
            foreach (TOuter outerItem in list)
            {
                var innerList = innerListGetter.Invoke(outerItem, null) as IEnumerable<TInner>;
                if (innerList == null || innerList.Count() == 0)
                {
                    // outer item has no inner items
                    DataRow row = table.NewRow();
                    foreach (PropertyInfo pi in outerProperties)
                        row[pi.Name] = pi.GetValue(outerItem) ?? DBNull.Value;
                    table.Rows.Add(row);
                }
                else
                {
                    // iterate through inner items
                    foreach (object innerItem in innerList)
                    {
                        DataRow row = table.NewRow();
                        foreach (PropertyInfo pi in outerProperties)
                            row[pi.Name] = pi.GetValue(outerItem) ?? DBNull.Value;
                        foreach (PropertyInfo pi in innerProperties)
                            row[pi.Name] = pi.GetValue(innerItem) ?? DBNull.Value;
                        table.Rows.Add(row);
                    }
                }
            }

            return table;
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> list)
        {

            DataTable dataTable = new DataTable();

            Type type = typeof(T);
            var properties = type.GetProperties().Where(x => !(x.PropertyType.GenericTypeArguments.Any() && x.PropertyType.GetGenericTypeDefinition() == typeof(List<>))).ToArray();

            //int count = PopulateColumns(ref dataTable, properties);

            foreach (PropertyInfo info in properties)
            {
                dataTable.Columns.Add(new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType));
            }

            foreach (T entity in list)
            {
                object[] values = new object[properties.Length];

                for (int i = 0; i < properties.Length; i++)
                {
                    values[i] = GetPropValue(entity, properties[i].Name);
                    //values[i] = properties[i].GetValue(entity);
                }

                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        public static int PopulateColumns(ref DataTable dataTable, IEnumerable<PropertyInfo> properties, int count = 0)
        {
            //Type type = typeof(T);
            //var properties = type.GetProperties().Where(info => !(info.PropertyType.GenericTypeArguments.Any() && info.PropertyType.GetGenericTypeDefinition() == typeof(List<>))).ToArray();

            count = properties.Count();

            foreach (PropertyInfo info in properties)
            {
                if (info.PropertyType.GenericTypeArguments.Any() && info.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var nestedType = info.PropertyType;
                    var nestedProperties = nestedType.GetProperties();
                    count += PopulateColumns(ref dataTable, nestedProperties);
                }

                dataTable.Columns.Add(new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType));
            }

            return count;
        }

        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        /// <summary>
        /// Executes a non-query asynchronously (with transaction).
        /// </summary>
        /// <param name="command"></param>
        /// <param name="ct"></param>
        /// <param name="manageConnection"></param>
        /// <returns></returns>
        public static async Task<int> ExecuteStoredNonQueryWithTransactionAsync(
            this DbCommand command
            , CancellationToken ct = default
            , bool manageConnection = true
            , DbTransaction transaction = null)
        {
            var numberOfRecordsAffected = -1;

            using (command)
            {
                if (transaction != null)
                    command.Transaction = transaction;

                if (command.Connection.State == System.Data.ConnectionState.Closed)
                {
                    await command.Connection.OpenAsync(ct).ConfigureAwait(false);
                }

                try
                {
                    numberOfRecordsAffected = await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }
                finally
                {
                    if (manageConnection)
                    {
                        command.Connection.Close();
                    }
                }
            }

            return numberOfRecordsAffected;
        }

        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int blockSize)
        {
            while (source.Any())
            {
                yield return source.Take(blockSize);
                source = source.Skip(blockSize);
            }
        }

        public static IEnumerable<List<T>> SplitList<T>(this List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }

        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
        {
            return items.GroupBy(property).Select(x => x.First());
        }

        public static string GetDescription(this Enum GenericEnum)
        {
            Type genericEnumType = GenericEnum.GetType();
            MemberInfo[] memberInfo = genericEnumType.GetMember(GenericEnum.ToString());
            if ((memberInfo != null && memberInfo.Length > 0))
            {
                var _Attribs = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                if ((_Attribs != null && _Attribs.Count() > 0))
                {
                    return ((System.ComponentModel.DescriptionAttribute)_Attribs.ElementAt(0)).Description;
                }
            }
            return GenericEnum.ToString();
        }
    }
}
