using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Scheme.Services.Implementations;
using Scheme.Services.Interfaces;
using System.Linq;
using System.Tactics;
using System.Templates;

namespace System.Providers
{
    public class Dependency
    {
        #region Public

        public static IServiceProvider Get(App app)
        {
            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });
            });

            services.AddTransient<PriceFluctuations>();
            services.AddTransient<IStorage, Storage>();
            services.AddTransient<IMarket, Market>();

            return services.BuildServiceProvider();
        }

        #endregion
    }
}
