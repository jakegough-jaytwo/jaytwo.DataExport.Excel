namespace jaytwo.DataExport.Abstractions;

/// <summary>
/// Describes the columns exposed by a tabular data reader.
/// </summary>
public interface ITabularSchema
{
    /// <summary>
    /// Gets the column for a zero-based ordinal.
    /// </summary>
    /// <param name="ordinal">The zero-based ordinal of the column.</param>
    /// <returns>The column at the requested ordinal.</returns>
    TabularColumn GetColumn(int ordinal);

    /// <summary>
    /// Gets the columns in ordinal order.
    /// </summary>
    /// <returns>The columns in ordinal order.</returns>
    TabularColumn[] GetColumns();
}
