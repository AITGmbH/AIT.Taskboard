using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace AIT.Taskboard.Interface
{
    public class CustomStatePropertyDescriptor : PropertyDescriptor
    {
        private readonly CustomState _state;
        private readonly string _name;

        public CustomStatePropertyDescriptor(string name, CustomState state)
            : base(name, null)
        {
            _name = name;
            _state = state;
        }

        public override string Name
        {
            get
            {
                return _name;
            }
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override Type ComponentType
        {
            get { return null; }
        }

        public override object GetValue(object component)
        {
            var backlogChildren = component as BacklogChildren;
            if (backlogChildren != null)
            {
                return backlogChildren.Children.Where(c => _state.WorkItemStates.Contains(c.State));
            }
            return null;
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override Type PropertyType
        {
            get { return typeof(IEnumerable<WorkItem>); }
        }

        public override void ResetValue(object component)
        {

        }

        public override void SetValue(object component, object value)
        {

        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }
}
