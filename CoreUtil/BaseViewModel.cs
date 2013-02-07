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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Core.Mvvm
{
    public class BaseViewModel<T> : NotifyPropertyChanged<T> where T : BaseViewModel<T>
    {
        private int loadingCounter = 0;
        public bool IsLoading
        {
            get { return loadingCounter > 0; }
            set
            {
                if (value)
                    loadingCounter++;
                else if (loadingCounter > 0)
                    loadingCounter--;

#if NETFX_CORE
                RaisePropertyChanged();
#else
                RaisePropertyChanged("IsLoading");
#endif
            }
        }
    }
}
