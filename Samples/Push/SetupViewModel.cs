﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Shiny;
using Shiny.Push;
using XF.Material.Forms.UI.Dialogs;


namespace Samples.Push
{
    public class SetupViewModel : ViewModel
    {
        readonly IPushManager? pushManager;
        readonly IMaterialDialog dialogs;


        public SetupViewModel(IMaterialDialog dialogs, IPushManager? pushManager = null)
        {
            this.dialogs = dialogs;
            this.pushManager = pushManager;

            this.RequestAccess = ReactiveCommand.CreateFromTask(
                () => this.Do(async () =>
                {
                    var result = await this.pushManager.RequestAccess();
                    this.AccessStatus = result.Status;
                })
            );
            this.UnRegister = ReactiveCommand.CreateFromTask(
                () => this.Do(async () =>
                {
                    await this.pushManager.UnRegister();
                    this.AccessStatus = AccessState.Disabled;
                }),
                this.WhenAny(
                    x => x.RegToken,
                    x => !x.GetValue().IsEmpty()
                )
            );
            this.UpdateTag = ReactiveCommand.CreateFromTask(
                () => this.Do(() => this.pushManager.TryUpdateTags(this.Tag)),
                this.WhenAny(
                    x => x.Tag,
                    x => x.RegToken,
                    (tag, token) =>
                        this.pushManager?.IsTagsSupport() ?? false &&
                        !tag.GetValue().IsEmpty() &&
                        !token.GetValue().IsEmpty()
                )
            );
        }


        public ICommand RequestAccess { get; }
        public ICommand UnRegister { get; }
        public ICommand UpdateTag { get; }

        public bool IsTagsSupported => this.pushManager?.IsTagsSupport() ?? false;
        [Reactive] public string Tag { get; set; }
        [Reactive] public string RegToken { get; private set; }
        [Reactive] public DateTime? RegDate { get; private set; }
        [Reactive] public DateTime? ExpiryDate { get; private set; }
        [Reactive] public AccessState AccessStatus { get; private set; } = AccessState.Unknown;


        public override void OnAppearing()
        {
            base.OnAppearing();
            this.Refresh();
        }


        void Refresh()
        {
            this.RegToken = this.pushManager?.CurrentRegistrationToken ?? "-";
            this.RegDate = this.pushManager?.CurrentRegistrationTokenDate?.ToLocalTime();
            this.ExpiryDate = this.pushManager?.CurrentRegistrationExpiryDate?.ToLocalTime();
            this.Tag = this.pushManager?.TryGetTags()?.FirstOrDefault() ?? String.Empty;
        }


        async Task Do(Func<Task> task)
        {
            if (this.pushManager == null)
                return;

            //using (this.dialogs.LoadingDialogAsync("Updating Push Details"))
            //using (this.dialogs.LoadingSnackbarAsync("Updating Push Details"))
                await task();

            await this.dialogs.SnackbarAsync("Push Details Updated");
            this.Refresh();
        }
    }
}
