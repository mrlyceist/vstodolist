using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace todolist
{
    public class EventFocusAttachment
    {
        public static readonly DependencyProperty ElementToFocusProperty = DependencyProperty.RegisterAttached(
            "ElementToFocus", typeof(Control), typeof(EventFocusAttachment),
            new UIPropertyMetadata(null, ElementToFocusPropertyChanged));

        private static void ElementToFocusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = d as ToggleButton;
            if (button == null) return;
            button.Click += (s, args) =>
            {
                Control control = GetElementToFocus(button);
                control?.Focus();
            };
        }

        public static Control GetElementToFocus(ToggleButton button)
        {
            return (Control)button.GetValue(ElementToFocusProperty);
        }

        public static void SetElementToFocus(ToggleButton button, Control value)
        {
            button.SetValue(ElementToFocusProperty, value);
        }
    }
}