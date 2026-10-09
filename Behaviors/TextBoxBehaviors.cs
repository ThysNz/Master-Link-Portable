using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MasterLink.Desktop.Behaviors;

// Commit-on-blur/Enter, revert-on-Escape behavior for plain WPF TextBoxes
// bound with UpdateSourceTrigger=LostFocus: Enter commits (moves focus,
// which fires LostFocus); Escape discards the in-progress edit and reverts
// to the last committed value. Multiline fields (AcceptsReturn=true) get
// Escape-revert only - Enter inserts a newline instead.
public static class TextBoxBehaviors
{
    public static readonly DependencyProperty CommitOnEnterEscapeRevertProperty =
        DependencyProperty.RegisterAttached(
            "CommitOnEnterEscapeRevert", typeof(bool), typeof(TextBoxBehaviors),
            new PropertyMetadata(false, OnChanged));

    public static void SetCommitOnEnterEscapeRevert(DependencyObject d, bool value) => d.SetValue(CommitOnEnterEscapeRevertProperty, value);
    public static bool GetCommitOnEnterEscapeRevert(DependencyObject d) => (bool)d.GetValue(CommitOnEnterEscapeRevertProperty);

    private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBoxBase tb) return;
        tb.PreviewKeyDown -= OnPreviewKeyDown;
        if ((bool)e.NewValue) tb.PreviewKeyDown += OnPreviewKeyDown;
    }

    private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not TextBox tb) return;

        if (e.Key == Key.Escape)
        {
            tb.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
            MoveFocusAway(tb);
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && !tb.AcceptsReturn)
        {
            tb.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            MoveFocusAway(tb);
            e.Handled = true;
        }
    }

    private static void MoveFocusAway(TextBox tb)
    {
        tb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
    }

    // Clears a still-default value (e.g. a freshly-added cost row's "0") the
    // moment the field is clicked into, so the user can type straight over
    // it instead of having to select-all/delete it first. Leaves the field
    // alone once it holds a real value.
    public static readonly DependencyProperty ClearOnFocusIfEqualsProperty =
        DependencyProperty.RegisterAttached(
            "ClearOnFocusIfEquals", typeof(string), typeof(TextBoxBehaviors),
            new PropertyMetadata(null, OnClearOnFocusIfEqualsChanged));

    public static void SetClearOnFocusIfEquals(DependencyObject d, string? value) => d.SetValue(ClearOnFocusIfEqualsProperty, value);
    public static string? GetClearOnFocusIfEquals(DependencyObject d) => (string?)d.GetValue(ClearOnFocusIfEqualsProperty);

    private static void OnClearOnFocusIfEqualsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox tb) return;
        tb.GotFocus -= OnGotFocusClearIfEquals;
        if (e.NewValue is string) tb.GotFocus += OnGotFocusClearIfEquals;
    }

    private static void OnGotFocusClearIfEquals(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox tb) return;
        if (tb.Text == GetClearOnFocusIfEquals(tb)) tb.Text = string.Empty;
    }
}
