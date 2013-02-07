// 
//  This code has been extracted from the following resource
//  https://mytoolkit.svn.codeplex.com/svn/Shared/
//  The conditional define WINRT has been replaced with NETFX_CORE which
//  is defined by default when creating a WINRT library
//
//  If you take it for your own usage, please refer to the original copyright      
//

using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Serialization;

#if NETFX_CORE
using System.Reflection;
using Windows.UI.Xaml.Data;
using System.ComponentModel;
#else
using System.ComponentModel;
#endif


namespace MyToolkit.MVVM
{
    public static class PropertyChangedEventArgsExtensions
    {
        public static bool IsProperty<TU>(this PropertyChangedEventArgs args, string propertyName)
        {
#if DEBUG
#if NETFX_CORE
			if (typeof(TU).GetTypeInfo().DeclaredProperties.Any(p => p.Name == propertyName))
				throw new ArgumentException("propertyName");
#else
            if (typeof(TU).GetProperty(propertyName) == null)
                throw new ArgumentException("propertyName");
#endif
#endif

            return args.PropertyName == propertyName;
        }

        public static bool IsProperty<TU>(this PropertyChangedEventArgs args, Expression<Func<TU, object>> expression)
        {
            var memexp = expression.Body is UnaryExpression ?
                (MemberExpression)((UnaryExpression)expression.Body).Operand : ((MemberExpression)expression.Body);
            return args.PropertyName == memexp.Member.Name;
        }
    }
}
