using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace AIT.Taskboard.Interface
{
    public interface ICustomState : INotifyPropertyChanged, ICloneable
    {
        string Name { get; set; }
        Color Color { get; set; }
        List<string> WorkItemStates { get; }
    }
}