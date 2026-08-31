using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace PoC.Sync;

public class Program
{
    private static readonly ConcurrentQueue<string> _outboxQueue = new();
    private static bool _isConnected = false;

    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== SysVet Sync PoC ===");
        Console.WriteLine("Mocking background outbox synchronization...");
        Console.WriteLine("Commands: [T]oggle Connection | [A]dd Item to Outbox | [Q]uit\n");

        using var cts = new CancellationTokenSource();
        var syncTask = Task.Run(() => ProcessOutboxBackgroundAsync(cts.Token));

        while (true)
        {
            var key = Console.ReadKey(intercept: true).Key;

            if (key == ConsoleKey.Q)
            {
                break;
            }
            else if (key == ConsoleKey.T)
            {
                _isConnected = !_isConnected;
                Console.WriteLine($"\n[Network] Connection status changed: {(_isConnected ? "ONLINE" : "OFFLINE")}");
            }
            else if (key == ConsoleKey.A)
            {
                var itemId = Guid.NewGuid().ToString().Substring(0, 8);
                _outboxQueue.Enqueue($"Item-{itemId}");
                Console.WriteLine($"\n[App] Item-{itemId} saved locally and added to Outbox.");
            }
        }

        Console.WriteLine("\nShutting down...");
        cts.Cancel();
        try
        {
            await syncTask;
        }
        catch (OperationCanceledException)
        {
            // Ignored
        }
    }

    private static async Task ProcessOutboxBackgroundAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (_isConnected && _outboxQueue.TryPeek(out var item))
            {
                Console.WriteLine($"\n[BackgroundWorker] Attempting to sync {item} to Cloud API...");
                
                // Simulate network latency
                await Task.Delay(1000, token);

                if (_isConnected) // check again in case it dropped
                {
                    _outboxQueue.TryDequeue(out _);
                    Console.WriteLine($"[BackgroundWorker] Successfully synced {item} to Cloud.");
                }
                else
                {
                    Console.WriteLine($"[BackgroundWorker] Sync failed for {item}. Connection lost. Will retry later.");
                }
            }
            else
            {
                // Idle when offline or queue is empty
                await Task.Delay(2000, token);
            }
        }
    }
}
