using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

namespace System.Providers
{
    public class Job : IJobFactory
    {
        #region Variables

        protected readonly IServiceScope scope;

        #endregion

        #region Constructor

        public Job(IServiceProvider container)
        {
            this.scope = container.CreateScope();
        }

        #endregion

        #region Implemented functions

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
            => scope.ServiceProvider.GetService(bundle.JobDetail.JobType) as IJob;

        public void ReturnJob(IJob job)
        {
            (job as IDisposable)?.Dispose();
        }

        #endregion

        #region Public

        public void Dispose()
        {
            scope.Dispose();
        }

        #endregion
    }
}