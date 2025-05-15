using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace SkChat.Skills;

public class WebSearchSkill
{
    [KernelFunction("search")]
    [Description("Search the web for information about a topic. Perform a web sercch.")]
    public async Task<string> SearchAsync(
        [Description("The topic or information to search for.")] string query)
    {
        // Replace this with actual API call (e.g., Bing, SerpAPI, etc.)
        return await Task.FromResult("[FAKE WEB RESULT] You searched for: {query}");
    }
}