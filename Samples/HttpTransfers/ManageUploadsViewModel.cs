using System;
using System.IO;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Samples.SqliteGenerator;
using Shiny.IO;
using XF.Material.Forms.UI.Dialogs;


namespace Samples.HttpTransfers
{
    public class ManageUploadsViewModel : ViewModel
    {
        readonly IFileSystem fileSystem;


        public ManageUploadsViewModel(IFileSystem fileSystem, IMaterialDialog dialogs)
        {
            this.fileSystem = fileSystem;

            this.Delete = ReactiveCommand.CreateFromTask<string>(async file =>
            {
                var path = Path.Combine(this.fileSystem.AppData.FullName, file);
                if (!File.Exists(path))
                {
                    await dialogs.SnackbarAsync($"{file} does not exist");
                }
                else
                {
                    File.Delete(path);
                    await dialogs.SnackbarAsync($"{file} has been deleted");
                }
            });
            this.CreateDatabase = ReactiveCommand.CreateFromTask(
                this.GenerateDatabase,
                this.WhenAny(
                    x => x.RecordsToCreate,
                    x => x.GetValue() > 0
                )
            );
            this.BindBusyCommand(this.CreateDatabase);

            this.CreateRandom = ReactiveCommand.CreateFromTask(
                this.GenerateRandom,
                this.WhenAny(
                    x => x.SizeInMegabytes,
                    x => x.GetValue() > 0
                )
            );
            this.BindBusyCommand(this.CreateRandom);
        }


        public IReactiveCommand Delete { get; }
        public IReactiveCommand CreateDatabase { get; }
        public IReactiveCommand CreateRandom { get; }
        [Reactive] public int SizeInMegabytes { get; set; } = 100;
        [Reactive] public int RecordsToCreate { get; set; } = 50000;


        Task GenerateRandom() => Task.Run(() =>
        {
            var path = Path.Combine(this.fileSystem.AppData.FullName, "upload.random");
            var byteSize = this.SizeInMegabytes * 1024 * 1024;
            var data = new byte[8192];
            var rng = new Random();

            using (var fs = new FileStream(path, FileMode.Create))
            {
                while (fs.Length < byteSize)
                {
                    rng.NextBytes(data);
                    fs.Write(data, 0, data.Length);
                    fs.Flush();
                }
            }
        });


        async Task GenerateDatabase()
        {
            var path = Path.Combine(this.fileSystem.AppData.FullName, "upload.db");
            await Generator.CreateSqlite(path, 50000);
        }
    }
}
