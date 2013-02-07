﻿// 
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
using Core.Extension;
using Windows.UI.Xaml.Data; 
using System.Reflection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
#else
using System.ComponentModel;
#endif

namespace Core.Mvvm
{
    [DataContract]
    public class NotifyPropertyChanged<TClass> : NotifyPropertyChanged
    {
        public bool SetProperty<T>(Expression<Func<TClass, T>> expression, ref T oldValue, T newValue)
        {
            return SetProperty(((MemberExpression)expression.Body).Member.Name, ref oldValue, newValue);
        }

        public void RaisePropertyChanged<T>(Expression<Func<TClass, T>> expression)
        {
            RaisePropertyChanged(((MemberExpression)expression.Body).Member.Name);
        }

        public void SetDependency<TProp1, TProp2>(Expression<Func<TClass, TProp1>> propertyName,
            Expression<Func<TClass, TProp2>> dependentPropertyName)
        {
            SetDependency(((MemberExpression)propertyName.Body).Member.Name,
                ((MemberExpression)dependentPropertyName.Body).Member.Name);
        }
    }

    [DataContract]
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private List<KeyValuePair<string, string>> dependencies;

        public bool SetProperty<TClass, TProp>(Expression<Func<TClass, TProp>> expression, ref TProp oldValue, TProp newValue)
        {
            return SetProperty(((MemberExpression)expression.Body).Member.Name, ref oldValue, newValue);
        }

        public bool SetProperty<T>(String propertyName, ref T oldValue, T newValue)
        {
            if (Equals(oldValue, newValue))
                return false;

            oldValue = newValue;
            RaisePropertyChanged(propertyName);
            return true;
        }

        public void SetDependency(string propertyName, string dependentPropertyName)
        {
            if (dependencies == null)
                dependencies = new List<KeyValuePair<string, string>>();

            if (dependencies.Any(d => d.Key == propertyName && d.Value == dependentPropertyName))
                return;

            dependencies.Add(new KeyValuePair<string, string>(propertyName, dependentPropertyName));
        }

        public void SetDependency<TClass, TProp1, TProp2>(Expression<Func<TClass, TProp1>> propertyName,
            Expression<Func<TClass, TProp2>> dependentPropertyName)
        {
            SetDependency(((MemberExpression)propertyName.Body).Member.Name,
                ((MemberExpression)dependentPropertyName.Body).Member.Name);
        }

        public void RaisePropertyChanged<TClass, TProp>(Expression<Func<TClass, TProp>> expression)
        {
            RaisePropertyChanged(((MemberExpression)expression.Body).Member.Name);
        }

#if NETFX_CORE
        public bool SetProperty<T>(ref T oldValue, T newValue, [CallerMemberName] String propertyName = null)
		{
			return SetProperty(propertyName, ref oldValue, newValue); 
		}

		public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
#else
        public void RaisePropertyChanged(string propertyName = null)
#endif
        {
#if DEBUG
#if NETFX_CORE
            if (GetType().GetTypeInfo().GetProperty(propertyName) == null)
						throw new ArgumentException("propertyName");
#else
            if (GetType().GetProperty(propertyName) == null)
                throw new ArgumentException("propertyName");
#endif
#endif

            var copy = PropertyChanged; // avoid concurrent changes
            if (copy != null)
                copy(this, new PropertyChangedEventArgs(propertyName));

            if (dependencies != null)
            {
                foreach (var d in dependencies.Where(d => d.Key == propertyName))
                    RaisePropertyChanged(d.Value);
            }
        }
    }
}
