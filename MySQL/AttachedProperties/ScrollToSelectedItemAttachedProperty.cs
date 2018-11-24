using System.Windows;

namespace MySQL
{
    /// <summary>
    /// The ScrollToSelectedItemAttachedProperty attached property for creating a <see cref="System.Windows.Controls.ListView"/> that can scroll to the current selected item
    /// </summary>
    public class ScrollToSelectedItemAttachedProperty: BaseAttachedProperty<ScrollToSelectedItemAttachedProperty, object>
    {
        /// <summary>
        /// A flag indicating if this is the first time this property has beeen updated
        /// </summary>
        public bool FirstUpdate { get; set; } = true;

        public override void OnValueUpdated (DependencyObject sender, object value)
        {
            // Get the ListView element
            if (!(sender is System.Windows.Controls.DataGrid element))
                return;

            // Don't fire if the value doesn't change
            if (sender.GetValue(ValueProperty) == value && FirstUpdate == false)
                return;  
            else
                // No longer in first update
                FirstUpdate = false;

            if (value != null)
                // Scroll to the current selected item
                element.ScrollIntoView(value);            
        }
    }
}
