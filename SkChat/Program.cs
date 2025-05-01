using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

Console.CancelKeyPress += (_, e) => { e.Cancel = true; Environment.Exit(0); };

// 1 · Build kernel
var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        deploymentName : Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT")
            ?? throw new ArgumentNullException("AZURE_OPENAI_DEPLOYMENT"),
        endpoint        : Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
            ?? throw new ArgumentNullException("AZURE_OPENAI_ENDPOINT"),
        apiKey          : Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY")
            ?? throw new ArgumentNullException("AZURE_OPENAI_KEY"))
    .Build();

// 2 · Load semantic-function plugin
var plugin  = kernel.CreatePluginFromPromptDirectory("./Plugins/ChatBot");
var chatFn  = plugin["Main"];                     // function name = file stem

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
