using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;

namespace Gtr2MemOpsTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        //public static Log.LogLevel LoggingLevel { get; set; } = Gtr2MemOpsTool.Log.LogLevel.Debug;
        //public static Log LogObj { get; } = new Gtr2MemOpsTool.Log(Gtr2MemOpsTool.Log.LogLevel.Debug);
        ////public static AsyncBatchLogger Log { get; private set; } = null!;
        public static AsyncBatchLogger.LogLevel DefaultLogLevel = AsyncBatchLogger.LogLevel.Debug;
        public static AsyncBatchLogger Log = new AsyncBatchLogger(DefaultLogLevel);
        //private readonly Channel<LogItem> _channel = Channel.CreateUnbounded<LogItem>();
        //private CancellationTokenSource? _cts;


        //public static void Log(string message, Log.LogLevel logLevel)
        //{
        //    Logger?.Log(new LogItem(message, logLevel));
        //}

        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    base.OnStartup(e);
        //    Debug.WriteLine("Application starting..."); // Argh. Debug.WriteLine is a synchronous blocking call and ridiculously slow. It freezes the UI. Don't use it.
        //    Log.LogLevel loggingLevel = Gtr2MemOpsTool.Log.LogLevel.Debug;
        //    Log = new AsyncBatchLogger(_channel, loggingLevel);
        //    _cts = new CancellationTokenSource();
        //    _ = StartLogConsumerAsync(_cts.Token);

        //    //Logger?.Log(new LogItem("AsyncBatchLogger: Application started", Log.LogLevel.Info));
        //}

        //protected override void OnExit(ExitEventArgs e)
        //{
        //    _cts!.Cancel();
        //    base.OnExit(e);
        //}

        //private async Task StartLogConsumerAsync(CancellationToken ct)
        //{
        //    Int32 taskDelay = 100;
        //    var buffer = new List<LogItem>();

        //    while (!ct.IsCancellationRequested)
        //    {
        //        // Wait for at least one item
        //        await _channel.Reader.WaitToReadAsync(ct);

        //        // Drain everything available right now (the batch)
        //        while (_channel.Reader.TryRead(out var logItem))
        //            buffer.Add(logItem);

        //        // Marshal batch to UI thread
        //        await Application.Current.Dispatcher.InvokeAsync(() =>
        //        {
        //            foreach (var logItem in buffer)
        //                Log.Add(logItem.Message, logItem.LogLevel);
        //        });

        //        //buffer.Clear();
        //        buffer = new List<LogItem>();
        //        await Task.Delay(taskDelay, ct);
        //    }
        //}
    }
}
