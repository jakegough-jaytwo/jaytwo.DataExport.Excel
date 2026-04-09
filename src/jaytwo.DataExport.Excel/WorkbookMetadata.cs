using System;

namespace jaytwo.DataExport.Excel;

/// <summary>
/// Holds workbook-level metadata written into the XLSX package properties.
/// </summary>
public class WorkbookMetadata
{
    /// <summary>
    /// Gets or sets the application name written to the extended properties. Defaults to <c>"XLS Export"</c>.
    /// </summary>
    public string ApplicationName { get; set; } = "XLS Export";

    /// <summary>
    /// Gets or sets the application version written to the extended properties. Defaults to <c>"1.0.0"</c>.
    /// </summary>
    public string ApplicationVersion { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the company name written to the extended properties.
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// Gets or sets the document creator written to the core properties.
    /// </summary>
    public string? Creator { get; set; }

    /// <summary>
    /// Gets or sets the last-modified-by value written to the core properties. Defaults to <see cref="Creator"/> when not set.
    /// </summary>
    public string? LastModifiedBy { get; set; }

    /// <summary>
    /// Gets or sets the document title written to the core properties.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the document subject written to the core properties.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the document description written to the core properties.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the keywords written to the core properties.
    /// </summary>
    public string? Keywords { get; set; }

    /// <summary>
    /// Gets or sets the category written to the core properties.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp written to the core properties. Defaults to <see cref="DateTime.UtcNow"/> at construction time.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last-modified timestamp written to the core properties. Defaults to <see cref="DateTime.UtcNow"/> at construction time.
    /// </summary>
    public DateTime ModifiedAtUtc { get; set; } = DateTime.UtcNow;
}
