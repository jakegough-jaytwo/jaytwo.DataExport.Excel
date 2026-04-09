using System;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace jaytwo.DataExport.Abstractions;

/// <summary>
/// Adapts a <see cref="DbDataReader"/> to <see cref="ITabularDataReader"/>.
/// </summary>
public sealed class DataReaderTabularDataReader : ITabularDataReader
{
    private readonly DbDataReader _data;
    private readonly ITabularSchema _schema;
    private readonly bool _disposeReader;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataReaderTabularDataReader"/> class.
    /// </summary>
    /// <param name="data">The data reader to adapt.</param>
    /// <param name="disposeReader"><see langword="true"/> to dispose the wrapped reader when this instance is disposed.</param>
    public DataReaderTabularDataReader(DbDataReader data, bool disposeReader = false)
    {
        _data = data;
        _disposeReader = disposeReader;
        _schema = new TabularSchema(
            Enumerable.Range(0, data.FieldCount)
                .Select(i => new TabularColumn(data.GetName(i), GetFieldType(data, i)))
                .ToArray());
    }

    /// <inheritdoc />
    public ITabularSchema Schema => _schema;

    /// <inheritdoc />
    public ValueTask<bool> ReadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new ValueTask<bool>(_data.ReadAsync(cancellationToken));
    }

    /// <inheritdoc />
    public object? GetValue(int ordinal) => _data.GetValue(ordinal);

    /// <inheritdoc />
    public object?[] GetValues()
    {
        var values = new object?[_data.FieldCount];
        _data.GetValues(values!);
        return values;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        if (_disposeReader)
        {
            return _data.DisposeAsync();
        }

        return default;
    }

    private static Type GetFieldType(DbDataReader data, int ordinal)
    {
        try
        {
            return data.GetFieldType(ordinal);
        }
        catch
        {
            return typeof(object);
        }
    }
}
