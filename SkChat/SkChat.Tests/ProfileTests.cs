using Microsoft.SemanticKernel.AI.Embeddings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;
using SkChat.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;

public class ProfileTests
{
    [Fact]
    public async Task CanWriteAndRecallFact()
    {
        #pragma warning disable SKEXP0010 // AddAzureOpenAITextEmbeddingGeneration is for evaluation only
        #pragma warning disable SKEXP0001
        var kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName  : GetEnv("AZURE_OPENAI_DEPLOYMENT"),
                endpoint        : GetEnv("AZURE_OPENAI_ENDPOINT"),
                apiKey          : GetEnv("AZURE_OPENAI_KEY"))
            .AddAzureOpenAITextEmbeddingGeneration(
                deploymentName  : GetEnv("AZURE_OPENAI_DEPLOYMENT_EMBEDDING"),
                endpoint        : GetEnv("AZURE_OPENAI_ENDPOINT"),
                apiKey          : GetEnv("AZURE_OPENAI_KEY"))   
            .Build();
       
            
        var embedGen = kernel.Services.GetRequiredService<ITextEmbeddingGenerationService>();
        var vectorStore = new QdrantVectorStore(new QdrantClient("localhost", 6334));
        var collection = vectorStore.GetCollection<Guid, Fact>("test-facts");

        if (!await collection.CollectionExistsAsync())
            await collection.CreateCollectionAsync();

        var fact = new Fact { Id = new Guid("2082b425-d25d-4963-a28d-2c191d554110"), Text = "Testing recall logic." };
        var embedding = await embedGen.GenerateEmbeddingAsync(fact.Text);
        fact.Embedding = embedding.Data;

        await collection.UpsertAsync(fact);

        var queryVec = await embedGen.GenerateEmbeddingAsync("What is this about?");
        var hits = await collection.SearchEmbeddingAsync(queryVec.Data, 1).ToListAsync();

        Assert.Contains(hits, h => h.Record.Text.Contains("Testing recall"));
    }

    static string GetEnv(string name) =>
        Environment.GetEnvironmentVariable(name)
            ?? throw new ArgumentNullException(name);
}

