using System;

namespace jaytwo.StreamingExcelExport.Tests.Models;

public class Person
{
    public string Name { get; set; } = string.Empty;

    public DateTime BirthDate { get; set; }

    public int Age { get; set; }

    public string Biography { get; set; } = string.Empty;

    public string Lipsum { get; set; } = string.Empty;
}
