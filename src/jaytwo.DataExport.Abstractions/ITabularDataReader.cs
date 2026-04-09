using System;
using System.Threading;
using System.Threading.Tasks;

namespace jaytwo.DataExport.Abstractions;

/// <summary>
/// Represents a forward-only asynchronous reader over tabular data.
/// </summary>
public interface ITabularDataReader : IAsyncDisposable
{
    /// <summary>
    /// Gets the schema for the tabular data.
    /// </summary>
    ITabularSchema Schema { get; }

    /// <summary>
    /// Advances to the next row in the data source.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the read operation.</param>
    /// <returns><see langword="true"/> when another row is available; otherwise, <see langword="false"/>.</returns>
    ValueTask<bool> ReadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current row value for a column ordinal.
    /// </summary>
    /// <param name="ordinal">The zero-based column ordinal.</param>
    /// <returns>The value for the current row at the requested ordinal.</returns>
    object? GetValue(int ordinal);

    /// <summary>
    /// Gets the values for the current row.
    /// </summary>
    /// <returns>The values for the current row.</returns>
    object?[] GetValues();
}
