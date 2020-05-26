using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using Shiny;
using XF.Material.Forms.UI.Dialogs;


namespace Samples
{
    public static class Extensions
    {
        public static Task ActionSheet(this IMaterialDialog dialogs, bool allowCancel = false, params (string Key, Action Action)[] actions)
        {
            var dict = actions.ToDictionary(
                x => x.Key,
                x => x.Action
            );
            return dialogs.ActionSheet(dict, allowCancel);
        }


        public static async Task ActionSheet(this IMaterialDialog dialogs, IDictionary<string, Action> actions, bool allowCancel = false)
        {
            var task = allowCancel
                ? await dialogs.SelectChoiceAsync("Select", actions.Keys.ToList())
                : await dialogs.SelectActionAsync("Select", actions.Keys.ToList());

            if (task >= 0)
                actions.Values.ElementAt(task).Invoke();
        }


        public static async Task<bool> RequestAccess(this IMaterialDialog dialogs, Func<Task<AccessState>> request)
        {
            var access = await request();
            return await dialogs.AlertAccess(access);
        }


        public static async Task<bool> AlertAccess(this IMaterialDialog dialogs,  AccessState access)
        {
            switch (access)
            {
                case AccessState.Available:
                    return true;

                case AccessState.Restricted:
                    await dialogs.AlertAsync("WARNING: Access is restricted");
                    return true;

                default:
                    await dialogs.AlertAsync("Invalid Access State: " + access);
                    return false;
            }
        }


        public static IDisposable SubOnMainThread<T>(this IObservable<T> obs, Action<T> onNext)
            => obs
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(onNext);


        public static IDisposable SubOnMainThread<T>(this IObservable<T> obs, Action<T> onNext, Action<Exception> onError)
            => obs
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(onNext, onError);


        public static IDisposable SubOnMainThread<T>(this IObservable<T> obs, Action<T> onNext, Action<Exception> onError, Action onComplete)
            => obs
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(onNext, onError, onComplete);
    }
}
