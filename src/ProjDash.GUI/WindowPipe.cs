// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;
using System.Reactive;
using System.Reactive.Linq;

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