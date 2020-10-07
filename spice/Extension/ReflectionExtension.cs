using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace spice.Extension
{
    public static class ReflectionExtension
    {
        public static string GetPropertyValue<T>(this T item ,string property)
        {
            return item.GetType().GetProperty(property).GetValue(item, null).ToString();
        }
    }
}
