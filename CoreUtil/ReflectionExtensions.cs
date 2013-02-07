// 
//  This code has been extracted from the following resource
//  https://mytoolkit.svn.codeplex.com/svn/Shared/
//  The conditional define WINRT has been replaced with NETFX_CORE which
//  is defined by default when creating a WINRT library
//
//  If you take it for your own usage, please refer to the original copyright      
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Core.Extension
{
    public static class ReflectionExtensions
    {
        public static string GetName(this Type type)
        {
            var fullName = type.FullName;
            var index = fullName.LastIndexOf('.');
            if (index != -1)
                return fullName.Substring(index + 1);
            return fullName;
        }

        public static object CreateGenericObject(this Type type, Type innerType, params object[] args)
        {
            Type specificType = type.MakeGenericType(new[] { innerType });
            return Activator.CreateInstance(specificType, args);
        }

#if NETFX_CORE
		public static void Clone(this object source, object target)
		{
			var targetType = target.GetType().GetTypeInfo();
			foreach (var p in source.GetType().GetTypeInfo().DeclaredProperties)
			{
				var tp = targetType.GetDeclaredProperty(p.Name);
				if (tp != null && p.CanWrite)
				{
					var value = p.GetValue(source, null);
					tp.SetValue(target, value, null);
				}
			}
		}

		public static IEnumerable<PropertyInfo> GetAllProperties(this TypeInfo type)
		{
			var list = type.DeclaredProperties.ToList();

			var subtype = type.BaseType;
			if (subtype != null)
				list.AddRange(subtype.GetTypeInfo().GetAllProperties());

			foreach (var i in type.ImplementedInterfaces)
				list.AddRange(i.GetTypeInfo().GetAllProperties());

			return list.ToArray();
		}

		public static PropertyInfo GetProperty(this TypeInfo type, string name)
		{
			var property = type.GetDeclaredProperty(name);
			if (property != null)
				return property;

			foreach (var i in type.ImplementedInterfaces)
			{
				property = i.GetTypeInfo().GetProperty(name);
				if (property != null)
					return property;
			}

			var subtype = type.BaseType;
			if (subtype != null)
			{
				property = subtype.GetTypeInfo().GetProperty(name);
				if (property != null)
					return property;
			}
			return null; 
		}
#else
        public static void Clone(this object source, object target)
        {
            var targetType = target.GetType();
            foreach (var p in source.GetType().GetProperties())
            {
                var tp = targetType.GetProperty(p.Name);
                if (tp != null && p.CanWrite)
                {
                    var value = p.GetValue(source, null);
                    tp.SetValue(target, value, null);
                }
            }
        }
#endif

    }
}
