using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace jaytwo.DataExport.Abstractions;

/// <summary>
/// Adapts an <see cref="IAsyncEnumerable{T}"/> of objects to <see cref="ITabularDataReader"/>.
/// </summary>
/// <typeparam name="T">The row type whose public instance properties define the schema.</typeparam>
public sealed class ObjectTabularDataReader<T> : ITabularDataReader
{
    private static readonly PropertyInfo[] TypeProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
    private static readonly ITabularSchema TypeSchema = new TabularSchema(
        TypeProperties
            .Select(x => new TabularColumn(x.Name, x.PropertyType))
            .ToArray());

    private readonly IAsyncEnumerator<T> _enumerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectTabularDataReader{T}"/> class.
    /// </summary>
    /// <param name="data">The asynchronous sequence to adapt.</param>
    public ObjectTabularDataReader(IAsyncEnumerable<T> data)
    {
        _enumerator = data.GetAsyncEnumerator();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectTabularDataReader{T}"/> class.
    /// </summary>
    /// <param name="data">The sequence to adapt.</param>
    /// <param name="cancellationToken">A token used to cancel enumeration.</param>
    public ObjectTabularDataReader(IEnumerable<T> data, CancellationToken cancellationToken = default)
        : this(ToAsyncEnumerable(data, cancellationToken))
    {
    }

    /// <inheritdoc />
    public ITabularSchema Schema => TypeSchema;

    /// <inheritdoc />
    public ValueTask<bool> ReadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _enumerator.MoveNextAsync();
    }

    /// <inheritdoc />
    public object? GetValue(int ordinal) => TypeProperties[ordinal].GetValue(_enumerator.Current);

    /// <inheritdoc />
    public object?[] GetValues()
        => TypeProperties
            .Select(x => x.GetValue(_enumerator.Current))
            .ToArray();

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _enumerator.DisposeAsync();
    }

    private static async IAsyncEnumerable<T> ToAsyncEnumerable(IEnumerable<T> source, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return item;
            await Task.Yield(); // intentional: yields the scheduler so sync-sourced enumerables don't block
        }
    }
}
