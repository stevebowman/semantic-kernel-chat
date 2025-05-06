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

var vectorStore = new QdrantVectorStore(new Qdrant.Client.QdrantClient("localhost", 6334));

var profileCol  = vectorStore.GetCollection<Guid, Fact>("profile");

if (!await profileCol.CollectionExistsAsync())
    await profileCol.CreateCollectionAsync();

var embedGen = kernel.Services.GetRequiredService<ITextEmbeddingGenerationService>();

var text = "My name is Steve and I like Hull and tea.";
var embedding = await embedGen.GenerateEmbeddingAsync(text);

await profileCol.UpsertAsync(new Fact
{
    Text      = text,
    Embedding = embedding          
});

Console.WriteLine("Profile stored.");

var queryText = "What do I like?";
var queryEmbedding = await embedGen.GenerateEmbeddingAsync(queryText);

var results = profileCol.SearchEmbeddingAsync(
    queryEmbedding,   // pre-generated embedding
    top: 1);

var firstResult = await results.FirstOrDefaultAsync();
Console.WriteLine(firstResult?.Record.Text);

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

    // wrap user input in KernelArguments
    var kernelArgs = new KernelArguments { ["input"] = user };

    var result = await chatFn.InvokeAsync(kernel, kernelArgs);   // <- correct signature
    Console.WriteLine($"Bot: {result.GetValue<string>()}");
}

static string GetEnv(string name) =>
    Environment.GetEnvironmentVariable(name)
        ?? throw new ArgumentNullException(name);
