using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using BACApp.UI.Extensions;

namespace BACApp.UI;

internal sealed partial class SingleInstanceCoordinator : IDisposable
{
    private readonly string _appId;
    private readonly string _mutexName;
    private readonly string _pipeName;
    private readonly Mutex _mutex;
    private readonly bool _ownsMutex;

    public SingleInstanceCoordinator(string appId)
    {
        _appId = appId ?? throw new ArgumentNullException(nameof(appId));

        // Keep names stable and OS-friendly.
        // Global\ is Windows-only; omit it for cross-platform.
        _mutexName = $"{_appId}.singleinstance";
        _pipeName = $"{_appId}.activation";

        _mutex = new Mutex(initiallyOwned: true, name: _mutexName, createdNew: out _ownsMutex);
    }

    public bool IsPrimaryInstance => _ownsMutex;

    public async Task<int> SendActivationAsync(ActivationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await using var client = new NamedPipeClientStream(
                serverName: ".",
                pipeName: _pipeName,
                direction: PipeDirection.Out,
                options: PipeOptions.Asynchronous);

            // Small timeout so a wedged server doesn't hang startup.
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(750));

            await client.ConnectAsync(timeoutCts.Token).ConfigureAwait(false);

            await using var writer = new StreamWriter(client) { AutoFlush = true };
            await writer.WriteLineAsync(request.ToWireValue()).ConfigureAwait(false);

            return 0;
        }
        catch
        {
            // If we can't reach the primary, let this instance continue (fallback).
            // Returning non-zero allows caller to decide.
            return 2;
        }
    }

    public Task RunServerAsync(Func<ActivationRequest, Task> onRequest, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(onRequest);

        return Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await using var server = new NamedPipeServerStream(
                        pipeName: _pipeName,
                        direction: PipeDirection.In,
                        maxNumberOfServerInstances: 1,
                        transmissionMode: PipeTransmissionMode.Byte,
                        options: PipeOptions.Asynchronous);

                    await server.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);

                    using var reader = new StreamReader(server);
                    var line = await reader.ReadLineAsync().ConfigureAwait(false);

                    if (ActivationRequestExtensions.TryParse(line, out var request))
                    {
                        await onRequest(request).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    // Ignore and keep server alive.
                }
            }
        }, cancellationToken);
    }

    public void Dispose()
    {
        if (_ownsMutex)
        {
            try { _mutex.ReleaseMutex(); } catch { }
        }

        _mutex.Dispose();
    }

    internal enum ActivationRequest
    {
        Activate,
        Maximize,
    }
}