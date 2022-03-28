using System.Data;
using System.Reflection;

namespace Sypper.Infra.Connection
{
    public static class EntityDataManipulate
    {
        #region Manipulate
        public static T ConvertDataRow<T>(DataTable dt)
        {
            DataRow row = dt.Rows[0];
            return GetItemDefault<T>(row);
        }

        public static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItemDefault<T>(row);
                data.Add(item);
            }
            return data;
        }

        public static T GetItemDefault<T>(DataRow dr)
        {
            T obj = Activator.CreateInstance<T>();
            List<PropertyInfo> props = typeof(T).GetProperties().ToList();

            foreach (DataColumn column in dr.Table.Columns)
            {
                var prop = props.Where(r => r.Name == column.ColumnName).FirstOrDefault();
                if (prop != null)
                {
                    Type field = prop.PropertyType;
                    var value = dr[column.ColumnName];
                    if (value.GetType().Name == "DBNull")
                    {
                        value = default;
                    }

                    prop.SetValue(obj, value, null);
                }
            }

            return (T)obj;
        }
        #endregion
    }
}
