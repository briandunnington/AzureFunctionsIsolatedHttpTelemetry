# AzureFunctionsIsolatedHttpTelemetry
Provides standard HTTP request logging handling for Azure Functions running in Isolated mode so that 4xx and 5xx responses are logged as Failures in AppInsights.

## Usage
Add the [Element.Azure.Functions.Worker.Extensions.HttpTelemetry](https://www.nuget.org/packages/Element.Azure.Functions.Worker.Extensions.HttpTelemetry) Nuget package to your Isolated Azure Functions project.

**NOTE**: Due to how the Azure Functions metadata generator works, you must actually use the extension in a function declaration. To meet that requirement, you must add the `[HttpTelemetry]` input binding attribute to **any one** of your HttpTrigger functions (does **not** need to be all of them):

``` csharp
[Function("Ping")]
public HttpResponseData Ping([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ping")] HttpRequestData req, [HttpTelemetry] object ignore)
{
    // ... your code ...
}
```
