using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using AIT.Taskboard.Interface;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace AIT.Taskboard.Interface
{
    public class BacklogChildren : ICustomTypeDescriptor, IBacklogChildren
    {
        public BacklogChildren(WorkItem backlog, List<WorkItem> children, List<ICustomState> states)
        {
            Backlog = backlog;
            Children = children;
            States = states;
        }

        public WorkItem Backlog { get; private set; }

        public List<WorkItem> Children { get; private set; }

        public List<ICustomState> States { get; private set; }

        #region ICustomTypeDescriptor implementation

        public PropertyDescriptorCollection GetProperties()
        {
            PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(this, true);

            int totalPropertyCount = pdc.Count + States.Count;
            var array = new PropertyDescriptor[totalPropertyCount];

            pdc.CopyTo(array, 0);

            int index = pdc.Count;

            foreach (CustomState state in States)
            {
                PropertyDescriptor pd = new CustomStatePropertyDescriptor("CustomState" + States.IndexOf(state).ToString(), state);
                array[index++] = pd;
            }

            var newProperties = new PropertyDescriptorCollection(array);
            return newProperties;
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this);
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return TypeDescriptor.GetProperties(this, attributes);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion
    }
}
