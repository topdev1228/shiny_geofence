using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Reactive.Linq;
using Prism.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Shiny;
using Shiny.IO;
using Shiny.Net.Http;
using Samples.Settings;
using XF.Material.Forms.UI.Dialogs;
using System.Collections.Generic;

namespace Samples.HttpTransfers
{
    public class CreateViewModel : ViewModel
    {
        readonly IFileSystem fileSystem;


        public CreateViewModel(INavigationService navigationService,
                               IHttpTransferManager httpTransfers,
                               IMaterialDialog dialogs,
                               IFileSystem fileSystem,
                               AppSettings appSettings)
        {
            this.fileSystem = fileSystem;
            this.Url = appSettings.LastTransferUrl;

            this.WhenAnyValue(x => x.IsUpload)
                .Subscribe(upload =>
                {
                    if (!upload && this.FileName.IsEmpty())
                        this.FileName = Guid.NewGuid().ToString();

                    this.Title = upload ? "New Upload" : "New Download";
                });

            this.ManageUploads = navigationService.NavigateCommand("ManageUploads");

            this.ResetUrl = ReactiveCommand.Create(() =>
            {
                appSettings.LastTransferUrl = null;
                this.Url = appSettings.LastTransferUrl;
            });

            this.SelectUpload = ReactiveCommand.CreateFromTask(async () =>
            {
                var files = fileSystem.AppData.GetFiles("upload.*", SearchOption.TopDirectoryOnly);
                if (!files.Any())
                {
                    await dialogs.AlertAsync("There are not files to upload.  Use 'Manage Uploads' below to create them");
                }
                else
                {
                    var cfg = new Dictionary<string, Action>();
                    foreach (var file in files)
                        cfg.Add(file.Name, () => this.FileName = file.Name);

                    await dialogs.ActionSheet(cfg);
                }
            });
            this.Save = ReactiveCommand.CreateFromTask(
                async () =>
                {
                    var path = Path.Combine(this.fileSystem.AppData.FullName, this.FileName);
                    var request = new HttpTransferRequest(this.Url, path, this.IsUpload)
                    {
                        UseMeteredConnection = this.UseMeteredConnection
                    };
                    await httpTransfers.Enqueue(request);
                    appSettings.LastTransferUrl = this.Url;
	                await navigationService.GoBackAsync();
                },
                this.WhenAny
                (
                    x => x.IsUpload,
                    x => x.Url,
                    x => x.FileName,
                    (up, url, fn) =>
                    {
                        this.ErrorMessage = String.Empty;
                        if (!Uri.TryCreate(url.GetValue(), UriKind.Absolute, out _))
                            this.ErrorMessage = "Invalid URL";

                        else if (up.GetValue() && fn.GetValue().IsEmpty())
                            this.ErrorMessage = "You must select or enter a filename";

                        return this.ErrorMessage.IsEmpty();
                    }
                )
            );
        }


        public ICommand Save { get; }
        public ICommand ResetUrl { get; }
        public ICommand SelectUpload { get; }
        public ICommand ManageUploads { get; }
        [Reactive] public string ErrorMessage { get; private set; }
        [Reactive] public string Url { get; set; }
        [Reactive] public bool UseMeteredConnection { get; set; }
        [Reactive] public bool IsUpload { get; set; }
        [Reactive] public string FileName { get; set; }
    }
}
