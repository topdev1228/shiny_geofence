using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using Prism.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Shiny;
using Shiny.BluetoothLE.Central;
using XF.Material.Forms.UI.Dialogs;


namespace Samples.BluetoothLE
{
    public class AdapterViewModel : ViewModel
    {
        IDisposable scan;


        public AdapterViewModel(ICentralManager central,
                                INavigationService navigator,
                                IMaterialDialog dialogs)
        {
            this.CanControlAdapterState = central.CanControlAdapterState();

            this.WhenAnyValue(x => x.SelectedPeripheral)
                .Skip(1)
                .Where(x => x != null)
                .SubOnMainThread(x => navigator.Navigate("Peripheral", ("Peripheral", x.Peripheral)));

            this.ToggleAdapterState = ReactiveCommand.CreateFromTask(
                async () =>
                {
                    var poweredOn = central.Status == AccessState.Available;
                    if (!central.TrySetAdapterState(!poweredOn))
                        await dialogs.AlertAsync("Cannot change bluetooth adapter state");
                }
            );

            this.ScanToggle = ReactiveCommand.Create(
                () =>
                {
                    if (this.IsScanning)
                    {
                        this.IsScanning = false;
                        this.scan?.Dispose();
                    }
                    else
                    {
                        this.Peripherals.Clear();

                        this.scan = central
                            .Scan()
                            .Buffer(TimeSpan.FromSeconds(1))
                            .Synchronize()
                            .SubOnMainThread(
                                results =>
                                {
                                    var list = new List<PeripheralItemViewModel>();
                                    foreach (var result in results)
                                    {
                                        var peripheral = this.Peripherals.FirstOrDefault(x => x.Equals(result.Peripheral));
                                        if (peripheral == null)
                                            peripheral = list.FirstOrDefault(x => x.Equals(result.Peripheral));

                                        if (peripheral != null)
                                        {
                                            peripheral.Update(result);
                                        }
                                        else
                                        {
                                            peripheral = new PeripheralItemViewModel(result.Peripheral);
                                            peripheral.Update(result);
                                            list.Add(peripheral);
                                        }
                                    }
                                    if (list.Any())
                                        this.Peripherals.AddRange(list);
                                },
                                ex => dialogs.AlertAsync(ex.ToString(), "ERROR")
                            )
                            .DisposeWith(this.DeactivateWith);

                        this.IsScanning = true;
                    }
                }
            );
        }


        public override void OnDisappearing()
        {
            base.OnDisappearing();
            this.IsScanning = false;
        }


        public ICommand ScanToggle { get; }
        public ICommand ToggleAdapterState { get; }
        public bool CanControlAdapterState { get; }
        public ObservableList<PeripheralItemViewModel> Peripherals { get; } = new ObservableList<PeripheralItemViewModel>();
        [Reactive] public PeripheralItemViewModel SelectedPeripheral { get; set; }
        [Reactive] public bool IsScanning { get; private set; }
    }
}