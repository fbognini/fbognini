using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace fbognini.Core.Extensions
{
    public static class DataTableExtensions
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
                    foreach (var innerItem in innerList)
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
            where T: notnull
        {
            DataTable dataTable = new DataTable();

            Type type = typeof(T);
            var properties = type.GetProperties().Where(x => !(x.PropertyType.GenericTypeArguments.Any() && x.PropertyType.GetGenericTypeDefinition() == typeof(List<>))).ToArray();

            foreach (PropertyInfo info in properties)
            {
                dataTable.Columns.Add(new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType));
            }

            foreach (var entity in list)
            {
                object[] values = new object[properties.Length];

                for (int i = 0; i < properties.Length; i++)
                {
                    values[i] = entity.GetPropertyValue(properties[i].Name);
                }

                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        public static int PopulateColumns(ref DataTable dataTable, IEnumerable<PropertyInfo> properties, int count = 0)
        {
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
    }
}
