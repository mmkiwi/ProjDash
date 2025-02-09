using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Serialization;

using Avalonia.Controls;

using ReactiveUI;

using Serilog;

namespace MMKiwi.ProjDash.GUI;

internal static class WindowPipe
{
    const string PipeName = "MMKiwi.ProjDash";

    [field: MaybeNull]
    public static IObservable<Unit> ShowWindowMessage => field ??= Observable.Create<Unit>(async (c, t) =>
    {
        while (!t.IsCancellationRequested)
        {
            NamedPipeServerStream pipeServer = new(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);
            await using (pipeServer.ConfigureAwait(false))
            {
                // Wait for a client to connect
                await pipeServer.WaitForConnectionAsync(t).ConfigureAwait(false);

                using var reader = new StreamReader(pipeServer);
                switch (await reader.ReadToEndAsync(t).ConfigureAwait(false))
                {
                    case "ShowWindow":
                        c.OnNext(Unit.Default);
                        break;
                }
            }
        }
    });

    public static async Task SendMessageAsync()
    {
        var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);

        await using (client.ConfigureAwait(false))
        {
            await client.ConnectAsync().ConfigureAwait(false);
            StreamWriter writer = new(client);
            await using (writer.ConfigureAwait(false))
            {
                await writer.WriteAsync("ShowWindow").ConfigureAwait(false);
            }
        }
    }
}