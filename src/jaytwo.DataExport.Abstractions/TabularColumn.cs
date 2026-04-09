using System;

namespace jaytwo.DataExport.Abstractions;

/// <summary>
/// Represents a tabular column definition.
/// </summary>
public sealed class TabularColumn
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TabularColumn"/> class.
    /// </summary>
    /// <param name="name">The column name.</param>
    /// <param name="fieldType">The CLR type represented by the column.</param>
    public TabularColumn(string name, Type fieldType)
    {
        Name = name;
        FieldType = fieldType;
    }

    /// <summary>
    /// Gets the column name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the CLR type represented by the column.
    /// </summary>
    public Type FieldType { get; }
}
