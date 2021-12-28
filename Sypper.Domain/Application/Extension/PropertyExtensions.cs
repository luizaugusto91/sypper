using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sypper.Domain.Application.Extension
{
    public static class PropertyExtensions
    {
        public static TValue GetAttributValue<TAttribute, TValue>(this PropertyInfo prop, Func<TAttribute, TValue> value) where TAttribute : Attribute
        {
            var att = prop.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;

            if (att != null)
            {
                return value(att);
            }

            return default(TValue);
        }


        private static dynamic GetPropertyValues<T>(this T obj)
        {
            dynamic result = default(dynamic);

            Type t = obj.GetType();

            PropertyInfo[] props = t.GetProperties();
            
            foreach (var prop in props)
            {
                if (prop.GetIndexParameters().Length == 0)
                {
                    result = prop.GetValue(obj);
                }
                else
                {
                    result = default(dynamic);
                }
            }

            return result;
        }
    }
}
