using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Linq;

namespace AIT.Taskboard.Application.Converter
{
    /// <summary>
    /// Converter that allows conversion from <see cref="bool"/> to <see cref="Visibility"/>
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter, IMultiValueConverter
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="BooleanToVisibilityConverter"/> is inverted.
        /// </summary>
        /// <value><c>true</c> if inverted; otherwise, <c>false</c>.</value>
        public bool Inverted { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="BooleanToVisibilityConverter"/> is using a NOT comparison.
        /// </summary>
        /// <value><c>true</c> if not; otherwise, <c>false</c>.</value>
        public bool Not { get; set; }

        /// <summary>
        /// Gets or sets the value to be used when true.
        /// </summary>
        /// <value>The on true.</value>
        public Visibility OnTrue { get; set; }

        /// <summary>
        /// Gets or sets the value to be used when false.
        /// </summary>
        /// <value>The on true.</value>
        public Visibility OnFalse { get; set; }

        #region IValueConverter Members

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            return Inverted
                       ? ConvertBoolToVisibility(value)
                       : ConvertVisibilityToBool(value);
        }


        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return Inverted
                       ? ConvertVisibilityToBool(value)
                       : ConvertBoolToVisibility(value);
        }

        #endregion

        /// <summary>
        /// Converts the visibility to bool.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private object ConvertVisibilityToBool(object value)
        {
            if (!(value is Visibility))

                return DependencyProperty.UnsetValue;


            return (((Visibility) value) == OnTrue) ^ Not;
        }


        /// <summary>
        /// Converts the bool to visibility.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private object ConvertBoolToVisibility(object value)
        {
            if (!(value is bool))

                return DependencyProperty.UnsetValue;


            return ((bool) value ^ Not)
                       ? OnTrue
                       : OnFalse;
        }

        #region Implementation of IMultiValueConverter

        /// <summary>
        ///                     Converts source values to a value for the binding target. The data binding engine calls this method when it propagates the values from source bindings to the binding target.
        /// </summary>
        /// <returns>
        ///                     A converted value.
        ///                     If the method returns null, the valid null value is used.
        ///                     A return value of <see cref="T:System.Windows.DependencyProperty" />.<see cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the converter did not produce a value, and that the binding will use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> if it is available, or else will use the default value.
        ///                     A return value of <see cref="T:System.Windows.Data.Binding" />.<see cref="F:System.Windows.Data.Binding.DoNothing" /> indicates that the binding does not transfer the value or use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue" /> or the default value.
        /// </returns>
        /// <param name="values">
        ///                     The array of values that the source bindings in the <see cref="T:System.Windows.Data.MultiBinding" /> produces. The value <see cref="F:System.Windows.DependencyProperty.UnsetValue" /> indicates that the source binding has no value to provide for conversion.
        ///                 </param>
        /// <param name="targetType">
        ///                     The type of the binding target property.
        ///                 </param>
        /// <param name="parameter">
        ///                     The converter parameter to use.
        ///                 </param>
        /// <param name="culture">
        ///                     The culture to use in the converter.
        ///                 </param>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool targetValue = values.Contains(false);
            return Convert(targetValue, targetType, parameter, culture);
        }

        /// <summary>
        ///                     Converts a binding target value to the source binding values.
        /// </summary>
        /// <returns>
        ///                     An array of values that have been converted from the target value back to the source values.
        /// </returns>
        /// <param name="value">
        ///                     The value that the binding target produces.
        ///                 </param>
        /// <param name="targetTypes">
        ///                     The array of types to convert to. The array length indicates the number and types of values that are suggested for the method to return.
        ///                 </param>
        /// <param name="parameter">
        ///                     The converter parameter to use.
        ///                 </param>
        /// <param name="culture">
        ///                     The culture to use in the converter.
        ///                 </param>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] {ConvertBack(value, targetTypes[0], parameter, culture)};
        }

        #endregion
    }
}