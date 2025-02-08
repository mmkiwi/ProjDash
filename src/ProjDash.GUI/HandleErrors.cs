using System.Diagnostics;
using System.Reactive.Concurrency;

using ReactiveUI;

using Splat;

namespace MMKiwi.ProjDash.GUI;

internal class HandleErrors : IObserver<Exception>, IEnableLogger
{
    public void OnNext(Exception value)
    {
        if (Debugger.IsAttached) Debugger.Break();

        this.Log().Fatal(value, "Unhandled exception");

        RxApp.MainThreadScheduler.Schedule(() => throw value);
    }

    public void OnError(Exception error)
    {
        if (Debugger.IsAttached) Debugger.Break();

        this.Log().Fatal(error, "Unhandled exception");

        RxApp.MainThreadScheduler.Schedule(() => throw error);
    }

    public void OnCompleted()
    {
    }
}