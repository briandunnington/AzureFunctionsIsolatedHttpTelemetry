
using Element.Azure.WebJobs.Extensions.HttpTelemetry;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: WebJobsStartup(typeof(Startup))]

namespace Element.Azure.WebJobs.Extensions.HttpTelemetry
{
    public class Startup : IWebJobsStartup2
    {
        public void Configure(IWebJobsBuilder builder)
        {
            // wont be called
        }

        public void Configure(WebJobsBuilderContext context, IWebJobsBuilder builder)
        {
            builder.AddExtension<HttpTelemetryExtensionConfigProvider>();
            builder.Services.AddSingleton<ITelemetryInitializer>(p => new HttpTelemetryInitializer());
        }

        public class HttpTelemetryInitializer : ITelemetryInitializer
        {
            public void Initialize(ITelemetry telemetry)
            {
                var rt = telemetry as RequestTelemetry;
                if (rt != null)
                {
                    if (rt.Url != null && int.TryParse(rt.ResponseCode, out var responseCode))
                    {
                        rt.Success = responseCode < 400;
                    }
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding]
    public class HttpTelemetryAttribute : Attribute
    {
    }

    [Extension("HttpTelemetry")]
    internal class HttpTelemetryExtensionConfigProvider : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context)
        {
            var bindingRule = context.AddBindingRule<HttpTelemetryAttribute>();
            bindingRule.BindToInput<object?>(attr => null);
        }
    }
}
