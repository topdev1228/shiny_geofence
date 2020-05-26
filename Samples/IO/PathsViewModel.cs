using System;
using Shiny.IO;


namespace Samples.IO
{
    public class PathsViewModel : ViewModel
    {
        readonly IFileSystem fileSystem;
        public PathsViewModel(IFileSystem fileSystem) => this.fileSystem = fileSystem;


        public string AppData => this.fileSystem.AppData.FullName;
        public string Cache => this.fileSystem.Cache.FullName;
        public string Public => this.fileSystem.Public.FullName;
    }
}
