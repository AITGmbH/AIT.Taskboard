using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIT.Taskboard.Interface
{
    public interface IDataObjectFactory
    {
        T CreateObject<T>() where T:class;
    }
}
