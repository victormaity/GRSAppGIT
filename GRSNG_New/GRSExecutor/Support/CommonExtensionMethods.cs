using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web;

namespace GRSExecutor.Support
{
    public static class CommonExtensionMethods
    {
        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            var props =
                TypeDescriptor.GetProperties(typeof(T));
            var table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                var prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            var values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }
    }
}