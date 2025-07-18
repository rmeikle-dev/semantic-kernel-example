using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace SemanticKernelTest.Plugins
{
    public class TaskPlugins
    {
        // Static list to store all FlowStatus objects
        private static readonly List<FlowStatus> flowStatuses = new()
        {
            new FlowStatus { Status = "Initial contract received and parsed" },
            new FlowStatus { Status = "Updated contract received and parsed" },
         //   new FlowStatus { Status = "Contracts compared - The differences between the contracts are as follows.  Price increase by 8%.  Increased obligation on customer to report data usage" },
        //    new FlowStatus { Status = "Stakeholders notified - need to check if they have replied" },
        };

        [KernelFunction("update_status")]
        [Description("Update status of workflow")]
        public FlowStatus UpdateStatus(FlowStatus status)
        {
            flowStatuses.Add(status);
            return status;
        }

        [KernelFunction("get_status")]
        [Description("Gets the current status of the workflow")]
        public List<FlowStatus> GetStatus(int id)
        {
            return flowStatuses;
        }

        [KernelFunction("get_contracts")]
        [Description("Gets the contracts associated with current workflow")]
        public List<object> GetContracts(int id)
        {
            return new List<object>()
            {
                new { ContractId = 1, StartDate = new DateTime(2020, 1, 1), EndDate = new DateTime(2020, 12, 31) },
                new { ContractId = 2, StartDate = new DateTime(2021, 1, 1), EndDate = new DateTime(2021, 12, 31) },
                new { ContractId = 3, StartDate = new DateTime(2022, 1, 1), EndDate = new DateTime(2022, 12, 31) },
                new { ContractId = 4, StartDate = new DateTime(2023, 1, 1), EndDate = new DateTime(2023, 12, 31) },
                new { ContractId = 5, StartDate = new DateTime(2024, 1, 1), EndDate = new DateTime(2024, 12, 31) },
            };
        }

        [KernelFunction("compare_contracts")]
        [Description("Retrieves and compares 2 contracts to understand the differences between them")]
        public string CompareContracts(int contractId1, int contractId2)
        {
            return "The differences between the contracts are as follows.  Price increase by 8%.  Increased obligation on customer to report data usage";
        }

        [KernelFunction("get_stakeholders")]
        [Description("Get stakeholders for workflow")]
        public List<StakeHolder> GetStakeholders(List<StakeHolder> stakeholders, string message)
        {
            return new List<StakeHolder>
            {
                new StakeHolder { Name = "Alice Johnson", Email = "alice.johnson@example.com" },
                new StakeHolder { Name = "Bob Smith", Email = "bob.smith@example.com" },
                new StakeHolder { Name = "Carol Lee", Email = "carol.lee@example.com" }
            };
        }

        [KernelFunction("notify_stakeholders")]
        [Description("Notifies stake holders of a message")]
        public string NotifyStakeholders(List<StakeHolder> stakeholders, string message)
        {
            return "Stakeholders notified";
        }

        [KernelFunction("get_stakeholder_notification_status")]
        [Description("Retrieves the notification status for each stakeholder in the workflow.  Use this to check the status of outstanding notifications")]
        public List<StakeHolderStatus> GetNotificationStatus(int workflowId)
        {
            var statusComplete = true;
            return statusComplete ? new List<StakeHolderStatus>
            {
                new StakeHolderStatus { Name = "Alice Johnson", Email = "alice.johnson@example.com", Status = "Complete", Comment = "I think the new contract differences are fine, we should renew"},
                new StakeHolderStatus { Name = "Bob Smith", Email = "bob.smith@example.com", Status = "Complete", Comment = "Increase in price is not worth it for this service - i dont use it much" },
                new StakeHolderStatus { Name = "Carol Lee", Email = "carol.lee@example.com", Status = "Complete", Comment = "Yes definitely renew - i use this service everyday!" }
            } :
            new List<StakeHolderStatus>
            {
                new StakeHolderStatus { Name = "Alice Johnson", Email = "alice.johnson@example.com", Status = "Pending" },
                new StakeHolderStatus { Name = "Bob Smith", Email = "bob.smith@example.com", Status = "Pending" },
                new StakeHolderStatus { Name = "Carol Lee", Email = "carol.lee@example.com", Status = "Pending" }
            };
        }


       
    }

    public class FlowStatus
    {
        public string Status { get; set; } = string.Empty;
    }

    public class StakeHolder
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class StakeHolderStatus: StakeHolder
    {
        public string Status { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
    }
}
