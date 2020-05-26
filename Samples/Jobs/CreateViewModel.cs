using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Prism.Navigation;
using Shiny;
using Shiny.Jobs;
using Samples.ShinyDelegates;
using Shiny.Notifications;
using XF.Material.Forms.UI.Dialogs;
using System.Collections.Generic;
using System.Linq;

namespace Samples.Jobs
{
    public class CreateViewModel : ViewModel
    {
        readonly IJobManager jobManager;
        readonly IMaterialDialog dialogs;


        public CreateViewModel(IJobManager jobManager,
                               INavigationService navigator,
                               INotificationManager notifications,
                               IMaterialDialog dialogs)
        {
            this.jobManager = jobManager;
            this.dialogs = dialogs;

            var valObs = this.WhenAny(
                x => x.JobName,
                x => x.SecondsToRun,
                (name, seconds) =>
                    !name.GetValue().IsEmpty() &&
                    seconds.GetValue() >= 10
            );

            this.CreateJob = ReactiveCommand.CreateFromTask(
                async _ =>
                {
                    var job = new JobInfo(typeof(SampleJob), this.JobName.Trim())
                    {
                        Repeat = this.Repeat,
                        BatteryNotLow = this.BatteryNotLow,
                        DeviceCharging = this.DeviceCharging,
                        RunOnForeground = this.RunOnForeground,
                        RequiredInternetAccess = (InternetAccess)Enum.Parse(typeof(InternetAccess), this.RequiredInternetAccess)
                    };
                    job.SetParameter("SecondsToRun", this.SecondsToRun);
                    await this.jobManager.Schedule(job);
                    await navigator.GoBack();
                },
                valObs
            );

            this.RunAsTask = ReactiveCommand.Create(
                () => this.jobManager.RunTask(this.JobName + "Task", async _ =>
                {
                    await notifications.Send("Shiny", $"Task {this.JobName} Started");
                    var ts = TimeSpan.FromSeconds(this.SecondsToRun);
                    await Task.Delay(ts);
                    await notifications.Send("Shiny", $"Task {this.JobName} Finshed");
                }),
                valObs
            );

            this.ChangeRequiredInternetAccess = ReactiveCommand.CreateFromTask(async () =>
            {
                var cfg = new Dictionary<string, Action>
                {
                    {
                        InternetAccess.None.ToString(),
                        () => this.RequiredInternetAccess = InternetAccess.None.ToString()
                    },
                    {
                        InternetAccess.Any.ToString(),
                        () => this.RequiredInternetAccess = InternetAccess.Any.ToString()
                    },
                    {
                        InternetAccess.Unmetered.ToString(),
                        () => this.RequiredInternetAccess = InternetAccess.Any.ToString()
                    }
                };

                var task = await this.dialogs.SelectChoiceAsync("Select", cfg.Keys.ToList());
                cfg.Values.ElementAt(task).Invoke();
            });
        }


        public ICommand CreateJob { get; }
        public ICommand RunAsTask { get; }
        public ICommand ChangeRequiredInternetAccess { get; }
        [Reactive] public string AccessStatus { get; private set; }
        [Reactive] public string JobName { get; set; } = "TestJob";
        [Reactive] public int SecondsToRun { get; set; } = 10;
        [Reactive] public string RequiredInternetAccess { get; set; } = InternetAccess.None.ToString();
        [Reactive] public bool BatteryNotLow { get; set; }
        [Reactive] public bool DeviceCharging { get; set; }
        [Reactive] public bool Repeat { get; set; } = true;
        [Reactive] public bool RunOnForeground { get; set; }


        public override async void OnAppearing()
        {
            base.OnAppearing();
            var r = await this.jobManager.RequestAccess();
            this.AccessStatus = r.ToString();
        }
    }
}
