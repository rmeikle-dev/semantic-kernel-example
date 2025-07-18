using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using System.Text;
using SemanticKernelTest.Plugins;

namespace SemanticKernelTest.Agents
{
    public static class TaskAgent
    {
        public static async Task RunAgent(Kernel kernel)
        {
            Console.WriteLine("Defining task agent...");
            kernel.ImportPluginFromType<TaskPlugins>();
            ChatHistoryAgentThread agentThread = new();

            var arguments = new KernelArguments(new AzureOpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Required()
            })
            {
                { "date", DateTime.UtcNow.ToString("yyyy-MM-dd") } // or use DateTime.Now for local time
            };

            ChatCompletionAgent agent =
                new()
                {
                    Name = "TaskAgent",
                    Instructions =
                        """
                        You are an agent that orchestrates the workflow for a contract renewal process. You will be given the id of a process from which you will obtain the current status of that process and decide what the next steps are.
                        You must be proactive in progressing the workflow - always check the status of the currently outstanding task.
                        
                        The key steps in the workflow are as follows:
                        - User uploads initial contract
                        - Contract should be parsed for key information
                        - Updated contract is uploaded
                        - The updated contract should be compared with the previous contract to understand the differences
                        - Stakeholders are notified of the differences and asked to review
                        - When all stakeholders have provided feedback, you should review all feedback and make a recommendation as to whether contract should be renewed
                        
                        After every action you take, you MUST update the status of the workflow using the appropriate function.  You MUST also provide a message to the user with the current status of the workflow.
                        
                        You must always output a JSON object with the following structure:
                        {
                          "message": "<your rationale and next steps>",
                          "shouldContinue": <true|false>,
                          "isComplete": <true|false>
                        }
                        Only set "isComplete" to true when the contract renewal process is fully complete.
                        Set 'shouldContinue' to true when process is not yet complete and you are able to progress the workflow.  Where you are not able to progress further (e.g. you are waiting on further information) 'shouldContinue' should be false.
                        
                        Today's date is {{$date}}
                        """,
                    Kernel = kernel,
                    Arguments = arguments
                };

            var message = new ChatMessageContent(AuthorRole.User, "the id is 1");

            bool isComplete = false;
            bool shouldContinue = true;
            while (shouldContinue)
            {
                StringBuilder responseBuilder = new();
                await foreach (StreamingChatMessageContent response in agent.InvokeStreamingAsync(message, agentThread))
                {
                    foreach (var streamingKernelContent in response.Items)
                    {
                        var content = streamingKernelContent.ToString();
                        if(content != "StreamingFunctionCallUpdateContent")
                            responseBuilder.Append(content);
                    }
                }

                string responseText = responseBuilder.ToString();
                Console.WriteLine(responseText);

                // Try to parse the JSON object from the response
                try
                {
                    var jsonStart = responseText.IndexOf('{');
                    var jsonEnd = responseText.LastIndexOf('}');
                    if (jsonStart >= 0 && jsonEnd > jsonStart)
                    {
                        var json = responseText.Substring(jsonStart, jsonEnd - jsonStart + 1);
                        var result = System.Text.Json.JsonSerializer.Deserialize<AgentResult>(json);
                        isComplete = result?.isComplete ?? false;
                        shouldContinue = result?.shouldContinue ?? false;
                        if (shouldContinue)
                        {
                            // Optionally, update the message/context for the next loop iteration
                            message = new ChatMessageContent(AuthorRole.User, "continue");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Could not find JSON object in response.");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing agent response: {ex.Message}");
                    break;
                }
            }

            Console.WriteLine("\nTask is complete.");
        }

        private class AgentResult
        {
            public string? message { get; set; }
            public bool isComplete { get; set; }
            public bool shouldContinue { get; set; }
        }
    }
}
