using System;
using System.Collections.Generic;
using System.Text;

namespace jaytwo.DataExport.Excel;

public class WorksheetOptions
{
    public WorksheetOptions(bool includeHeader = true, bool boldHeaderRow = true, bool freezeHeaderRow = true, bool applyZebraStripe = true)
    {
        IncludeHeader = includeHeader;
        BoldHeaderRow = boldHeaderRow;
        FreezeHeaderRow = freezeHeaderRow;
        ApplyZebraStripe = applyZebraStripe;
    }

    public Dictionary<string, ColumnDefinition> ColumnDefinitions { get; set; } = new();

    public bool IncludeHeader { get; set; }

    public bool BoldHeaderRow { get; set; }

    public bool ApplyZebraStripe { get; set; }

    public bool FreezeHeaderRow { get; set; }

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
