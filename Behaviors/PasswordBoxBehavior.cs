using System.Windows;
using System.Windows.Controls;

namespace MasterLink.Desktop.Behaviors;

// PasswordBox.Password isn't a dependency property, so it can't be bound
// directly. BoundPassword mirrors it: view model -> box whenever the bound
// value changes, box -> view model on LostFocus (the same commit timing the
// plain TextBoxes use, so the db isn't written on every keystroke).
public static class PasswordBoxBehavior
{
    public static readonly DependencyProperty BoundPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BoundPassword", typeof(string), typeof(PasswordBoxBehavior),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundPasswordChanged));

    public static void SetBoundPassword(DependencyObject d, string value) => d.SetValue(BoundPasswordProperty, value);
    public static string GetBoundPassword(DependencyObject d) => (string)d.GetValue(BoundPasswordProperty);

    private static readonly DependencyProperty HookedProperty =
        DependencyProperty.RegisterAttached("Hooked", typeof(bool), typeof(PasswordBoxBehavior), new PropertyMetadata(false));

    private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox box) return;

        if (!(bool)box.GetValue(HookedProperty))
        {
            box.SetValue(HookedProperty, true);
            box.LostFocus += (_, _) => SetBoundPassword(box, box.Password);
        }

        var value = (string?)e.NewValue ?? "";
        if (box.Password != value) box.Password = value;
    }
}
