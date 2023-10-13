# HTTP success/failure logging for Isolated Azure Functions
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

## How does it work?

As far as functions are concerned, there is no difference between and `HttpTrigger` and any other function trigger type. So even if your `HttpTrigger` returns a 4xx or 5xx error, the *function itself* was still successful (it returned exactly what you told it to do without error). So in AppInsights, these request show up as `Success` regardless of the HTTP response status code.

In ASP.NET Core and other web app technologies, HTTP response codes like this are treated as unsuccessful and hence show up in AppInsights logs in the 'Failures' blade. This package aims to make isolated functions log HTTP requests in this manner.

In the in-process model, the solution was to register an `ITelemetryInitializer` with the `IServiceCollection` and then you could watch for `RequestTelemetry` and modify the `Success` property based on the response code.

However, in the the isolated functions model, the functions host and your functions code run as two separate processes. The functions host recieves the incoming request then communicates with your worker process via gRPC. Although some telemetry data can be handled in the worker, only the host handles the `RequestTelemetry`. So even if you register an `ITelemetryInitializer` in your worker, you will never receive any `RequestTelemetry` events. The only way to get access to the `RequestTelemetry` is to do it from the host.

Unfortunately, the code you write in your function app only runs in the worker process and there is no way to 'reach into' the host to modify its behavior.

Except, there is. And it is via Binding Extensions.

Binding extensions are ways to add your own custom trigger types, input bindings, and output bindings. In order to allow them to work, they also have to run in the host. So this solution creates a custom extension that allows code (in this case, code to register the `ITelemetryInitializer` and convert the `RequestTelemetry` `Success` property) to run on the host.

One limitation of extensions is that they are only geared towards bindings. As such, your function app code must make use of at least one binding from your extension or else the function extension metadata generator will strip out the extension information from the generated host code. That is why the `[HttpTelemetry]` input binding attribute needs to be applied _somewhere_ in a function in your function app - it doesnt do anything, other than prevent the rest of the extension code from being complied out.

The actual mechanisms of how extensions work is:
- you add a reference to a worker extension library (in this case: `Element.Azure.Functions.Worker.Extensions.HttpTelemetry` available via [Nuget](https://www.nuget.org/packages/Element.Azure.Functions.Worker.Extensions.HttpTelemetry))
- that worker extension library has an `[ExtensionInformation]` attribute that points to *a different* Nuget package that contains the actual logic which will be injected into the host (in this case: `Element.Azure.WebJobs.Extensions.HttpTelemetry`, which is also available on [Nuget](https://www.nuget.org/packages/Element.Azure.WebJobs.Extensions.HttpTelemetry) but should not be referenced directly from your function app)
- when you compile your function app, the function metadata generator scans your code for binding extensions, creates a temporary `.csproj` file, and outputs the necessary .dlls and and emits an `extensions.json` file with metadata about your extension and the entry points

You don't _need_ to know any of that in order to take advantage of this package, but it might be interesting to know what is going on under the hood.

### References

Credit goes to [Maarten Balliauw](https://github.com/maartenba) for [documenting how to write custom extensions for isolated functions](https://blog.maartenballiauw.be/post/2021/06/01/custom-bindings-with-azure-functions-dotnet-isolated-worker.html). It was invaluable in understanding how things worked internally.

The [source code for the functions metadata generator](https://github.com/Azure/azure-functions-dotnet-worker/blob/bba8136917f2a60d884387182fca35ed19aaf8e4/sdk/FunctionMetadataLoaderExtension/Startup.cs) (where you can see how items are stripped out if not used) is also interesting reading.

The [source code for the Microsoft-provided extensions](https://github.com/Azure/azure-functions-dotnet-worker/tree/main/extensions) was also a great reference, as was the [Dapr extension source code](https://github.com/Azure/azure-functions-dapr-extension/tree/master/src).
