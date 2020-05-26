using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Shiny;
using Shiny.Nfc;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Threading.Tasks;
using System.Windows.Input;


namespace Samples.Nfc
{
    public class NfcViewModel : ViewModel
    {
        readonly INfcManager nfcManager;


        public NfcViewModel(INfcManager nfcManager = null)
        {
            this.nfcManager = nfcManager;

            this.CheckPermission = ReactiveCommand.CreateFromTask(async () =>
                this.Access = await nfcManager.RequestAccess().ToTask()
            );

            this.Clear = ReactiveCommand.Create(() =>
                this.ChangeRecords(() => this.NDefRecords.Clear())
            );

            this.Listen = ReactiveCommand.Create(() =>
            {
                if (this.IsListening)
                {
                    this.IsListening = false;
                    this.Deactivate();
                }
                else
                {
                    this.nfcManager
                        .Reader()
                        .SubOnMainThread(x =>
                            this.ChangeRecords(() => this.NDefRecords.AddRange(x))
                        )
                        .DisposeWith(this.DeactivateWith);
                    this.IsListening = true;
                }
            });
        }


        void ChangeRecords(Action action)
        {
            lock (this.NDefRecords)
                action();

            this.RaisePropertyChanged(nameof(NDefRecords));
        }


        public ICommand Clear { get; }
        public ICommand Listen { get; }
        public ICommand CheckPermission { get; }
        public List<NDefRecord> NDefRecords { get; } = new List<NDefRecord>();
        [Reactive] public AccessState Access { get; private set; } = AccessState.Unknown;
        [Reactive] public bool IsListening { get; private set; }
    }
}
