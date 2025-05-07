namespace jaytwo.StreamingExcelExport.OpenXml;

internal record class RelationshipSpec
{
    public RelationshipSpec(string id, string type, string target)
        => (Id, Type, Target) = (id, type, target);

    public string Id { get; set; }

    public string Type { get; set; }

    public string Target { get; set; }
}
