using System.Runtime.CompilerServices;
using Windows.Data.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace WinUI.Fluent.Icons;

internal static class FontData
{
    // Amethyst language resource trees
    private static JsonObject _fontResources;

    public static JsonObject FontResources
    {
        get
        {
            if (_fontResources is null)
                LoadFluentIconResources();

            // Return whatever
            return _fontResources;
        }
    }

    // Load the current desired resource JSON into app memory
    private static void LoadFluentIconResources()
    {
        try
        {
            var resourcePath = Path.Join(
                new FileInfo(typeof(FontData).Assembly.Location).DirectoryName,
                "Assets", "FluentSymbolIcons.json");

            // If failed again, just give up
            if (!File.Exists(resourcePath)) return; // Just give up

            // Parse the loaded json
            _fontResources = JsonObject.Parse(File.ReadAllText(resourcePath));
        }
        catch (Exception)
        {
            // ignored
        }
    }

    // Get a string from runtime JSON resources, language from settings
    public static bool GetFluentIconData(string resourceKey, out string result)
    {
        try
        {
            // Check if the resource root is fine
            if (FontResources is not null && FontResources.Count > 0)
            {
                result = FontResources.GetNamedString(resourceKey);
                return true;
            }
        }
        catch (Exception)
        {
            // ignored
        }

        result = null;
        return false; // Just give up
    }
}

public partial class FluentSymbolIcon : Control
{
    /// <summary>
    ///     Identifies the <see cref="Symbol" /> property.
    /// </summary>
    public static readonly DependencyProperty SymbolProperty = DependencyProperty.Register(
        nameof(Symbol), typeof(FluentSymbol), typeof(FluentSymbolIcon),
        new PropertyMetadata(null, OnSymbolChanged)
    );

    private PathIcon iconDisplay;

    public FluentSymbolIcon()
    {
        DefaultStyleKey = typeof(FluentSymbolIcon);
    }

    /// <summary>
    ///     Constructs a <see cref="FluentSymbolIcon" /> with the specified symbol.
    /// </summary>
    public FluentSymbolIcon(FluentSymbol symbol)
    {
        DefaultStyleKey = typeof(FluentSymbolIcon);
        Symbol = symbol;
    }

    /// <summary>
    ///     Gets or sets the Fluent System Icons glyph used as the icon content.
    /// </summary>
    public FluentSymbol Symbol
    {
        get => (FluentSymbol)GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild("IconDisplay") is not PathIcon pi) return;

        // Awkward workaround for a weird bug where iconDisplay is null
        // when OnSymbolChanged fires in a newly created FluentSymbolIcon
        iconDisplay = pi;
        Symbol = Symbol;
    }

    private static void OnSymbolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentSymbolIcon self && (e.NewValue is FluentSymbol || e.NewValue is int) && self.iconDisplay != null)
            // Set internal Data to the Path from the look-up table
            self.iconDisplay.Data = GetPathData((FluentSymbol)e.NewValue);
    }

    /// <summary>
    ///     Returns a new <see cref="PathIcon" /> using the path associated with the provided <see cref="FluentSymbol" />.
    /// </summary>
    public static PathIcon GetPathIcon(FluentSymbol symbol)
    {
        return new PathIcon
        {
            Data = (Geometry)XamlBindingHelper.ConvertValue(typeof(Geometry), GetPathData(symbol)),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
    }

    /// <summary>
    ///     Returns a new <see cref="Geometry" /> using the path associated with the provided <see cref="int" />.
    ///     The <paramref name="symbol" /> parameter is cast to <see cref="FluentSymbol" />.
    /// </summary>
    public static Geometry GetPathData(int symbol)
    {
        return GetPathData((FluentSymbol)symbol);
    }

    /// <summary>
    ///     Returns a new <see cref="Geometry" /> using the path associated with the provided <see cref="int" />.
    /// </summary>
    public static Geometry GetPathData(FluentSymbol symbol)
    {
        if (FontData.GetFluentIconData(symbol.ToString(), out var pathData))
            return (Geometry)XamlBindingHelper.ConvertValue(typeof(Geometry), pathData);
        return null;
    }
}