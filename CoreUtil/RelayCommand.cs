﻿// 
//  This code has been extracted from the following resource
//  https://mytoolkit.svn.codeplex.com/svn/Shared/
//  The conditional define WINRT has been replaced with NETFX_CORE which
//  is defined by default when creating a WINRT library
//
//  If you take it for your own usage, please refer to the original copyright      
//

using System;
using System.Diagnostics;

#if NETFX_CORE
using Windows.UI.Xaml.Input;
using System.Windows.Input;
#else
using System.Windows.Input;
#endif

namespace Core.Mvvm
{
#if NETFX_CORE
	public class RelayCommand : NotifyPropertyChanged, ICommand
#else
    public class RelayCommand : NotifyPropertyChanged<RelayCommand>, ICommand
#endif
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;

        public RelayCommand(Action execute)
            : this(execute, null) { }

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            this.execute = execute;
            this.canExecute = canExecute;
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute;
        }

        public void Execute(object parameter)
        {
            execute();
        }

        public bool CanExecute
        {
            get { return canExecute == null || canExecute(); }
        }

        public void RaiseCanExecuteChanged()
        {
            RaisePropertyChanged("CanExecute");
            var copy = CanExecuteChanged;
            if (copy != null)
                copy(this, new EventArgs());
        }

        public event EventHandler CanExecuteChanged;
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> execute;
        private readonly Predicate<T> canExecute;

        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            this.execute = execute;
            this.canExecute = canExecute;
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            execute((T)parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            var copy = CanExecuteChanged;
            if (copy != null)
                copy(this, new EventArgs());
        }

        public event EventHandler CanExecuteChanged;
    }
}