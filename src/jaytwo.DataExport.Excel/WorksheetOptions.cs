using System;
using System.Collections.Generic;
using System.Text;

namespace jaytwo.DataExport.Excel;

public class WorksheetOptions
{
    private Dictionary<string, ColumnDefinition> _columnDefinitions = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new <see cref="WorksheetOptions"/> with the specified formatting defaults.
    /// </summary>
    /// <param name="includeHeader">Whether to write a header row from column or property names.</param>
    /// <param name="boldHeaderRow">Whether to apply bold formatting to the header row.</param>
    /// <param name="freezeHeaderRow">Whether to freeze the header row so it stays visible while scrolling.</param>
    /// <param name="applyZebraStripe">Whether to apply alternating row shading.</param>
    public WorksheetOptions(bool includeHeader = true, bool boldHeaderRow = true, bool freezeHeaderRow = true, bool applyZebraStripe = true)
    {
        IncludeHeader = includeHeader;
        BoldHeaderRow = boldHeaderRow;
        FreezeHeaderRow = freezeHeaderRow;
        ApplyZebraStripe = applyZebraStripe;
    }

    /// <summary>
    /// Gets or sets per-column formatting and layout settings, keyed by column or property name (case-insensitive).
    /// </summary>
    public Dictionary<string, ColumnDefinition> ColumnDefinitions
    {
        get => _columnDefinitions;
        set => _columnDefinitions = value == null
            ? throw new ArgumentNullException(nameof(value))
            : new Dictionary<string, ColumnDefinition>(value, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets or sets a value indicating whether a header row is written from column or property names.
    /// </summary>
    public bool IncludeHeader { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the header row is bold.
    /// </summary>
    public bool BoldHeaderRow { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether alternating row shading is applied.
    /// </summary>
    public bool ApplyZebraStripe { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the header row is frozen so it remains visible while scrolling.
    /// </summary>
    public bool FreezeHeaderRow { get; set; }

    /// <summary>
    /// Configures formatting and layout for a named column.
    /// </summary>
    /// <param name="columnName">The source column or property name to configure.</param>
    /// <param name="definition">The formatting and layout settings to apply.</param>
    /// <returns>The current <see cref="WorksheetOptions"/> instance.</returns>
    public WorksheetOptions SetupColumn(string columnName, ColumnDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(columnName))
        {
            throw new ArgumentException("Column name must not be null or whitespace.", nameof(columnName));
        }

        ColumnDefinitions[columnName] = definition ?? throw new ArgumentNullException(nameof(definition));
        return this;
    }
}
