using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.DataExport.Excel.Writers;

internal class WorksheetWriterDataReader : WorksheetWriterBase
{
    private readonly IDataReader _data;

    public WorksheetWriterDataReader(string worksheetUid, WorksheetOptions options, IDataReader data, XmlWriter writer, StyleRegistry styleRegistry)
        : base(options, worksheetUid, writer, BuildFieldDictionary(data, options), styleRegistry)
    {
        _data = data;
    }

    protected override async IAsyncEnumerable<object[]> GetCellData(bool writeHeader, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (writeHeader)
        {
            yield return FieldNames.Cast<object>().ToArray();
        }

        while (_data.Read())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var values = new object[_data.FieldCount];
            _data.GetValues(values);

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] is DBNull)
                {
                    values[i] = null!;
                }
            }

            yield return values;
            await Task.Yield();
        }
    }

    private static IDictionary<string, (int ColumnNumber, ColumnDefinition? ColumnLayout)> BuildFieldDictionary(IDataReader data, WorksheetOptions options)
    {
        return Enumerable.Range(0, data.FieldCount).ToDictionary(
            i => data.GetName(i),
            i =>
            {
                ColumnDefinition? layout = null;
                options.ColumnDefinitions?.TryGetValue(data.GetName(i), out layout);
                return (i + 1, layout);
            },
            StringComparer.OrdinalIgnoreCase);
    }
}
