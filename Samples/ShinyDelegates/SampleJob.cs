using System;
using System.Threading;
using System.Threading.Tasks;
using Shiny;
using Shiny.Jobs;


namespace Samples.ShinyDelegates
{
    public class SampleJob : IJob
    {
        readonly CoreDelegateServices services;
        public SampleJob(CoreDelegateServices services) => this.services = services;


        public async Task<bool> Run(JobInfo jobInfo, CancellationToken cancelToken)
        {
            await this.services.SendNotification(
                "Job Started",
                $"{jobInfo.Identifier} Started",
                x => x.UseNotificationsJobStart
            );
            var seconds = jobInfo.Parameters.Get("SecondsToRun", 10);
            await Task.Delay(TimeSpan.FromSeconds(seconds), cancelToken);

            await this.services.SendNotification(
                "Job Finished",
                $"{jobInfo.Identifier} Finished",
                x => x.UseNotificationsJobFinish
            );

            // you really shouldn't lie about this on iOS as it is watching :)
            return true;
        }
    }
}
