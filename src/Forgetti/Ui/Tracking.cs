using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Forgetti.Ui;

/// <summary>
/// Spaced capitals for headers, as an attached property:
/// <c>&lt;TextBlock ui:Tracking.Text="Installs" ui:Tracking.Amount="0.08" /&gt;</c>.
///
/// WPF has no letter-spacing property anywhere in its text stack. The header is therefore built
/// from one <see cref="Run"/> per character with a thin space between them, its font size scaled
/// to the wanted gap. Approximate to a fraction of a pixel, and in exchange the text stays one
/// wrappable, selectable block.
/// </summary>
public static class Tracking
{
    /// <summary>A thin space is about a fifth of an em wide.</summary>
    private const double ThinSpaceEmWidth = 0.2d;

    public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
        "Text", typeof(string), typeof(Tracking), new PropertyMetadata(null, OnChanged));

    /// <summary>Space between letters, as a fraction of the font size.</summary>
    public static readonly DependencyProperty AmountProperty = DependencyProperty.RegisterAttached(
        "Amount", typeof(double), typeof(Tracking), new PropertyMetadata(0.06d, OnChanged));

    public static string? GetText(DependencyObject element) => (string?)element.GetValue(TextProperty);

    public static void SetText(DependencyObject element, string? value) => element.SetValue(TextProperty, value);

    public static double GetAmount(DependencyObject element) => (double)element.GetValue(AmountProperty);

    public static void SetAmount(DependencyObject element, double value) => element.SetValue(AmountProperty, value);

    private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBlock block)
        {
            return;
        }

        Fill(block);

        // XAML sets attributes in document order, so the font size may not be final yet. One more
        // pass once the block is loaded picks up whatever it ended up with.
        if (!block.IsLoaded)
        {
            block.Loaded -= OnLoaded;
            block.Loaded += OnLoaded;
        }
    }

    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is TextBlock block)
        {
            block.Loaded -= OnLoaded;
            Fill(block);
        }
    }

    private static void Fill(TextBlock block)
    {
        block.Inlines.Clear();

        // Culture-aware on purpose: this is display text a person reads, not a key.
        string value = (GetText(block) ?? string.Empty).ToUpper(CultureInfo.CurrentCulture);
        double spacerSize = GetAmount(block) * block.FontSize / ThinSpaceEmWidth;

        for (int i = 0; i < value.Length; i++)
        {
            block.Inlines.Add(new Run(value[i].ToString()));

            // No trailing spacer: it would push a centered header off center.
            if (i < value.Length - 1)
            {
                block.Inlines.Add(new Run(" ") { FontSize = spacerSize });
            }
        }
    }
}
