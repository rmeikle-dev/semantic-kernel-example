using System.Diagnostics.CodeAnalysis;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.GroupChat;
using Microsoft.SemanticKernel.Agents.Runtime;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticKernelTest.Orchestration;

namespace SemanticKernelTest.Agents
{
    public static class GroupChat
    {
        [Experimental("SKEXP0110")]
        public static async Task RunAgent(Kernel kernel)
        {
            ChatCompletionAgent writer = new ChatCompletionAgent
            {
                Name = "Suggester",
                Description = "A suggester of slogans",
                Instructions = "You suggest slogans until you get approval. Only return the slogan. If you get approval, then you return a JSON object: {agentName: 'suggester', allTasksCompleted: true}",
                Kernel = kernel,
            };

            ChatCompletionAgent editor = new ChatCompletionAgent
            {
                Name = "Reviewer",
                Description = "A reviewer of slogans.",
                Instructions = "You review slogans, and you provide a single response in JSON format: {approved:bool}. Once you have approved a slogan, you must return a JSON object: {agentName: 'reviewer', allTasksCompleted: true}",
                Kernel = kernel
            };

            ChatHistory history = [];

            ValueTask responseCallback(ChatMessageContent response)
            {
                Console.WriteLine($"[{response.AuthorName}] {response.Content}");
                history.Add(response);
                return ValueTask.CompletedTask;
            }

            var topic = "Create a slogan for a new electric SUV that is affordable and fun to drive.";

            GroupChatOrchestration orchestration =
                new(
                    new AIGroupChatManager(
                        topic,
                        kernel.GetRequiredService<IChatCompletionService>())
                    {
                        MaximumInvocationCount = 5
                    },
                    writer, editor)
                {
                    ResponseCallback = responseCallback
                };


            InProcessRuntime runtime = new();
            await runtime.StartAsync();

            // Run the orchestration
            Console.WriteLine($"\n# INPUT: {topic}\n");
            OrchestrationResult<string> result = await orchestration.InvokeAsync(topic, runtime);
            string text = await result.GetValueAsync(TimeSpan.FromSeconds(60));
            Console.WriteLine($"\n# FINAL RESULT: {text}");

            await runtime.RunUntilIdleAsync();


            Console.WriteLine("\n\nFULL ORCHESTRATION HISTORY");
            foreach (ChatMessageContent message in history)
            {
                Console.WriteLine($"[{message.AuthorName}] {message.Content}");
            }
            Console.ReadLine();
        }

        private class AgentResult
        {
            public string? message { get; set; }
            public bool isComplete { get; set; }
            public bool shouldContinue { get; set; }
        }
    }
}
