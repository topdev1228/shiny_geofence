using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using Humanizer;
using Prism.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Shiny;
using Shiny.IO;
using XF.Material.Forms.UI.Dialogs;


namespace Samples.IO
{
    public class FileEntryViewModel : ReactiveObject
    {
        public FileSystemInfo Entry { get; set; }
        public string Name => this.Entry.Name;
        public string Size
        {
            get
            {
                if (this.IsDirectory)
                {
                    var dc = Directory.GetDirectories(this.Entry.FullName).Length;
                    var fc = Directory.GetFiles(this.Entry.FullName).Length;
                    return $"{dc} Dir(s) - {fc} File(s)";
                }
                return new FileInfo(this.Entry.FullName).Length.Bytes().Megabytes + " MB";
            }
        }

        public bool IsDirectory => this.Entry.Attributes.HasFlag(FileAttributes.Directory);
        public ICommand Actions { get; set; }
    }


    public class FileManagerViewModel : ViewModel
    {
        readonly IFileSystem fileSystem;
        readonly IMaterialDialog dialogs;


        public FileManagerViewModel(IMaterialDialog dialogs, IFileSystem fileSystem)
        {
            this.dialogs = dialogs;
            this.fileSystem = fileSystem;

            this.Select = ReactiveCommand.CreateFromTask<FileEntryViewModel>(async entry =>
            {
                var cfg = new Dictionary<string, Action>();

                if (entry.IsDirectory)
                {
                    cfg.Add("Enter", () => this.CurrentPath = entry.Entry.FullName);
                }
                else
                {
                    cfg.Add("View", async () =>
                    {
                        var text = File.ReadAllText(entry.Entry.FullName);
                        await this.dialogs.AlertAsync(text, entry.Entry.Name);
                    });
                    //cfg.Add("Copy", () =>
                    //{
                    //    //var progress = dialogs.Progress(new ProgressDialogConfig
                    //    //{
                    //    //    Title = "Copying File"
                    //    //});

                    //    var fn = Path.GetFileNameWithoutExtension(entry.Entry.Name);
                    //    var ext = Path.GetExtension(entry.Entry.Name);
                    //    var newFn = ext.IsEmpty() ? $"fn_1" : $"{fn}_1.{ext}";
                    //    var target = new FileInfo(newFn);
                    //    var file = new FileInfo(entry.Entry.FullName);
                    //    file
                    //        .CopyProgress(target, true)
                    //        .Subscribe(p =>
                    //        {
                    //            //progress.Title = "Copying File - Seconds Left: " + p.TimeRemaining.TotalSeconds;
                    //            //progress.PercentComplete = p.PercentComplete;
                    //        });
                    //});
                }
                //cfg.Add("Delete", () => Confirm("Delete " + entry.Name, entry.Entry.Delete));
                await dialogs.ActionSheet(cfg, true);
            });

            this.showBack = this.WhenAnyValue(x => x.CurrentPath)
                .Select(x => x == this.CurrentPath)
                .ToProperty(this, x => x.ShowBack);

            this.WhenAnyValue(x => x.CurrentPath)
                .Skip(1)
                .Subscribe(_ => this.LoadEntries())
                .DisposeWith(this.DestroyWith);
        }


        public override void Initialize(INavigationParameters parameters)
        {
            base.Initialize(parameters);
            this.CurrentPath = this.fileSystem.AppData.FullName;
        }


        public ICommand Select { get; }
        public ICommand Back { get; }
        [Reactive] public bool HasEntries { get; private set; }
        [Reactive] public string CurrentPath { get; private set; }
        public ObservableList<FileEntryViewModel> Entries { get; } = new ObservableList<FileEntryViewModel>();

        readonly ObservableAsPropertyHelper<bool> showBack;
        public bool ShowBack => this.showBack.Value;


        async void Confirm(string message, Action action)
        {
            var result = await this.dialogs.ConfirmAsync(message, null, "Yes", "No") ?? false;
            if (result)
                action();
        }


        void LoadEntries()
        {
            var dir = new DirectoryInfo(this.CurrentPath);
            this.Title = Path.GetDirectoryName(this.CurrentPath);

            this.Entries.ReplaceAll(
                dir
                    .GetFileSystemInfos()
                    .Select(x => new FileEntryViewModel { Entry = x })
            );
            this.HasEntries = this.Entries.Any();
        }
    }
}
