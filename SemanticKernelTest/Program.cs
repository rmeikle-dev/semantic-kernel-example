using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelTest.Agents;

namespace SemanticKernelTest
{
    internal class Program
    {
        [Experimental("SKEXP0110")]
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            var builder = Kernel.CreateBuilder();
            // add logging
            builder.Services.AddLogging(configure =>
            {
                configure.AddConsole();
                configure.SetMinimumLevel(LogLevel.Warning);
            });

            builder.AddAzureOpenAIChatCompletion(
                "axon-qa-gpt-41-mini",                      // Azure OpenAI Deployment Name
                "https://eastus.api.cognitive.microsoft.com/", // Azure OpenAI Endpoint
                "7cf4ef8704314e89a0e1573e23e2aa7a");

            var kernel = builder.Build();

            await TaskAgent.RunAgent(kernel.Clone());
            //await GroupChat.RunAgent(kernel.Clone());

            // await InlineFunction(kernel);
            Console.WriteLine("Done");
        }

   
    }
}
