using Microsoft.Extensions.VectorData;

namespace SkChat.Models;

public class Fact
{
    [VectorStoreRecordKey]
    public Guid Id { get; set; } = Guid.NewGuid();

    [VectorStoreRecordData(IsFullTextIndexed = true)]
    public string Text { get; set; } = "";

    [VectorStoreRecordVector(1536)]
    public ReadOnlyMemory<float>? Embedding { get; set; }
}
