using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using Humanizer;
using Prism.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Shiny.Net.Http;
using XF.Material.Forms.UI.Dialogs;


namespace Samples.HttpTransfers
{
    public class PendingViewModel : ViewModel
    {
        readonly IHttpTransferManager httpTransfers;
        readonly IMaterialDialog dialogs;


        public PendingViewModel(INavigationService navigation,
                                IHttpTransferManager httpTransfers,
                                IMaterialDialog dialogs)
        {
            this.httpTransfers = httpTransfers;
            this.dialogs = dialogs;

            this.Create = navigation.NavigateCommand("CreateTransfer");

            this.Load = ReactiveCommand.CreateFromTask(async () =>
            {
                var transfers = await httpTransfers.GetTransfers();
                this.Transfers = transfers
                    .Select(transfer =>
                    {
                        var vm = new HttpTransferViewModel
                        {
                            Identifier = transfer.Identifier,
                            Uri = transfer.Uri,
                            IsUpload = transfer.IsUpload,

                            Cancel = ReactiveCommand.CreateFromTask(async () =>
                            {
                                var confirm = await dialogs.ConfirmAsync("Are you sure you want to cancel all transfers?", "Confirm", "Yes", "No") ?? false;
                                if (confirm)
                                {
                                    await this.httpTransfers.Cancel(transfer.Identifier);
                                    this.Load.Execute(null);
                                }
                            })
                        };

                        ToViewModel(vm, transfer);
                        return vm;
                    })
                    .ToList();
            });
            this.CancelAll = ReactiveCommand.CreateFromTask(async () =>
            {
                await httpTransfers.Cancel();
                this.Load.Execute(null);
            });
            this.BindBusyCommand(this.Load);
        }


        public ICommand Create { get; }
        public ICommand Load { get; }
        public ICommand CancelAll { get; }
        [Reactive] public IList<HttpTransferViewModel> Transfers { get; private set; }


        public override void OnAppearing()
        {
            base.OnAppearing();
            this.Load.Execute(null);

            this.httpTransfers
                .WhenUpdated()
                .WithMetrics()
                .SubOnMainThread(
                    transfer =>
                    {
                        var vm = this.Transfers.FirstOrDefault(x => x.Identifier == transfer.Transfer.Identifier);
                        if (vm != null)
                        {
                            ToViewModel(vm, transfer.Transfer);
                            vm.TransferSpeed = Math.Round(transfer.BytesPerSecond.Bytes().Kilobytes, 2) + " Kb/s";
                            vm.EstimateTimeRemaining = Math.Round(transfer.EstimatedTimeRemaining.TotalMinutes, 1) + " min(s)";
                        }
                    },
                    ex => this.dialogs.AlertAsync(ex.ToString())
                )
                .DisposeWith(this.DeactivateWith);
        }


        static void ToViewModel(HttpTransferViewModel viewModel, HttpTransfer transfer)
        {
            viewModel.PercentComplete = transfer.PercentComplete;
            viewModel.PercentCompleteText = $"{transfer.PercentComplete * 100}%";
            viewModel.Status = transfer.Status.ToString();
        }
    }
}
