using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

[assembly: ExtensionInformation("Element.Azure.WebJobs.Extensions.HttpTelemetry", "1.0.3")]

namespace Element.Azure.Functions.Worker.Extensions.HttpTelemetry
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class HttpTelemetryAttribute : InputBindingAttribute
    {
    }
}