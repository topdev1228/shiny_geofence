using System;
using System.Reactive.Disposables;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Shiny.Vpn;
using XF.Material.Forms.UI.Dialogs;


namespace Samples.Vpn
{
    public class MainViewModel : ViewModel
    {
        readonly IVpnManager? vpnManager;


        public MainViewModel(IMaterialDialog dialogs, IVpnManager? vpnManager = null)
        {
            this.vpnManager = vpnManager;

            this.ToggleConnection = ReactiveCommand.CreateFromTask(async () =>
            {
                if (this.vpnManager == null)
                {
                    await dialogs.AlertAsync("VPN Management is not supported on this platform");
                    return;
                }
                if (this.vpnManager.Status != VpnConnectionState.Disconnected)
                {
                    await this.vpnManager.Disconnect();
                }
                else
                { 
                    await this.vpnManager.Connect(new VpnConnectionOptions(this.ServerAddress)
                    {
                        UserName = this.UserName,
                        Password = this.Password
                    });
                }
            });
        }


        public ICommand ToggleConnection { get; }
        [Reactive] public string ConnectText { get; private set; }
        [Reactive] public string ConnectionStatus { get; private set; }
        [Reactive] public string ServerAddress { get; set; }
        [Reactive] public string UserName { get; set; }
        [Reactive] public string Password { get; set; }


        public override void OnAppearing()
        {
            base.OnAppearing();
            this.vpnManager?
                .WhenStatusChanged()
                .SubOnMainThread(x => this.ConnectionStatus = x.ToString())
                .DisposeWith(this.DeactivateWith);
        }
    }
}
