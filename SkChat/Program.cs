using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? throw new ArgumentNullException("AZURE_OPENAI_DEPLOYMENT environment variable is not set."),
        Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new ArgumentNullException("AZURE_OPENAI_ENDPOINT environment variable is not set."),
        Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY") ?? throw new ArgumentNullException("AZURE_OPENAI_KEY environment variable is not set."))
    .Build();

Console.WriteLine("Kernel initialized: " + Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"));
Console.WriteLine("Deployment name: " + Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT"));

while (true)
{
    Console.Write("> ");
    var user = Console.ReadLine();
    var reply = await kernel.InvokePromptAsync("{{$input}}", new() { { "input", user } });
    Console.WriteLine($"Bot: {reply}");
}
