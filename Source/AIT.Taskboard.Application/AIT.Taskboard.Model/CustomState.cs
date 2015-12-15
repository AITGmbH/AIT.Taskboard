using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using System.Xml.Serialization;
using AIT.Taskboard.Interface;

namespace AIT.Taskboard.Interface
{
    [Serializable]
    public class CustomState : ICloneable, ICustomState
    {
        private byte a, r, g, b;

        public CustomState()
        {
            WorkItemStates = new List<string>();
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public Color Color
        {
            get { return Color.FromArgb(a,r,g,b); }
            set
            {
                a = value.A;
                r = value.R;
                g = value.G;
                b = value.B;
                OnPropertyChanged("Color");
            }
        }
        [XmlArray]
        public List<string> WorkItemStates { get; private set; }
        public object Clone()
        {
            var clone = new CustomState
                            {
                                Color = Color,
                                Name = Name,
                                WorkItemStates = new List<string>(WorkItemStates.ToArray())
                            };
            return clone;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged (string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        
    }
}