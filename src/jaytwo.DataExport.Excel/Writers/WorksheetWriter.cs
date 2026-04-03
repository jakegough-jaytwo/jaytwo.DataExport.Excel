using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.DataExport.Excel.Writers;

internal class WorksheetWriter<T> : WorksheetWriterBase
{
    private static readonly PropertyInfo[] _typeProps = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

    private readonly IAsyncEnumerable<T> _data;

    public WorksheetWriter(WorksheetWriterContext<T> context, XmlWriter writer)
        : base(context.Options, context.WorksheetUid, writer, BuildFieldDictionary(context.Options))
    {
        _data = context.Data;
    }

    protected override async IAsyncEnumerable<object[]> GetCellData(bool writeHeader, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (writeHeader)
        {
            yield return _typeProps.Select(x => x.Name).Cast<object>().ToArray();
        }

        await foreach (var item in _data.WithCancellation(cancellationToken))
        {
            yield return _typeProps.Select(x => x.GetValue(item)).Cast<object>().ToArray();
        }
    }

    private static IDictionary<string, (int ColumnNumber, ColumnDefinition? ColumnLayout)> BuildFieldDictionary(WorksheetOptions options)
    {
        return Enumerable.Range(0, _typeProps.Length).ToDictionary(
            i => _typeProps[i].Name,
            i =>
            {
                ColumnDefinition? layout = null;
                options.ColumnDefinitions?.TryGetValue(_typeProps[i].Name, out layout);
                return (i + 1, layout);
            },
            StringComparer.OrdinalIgnoreCase);
    }
}
