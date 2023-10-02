using CorrelationIdPoc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: WebJobsStartup(typeof(CorrelationIdFilterWebJobsStartup))]
namespace CorrelationIdPoc
{
    public class CorrelationIdFilterWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            builder.Services.AddSingleton<IFunctionFilter, CorrelationIdFilterAttribute>();
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
