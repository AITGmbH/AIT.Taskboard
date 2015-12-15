using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AIT.Taskboard.Application.Helper;
using AIT.Taskboard.ViewModel;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace AIT.Taskboard.Application.Controls
{
    /// <summary>
    /// Interaction logic for NumericTextBox.xaml
    /// </summary>
    public partial class NumericUpDown : UserControl
    {
        /// <summary>
        /// Numeric value
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(NumericUpDown), new PropertyMetadata(new PropertyChangedCallback(ValueChanged)));

        private static RoutedCommand _increaseCommand;
        private static RoutedCommand _decreaseCommand;

        /// <summary>
        /// Gets or sets the current value
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private WorkItem LastWorkItem { get; set; }

        private static void ValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var numUpDown = sender as NumericUpDown;
            var field = numUpDown.DataContext as Field;
            var workItem = field.WorkItem;
            if (e.NewValue != e.OldValue && numUpDown.LastWorkItem != null && numUpDown.LastWorkItem == workItem)
            {
                WindowHelper.RefreshWorkItemCell(WpfHelper.FindVisualParent<TaskboardControl>(sender));
            }
            numUpDown.LastWorkItem = workItem;
        }

        public NumericUpDown()
        {
            InitializeCommands();
            InitializeComponent();
        }

        #region Commands

        /// <summary>
        /// Increase command
        /// </summary>
        public static RoutedCommand IncreaseCommand
        {
            get
            {
                return _increaseCommand;
            }
        }

        /// <summary>
        /// Decrease command
        /// </summary>
        public static RoutedCommand DecreaseCommand
        {
            get
            {
                return _decreaseCommand;
            }
        }

        private static void InitializeCommands()
        {
            _increaseCommand = new RoutedCommand("IncreaseCommand", typeof(NumericUpDown));
            CommandManager.RegisterClassCommandBinding(typeof(NumericUpDown), new CommandBinding(_increaseCommand, OnIncreaseCommand));
            CommandManager.RegisterClassInputBinding(typeof(NumericUpDown), new InputBinding(_increaseCommand, new KeyGesture(Key.Up)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(_increaseCommand, new KeyGesture(Key.Up)));

            _decreaseCommand = new RoutedCommand("DecreaseCommand", typeof(NumericUpDown));
            CommandManager.RegisterClassCommandBinding(typeof(NumericUpDown), new CommandBinding(_decreaseCommand, OnDecreaseCommand));
            CommandManager.RegisterClassInputBinding(typeof(NumericUpDown), new InputBinding(_decreaseCommand, new KeyGesture(Key.Down)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(_decreaseCommand, new KeyGesture(Key.Down)));
        }

        private static void OnIncreaseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            NumericUpDown control = sender as NumericUpDown;
            if (control != null)
            {
                control.OnIncrease();
            }
        }
        private static void OnDecreaseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            NumericUpDown control = sender as NumericUpDown;
            if (control != null)
            {
                control.OnDecrease();
            }
        }

        /// <summary>
        /// Executes the increase
        /// </summary>
        protected virtual void OnIncrease()
        {
            Field field = this.DataContext as Field;
            if (field != null)
            {
                field.WorkItem.Open();               
            }
            object tempValue = Value;
            if (tempValue == null)
            {
                SetValue(ValueProperty, 0);
            }
            if (tempValue is double)
            {
                var temp = (double)tempValue;
                Value = temp+1;
            }
            if (tempValue is int)
            {
                Value = ((int)tempValue) + 1;
            }
        }

        /// <summary>
        /// Executes the decrease
        /// </summary>
        protected virtual void OnDecrease()
        {
            if (Value == null)
            {
                SetValue(ValueProperty, 0);
            }
            if (Value is double)
            {
                Value = ((double)Value) - 1;
            }
            if (Value is int)
            {
                Value = ((int)Value) - 1;
            }
        }

        #endregion
    }
}
