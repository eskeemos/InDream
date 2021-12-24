using Binance.Net;
using Binance.Net.Objects;
using CryptoExchange.Net.Authentication;
using Microsoft.Extensions.Configuration;
using NLog;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Providers;
using System.Security.Permissions;
using System.Tactics;
using System.Templates;
using System.Threading.Tasks;

namespace InDream
{
    class Program
    {
        
        #region Variables

        private static readonly string fileName = $"InDream-{DateTime.UtcNow:ddMMyyyy}.log";
        private static ISchedulerFactory schedulerFactory;
        private static IScheduler scheduler;
        private static readonly Logger logger = LogManager.GetLogger("INDREAM");
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]

        #endregion

        static async Task<int> Main()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);

            NLog.LogManager.Configuration.Variables["fileName"] = fileName;
            NLog.LogManager.Configuration.Variables["archiveFileName"] = fileName;

            var configData = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"indream.json");

            var config = configData.Build();

            var app = config.Get<App>();

            try
            {
                var services = Dependency.Get(app);

                var serviceJob = new Job(services);

                schedulerFactory = new StdSchedulerFactory();

                scheduler = await schedulerFactory.GetScheduler();
                scheduler.JobFactory = serviceJob;

                await scheduler.Start();

                #region Exchange

                var exchange = (app.Exchanges as IList<Exchange>).FirstOrDefault();

                if (exchange.KeysProvided)
                {
                    BinanceClient.SetDefaultOptions(new BinanceClientOptions()
                    {
                        ApiCredentials = new ApiCredentials(exchange.ApiKey, exchange.ApiSecret)
                    });
                }

                #endregion

                #region Tactic

                IJobDetail job = JobBuilder.Create<PriceFluctuations>()
                    .WithIdentity("PriceFluctuations")
                    .Build();

                job.JobDataMap["Main"] = app.Main;
                job.JobDataMap["Exchanges"] = app.Exchanges;

                var triggerData = TriggerBuilder.Create()
                    .WithIdentity("PriceFluctuations")
                    .StartNow();

                triggerData.WithSimpleSchedule(task => task
                    .WithIntervalInMinutes(app.Main.Interval)
                    .RepeatForever());

                var trigger = triggerData.Build();

                #endregion

                await scheduler.ScheduleJob(job, trigger);

                await Task.Delay(TimeSpan.FromSeconds(30));

                Console.ReadKey();
            }
            catch (Exception e)
            {
                logger.Fatal($"{e.Message}");
            }

            NLog.LogManager.Shutdown();

            return 0;
        }
        static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            logger.Fatal($"{e.Message}");
            logger.Fatal($"{args.IsTerminating}");
        }
    }
}
