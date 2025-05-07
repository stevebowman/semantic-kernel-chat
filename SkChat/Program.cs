using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.Extensions.DependencyInjection;
using SkChat.Models;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.VectorData;

Console.CancelKeyPress += (_, e) => { e.Cancel = true; Environment.Exit(0); };

#pragma warning disable SKEXP0010 // AddAzureOpenAITextEmbeddingGeneration is for evaluation only
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
#pragma warning disable SKEXP0001

var profileCol = await SetupVectorStore(kernel);

// 2 · Load semantic-function plugin
var plugin  = kernel.CreatePluginFromPromptDirectory("./Plugins/ChatBot");
var chatFn  = plugin["Main"]; 

Console.WriteLine("Chat ready. Press Ctrl-C to exit.\n");

// 3 · Chat loop
while (true)
{
    Console.Write("> ");
    var user = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(user)) continue;

    var minScore = 0.75f;

    var embedGen = kernel.Services.GetRequiredService<ITextEmbeddingGenerationService>();
    var userEmbedding = await embedGen.GenerateEmbeddingAsync(user);
    var recalls = await profileCol.SearchEmbeddingAsync(userEmbedding, 10)
                            .Where(r => r.Score >= minScore)                   
                            .ToListAsync();

    // Log the recalls and scoresWhat
    foreach (var recall in recalls)
    {
        Console.WriteLine($"Recall: {recall.Record.Text} ({recall.Score})");
    }

    Console.WriteLine($"Recalls: {recalls.Count}");

    if (recalls.Any())
    {
        var context = string.Join("\n", recalls.Select(r => r.Record.Text));
        user = context + "\n\n" + user;
    }
    
    var kernelArgs = new KernelArguments { ["input"] = user };
    var result = await chatFn.InvokeAsync(kernel, kernelArgs);   // <- correct signature
    Console.WriteLine($"Bot: {result.GetValue<string>()}");
}

static string GetEnv(string name) =>
    Environment.GetEnvironmentVariable(name)
        ?? throw new ArgumentNullException(name);

async Task<IVectorStoreRecordCollection<Guid, Fact>> SetupVectorStore(Kernel kernel)
{
    var vectorStore = new QdrantVectorStore(new Qdrant.Client.QdrantClient("localhost", 6334));

    var profileCol = vectorStore.GetCollection<Guid, Fact>("profile");

    if (!await profileCol.CollectionExistsAsync())
        await profileCol.CreateCollectionAsync();

    var embedGen = kernel.Services.GetRequiredService<ITextEmbeddingGenerationService>();

    var profileFacts = new List<Fact>
    {
        new Fact { Id = new Guid("6297a907-3973-479f-ab18-fd2702dfebbb"), Text = "My name is Steve." },
        new Fact { Id = new Guid("b999806b-9fe5-4526-803b-085bb83791b3"), Text = "I live in Hull, UK." },
        new Fact { Id = new Guid("fe192bca-c7e8-40a9-bdd0-68267ecfdaef"), Text = "I'm interested in physics and LLMs." },
        new Fact { Id = new Guid("0ad22372-4adc-45e5-b28d-71ee2464988d"), Text = "I like drinking tea." },
        new Fact { Id = new Guid("d57aaf37-3ca0-4684-a855-49431e4bafa7"), Text = "I like Karol G." },
        new Fact { Id = new Guid("28132886-1f72-41d7-aff7-242b35edd4d7"), Text = "I support Hull City, Atletico Nacional and Banfield." },
        new Fact { Id = new Guid("eb8fb372-7202-4b96-8aea-60722a7b517d"), Text = "I hate Leeds United." }
    };

    foreach (var fact in profileFacts)
    {
        var embedding = await embedGen.GenerateEmbeddingAsync(fact.Text);
        fact.Embedding = embedding;
        await profileCol.UpsertAsync(fact);
    }

    Console.WriteLine("Profile facts stored.");

    // var queryText = "What do I like?";
    // var queryEmbedding = await embedGen.GenerateEmbeddingAsync(queryText);

    // var results = profileCol.SearchEmbeddingAsync(
    //     queryEmbedding,   // pre-generated embedding
    //     top: 1);

    // var firstResult = await results.FirstOrDefaultAsync();
    // Console.WriteLine(firstResult?.Record.Text);

    return profileCol;
}