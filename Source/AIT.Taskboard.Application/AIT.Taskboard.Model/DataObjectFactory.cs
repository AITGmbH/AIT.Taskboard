using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using AIT.Taskboard.Interface;

namespace AIT.Taskboard.Model
{
    [Export (typeof(IDataObjectFactory))]
    public class DataObjectFactory : IDataObjectFactory
    {
        #region Implementation of IDataObjectFactory

        public T CreateObject<T>() where T:class
        {
            if (typeof(T) == typeof (ICustomState))
            {
                return new CustomState () as T ;
            }
            if (typeof(T) == typeof(ILoginData))
            {
                return new LoginData() as T;
            }
            return null;
        }

        #endregion
    }
}
