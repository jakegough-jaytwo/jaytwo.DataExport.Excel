using System;
using System.Linq;
using System.Reflection;

namespace jaytwo.DataExport.Excel;

public class WorkbookMetadata
{
    // TODO: other excel properties like title/tags/comments/status/categories/subject
    public string ApplicationName { get; set; } = "XLS Export";

    public string ApplicationVersion { get; set; } = "1.0.0";

    public string? CompanyName { get; set; }

    public string? Creator { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
