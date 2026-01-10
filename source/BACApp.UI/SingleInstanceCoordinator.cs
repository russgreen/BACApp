using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using BACApp.UI.Extensions;

namespace BACApp.UI;

internal sealed partial class SingleInstanceCoordinator : IDisposable
{
    private readonly string _pipeName;
    private readonly FileStream? _lockStream;

    public SingleInstanceCoordinator(string appId)
    {
        if (string.IsNullOrWhiteSpace(appId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(appId));
        }

        _pipeName = $"{SanitizeName(appId)}.activation";
        _lockStream = TryAcquireLockStream(appId);
    }

    public bool IsPrimaryInstance => _lockStream is not null;

    public async Task<int> SendActivationAsync(ActivationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Retry briefly to allow the primary to finish starting the server.
            for (var attempt = 0; attempt < 12; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await using var client = new NamedPipeClientStream(
                        serverName: ".",
                        pipeName: _pipeName,
                        direction: PipeDirection.Out,
                        options: PipeOptions.Asynchronous);

                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(250));

                    await client.ConnectAsync(timeoutCts.Token).ConfigureAwait(false);

                    await using var writer = new StreamWriter(client) { AutoFlush = true };
                    await writer.WriteLineAsync(request.ToWireValue()).ConfigureAwait(false);

                    return 0;
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    // connect timeout; retry
                }
                catch (IOException)
                {
                    // server not ready; retry
                }

                await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken).ConfigureAwait(false);
            }

            return 3;
        }
        catch
        {
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
        _lockStream?.Dispose();
    }

    private static FileStream? TryAcquireLockStream(string appId)
    {
        var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        if (string.IsNullOrWhiteSpace(baseDir))
        {
            baseDir = AppContext.BaseDirectory;
        }

        var dir = Path.Combine(baseDir, SanitizeName(appId));
        Directory.CreateDirectory(dir);

        var lockPath = Path.Combine(dir, "singleinstance.lock");

        try
        {
            // FileShare.None gives us an exclusive lock across processes on Windows/Linux/macOS.
            return new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }
        catch
        {
            return null;
        }
    }

    private static string SanitizeName(string value)
    {
        Span<char> buffer = stackalloc char[value.Length];
        var written = 0;

        foreach (var ch in value)
        {
            buffer[written++] = char.IsLetterOrDigit(ch) ? ch : '_';
        }

        return new string(buffer[..written]);
    }

    internal enum ActivationRequest
    {
        Activate,
        Maximize,
    }
}