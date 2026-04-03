using System;

namespace jaytwo.DataExport.Excel;

public class WorkbookMetadata
{
    public string ApplicationName { get; set; } = "XLS Export";

    public string ApplicationVersion { get; set; } = "1.0.0";

    public string? CompanyName { get; set; }

    public string? Creator { get; set; }

    public string? LastModifiedBy { get; set; }

    public string? Title { get; set; }

    public string? Subject { get; set; }

    public string? Description { get; set; }

    public string? Keywords { get; set; }

    public string? Category { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime ModifiedAtUtc { get; set; } = DateTime.UtcNow;
}
