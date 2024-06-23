using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Test3.Models;

public static class OrderProcessingOrchestrator
{
    [FunctionName("OrderProcessingOrchestrator")]
    public static async Task RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        var orderDetails = context.GetInput<OrderDetails>();

        await context.CallActivityAsync("UpdateInventory", orderDetails);
        await context.CallActivityAsync("ProcessPayment", orderDetails);
        await context.CallActivityAsync("SendOrderConfirmation", orderDetails);
    }
}
