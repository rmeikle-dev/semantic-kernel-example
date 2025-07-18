using Microsoft.SemanticKernel.Agents.Orchestration.GroupChat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace SemanticKernelTest.Orchestration
{
    [Experimental("SKEXP0110")]
    public sealed class AIGroupChatManager(string topic, IChatCompletionService chatCompletion) : GroupChatManager
    {
        private static class Prompts
        {
            public static string Termination(string topic) =>
                $"""
                You are a mediator that orchestrates a group of other agents that each have a different task.'. 
                You need to determine if the all agents have completed the tasks that they need to complete. 
                You should be able to determine this by reviewing the history of responses. Agents will sign off that they have completed all their tasks by providing a json response like this : {String.Format("{0}{1:N}{2}", "{", "agentName: string, allTasksCompleted: bool", "}")}
                If you determine that all agents have completed their tasks, please respond with True. Otherwise, respond with False.
                """;

            public static string Selection(string topic, string participants) =>
                $"""
                You are a mediator that orchestrates a group of other agents that each have a different task. 
                You need to select the next participant to speak. 
                Here are the names and descriptions of the participants: 
                {participants}\n
                Please respond with only the name of the participant you would like to select.
                """;

            public static string Filter(string topic) =>
                $"""
                You are a mediator that orchestrates a group of other agents that each have a different task. 
                You have just concluded the discussion. 
                Please summarize the completed tasks.
                """;
        }

        /// <inheritdoc/>
        public override ValueTask<GroupChatManagerResult<string>> FilterResults(ChatHistory history, CancellationToken cancellationToken = default) =>
            this.GetResponseAsync<string>(history, Prompts.Filter(topic), cancellationToken);

        /// <inheritdoc/>
        public override async ValueTask<GroupChatManagerResult<string>> SelectNextAgent(ChatHistory history,
            GroupChatTeam team, CancellationToken cancellationToken = default)
        {
            var result = await this.GetResponseAsync<string>(history, Prompts.Selection(topic, team.FormatList()), cancellationToken);
            Console.WriteLine($"Mediator Requests response from Agent: {result.Value} ");
            return result;
        }

        /// <inheritdoc/>
        public override ValueTask<GroupChatManagerResult<bool>> ShouldRequestUserInput(ChatHistory history, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(new GroupChatManagerResult<bool>(false) { Reason = "The AI group chat manager does not request user input." });

        /// <inheritdoc/>
        public override async ValueTask<GroupChatManagerResult<bool>> ShouldTerminate(ChatHistory history, CancellationToken cancellationToken = default)
        {
            GroupChatManagerResult<bool> result = await base.ShouldTerminate(history, cancellationToken);
            if (!result.Value)
            {
                result = await this.GetResponseAsync<bool>(history, Prompts.Termination(topic), cancellationToken);
            }
            return result;
        }

        private async ValueTask<GroupChatManagerResult<TValue>> GetResponseAsync<TValue>(ChatHistory history, string prompt, CancellationToken cancellationToken = default)
        {
            OpenAIPromptExecutionSettings executionSettings = new() { ResponseFormat = typeof(GroupChatManagerResult<TValue>) };
            ChatHistory request = [.. history, new ChatMessageContent(AuthorRole.System, prompt)];
            ChatMessageContent response = await chatCompletion.GetChatMessageContentAsync(request, executionSettings, kernel: null, cancellationToken);
            string responseText = response.ToString();
            return
                JsonSerializer.Deserialize<GroupChatManagerResult<TValue>>(responseText) ??
                throw new InvalidOperationException($"Failed to parse response: {responseText}");
        }
    }
}
