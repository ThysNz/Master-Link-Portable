using System.Windows;

namespace MasterLink.Desktop.Behaviors;

// Attached property consumed by the placeholder-aware TextBox templates in
// Themes/Styles.xaml (EditableTextBoxStyle / BoxedTextBoxStyle) - set it on
// any TextBox using either style to show greyed-out text while the field is
// empty. Purely visual: it never touches the bound Text value, so nothing
// needs to be cleared before typing.
public static class PlaceholderBehavior
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.RegisterAttached("Text", typeof(string), typeof(PlaceholderBehavior), new PropertyMetadata(null));

    public static void SetText(DependencyObject d, string value) => d.SetValue(TextProperty, value);
    public static string GetText(DependencyObject d) => (string)d.GetValue(TextProperty);
}
