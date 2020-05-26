using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Shiny.Caching;


namespace Samples.Caching
{
    public class CacheViewModel : ViewModel
    {
        const string CacheKey = "Test";
        readonly ICache cache;


        public CacheViewModel(ICache cache)
        {
            this.cache = cache;

            this.Set = ReactiveCommand.Create(() =>
            {
                this.cache.Set(
                    DateTime.Now.ToString(),
                    TimeSpan.FromSeconds(this.CacheSeconds)
                );
                this.DisplayManualCache();
            });
            this.Remove = ReactiveCommand.Create(() =>
            {
                this.cache.Remove(CacheKey);
                this.DisplayManualCache();
            });
        }


        [Reactive] public string AutoCacheDate { get; private set; }
        [Reactive] public string ManualCacheDate { get; private set; }
        [Reactive] public int CacheSeconds { get; set; } = 60;
        public ICommand Set { get; }
        public ICommand Remove { get; }


        public override void OnAppearing()
        {
            base.OnAppearing();
            this.DisplayManualCache();
            this.DisplayAutoCache();

            Observable
                .Interval(TimeSpan.FromSeconds(3))
                .SubOnMainThread(_ => {
                    this.DisplayAutoCache();
                    this.DisplayManualCache();
                })
                .DisposeWith(this.DeactivateWith);
        }


        async void DisplayManualCache()
        {
            this.ManualCacheDate = await this.cache.Get<string>(nameof(ManualCacheDate)) ?? "Nothing Cached";
        }


        async void DisplayAutoCache()
        {
            this.AutoCacheDate = await this.cache.TryGet("MyData", () =>
            {
                return Task.FromResult(DateTime.Now.ToString());
            }, TimeSpan.FromSeconds(10)) ?? "Nothing Cached";
        }
    }
}
