using System.Collections.Generic;

namespace jaytwo.DataExport.Abstractions;

/// <summary>
/// Represents an in-memory tabular schema.
/// </summary>
public sealed class TabularSchema : ITabularSchema
{
    private readonly TabularColumn[] _columns;

    /// <summary>
    /// Initializes a new instance of the <see cref="TabularSchema"/> class.
    /// </summary>
    /// <param name="columns">The columns in ordinal order.</param>
    public TabularSchema(IReadOnlyList<TabularColumn> columns)
    {
        _columns = [.. columns];
    }

    /// <inheritdoc />
    public TabularColumn GetColumn(int ordinal) => _columns[ordinal];

    /// <inheritdoc />
    public TabularColumn[] GetColumns() => _columns;
}
