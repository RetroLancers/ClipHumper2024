using System.ComponentModel;
using ClipHunta2.StreamLink;
using Serilog;

namespace ClipHunta2;

public enum StreamCaptureType
{
    Clip,
    Stream
}

public class StreamCaptureTaskStarterTask
{
    private readonly CancellationTokenSource _cts;
    private readonly string _stream;
    private readonly StreamCaptureType _captureType;


    private readonly BackgroundWorker _backgroundWorker;

    public StreamCaptureTaskStarterTask(CancellationTokenSource cts, string stream, StreamCaptureType captureType)
    {
        _cts = cts;
        _stream = stream;
        _captureType = captureType;

        _backgroundWorker = new BackgroundWorker { WorkerSupportsCancellation = true };
        _backgroundWorker.DoWork += _watch;
    }

    private void _watch(object? sender, DoWorkEventArgs e)
    {
        StreamCaptureTask captureTask = new StreamCaptureTask(_cts, _stream);

        var eArgument = (string)e.Argument!;
        string streamUrl = null;
        if (_captureType == StreamCaptureType.Clip)
        {
            var clip = TwitchDlRunner.LookUpStream(eArgument);
            if (clip != null)
            {
                streamUrl = clip.Url.ToString();
            }
            else Log.Logger.Debug("Failed to get the clip url {Streamer} ", _stream);
        }
        else
        {
            var streams = StreamLinkRunner.LookUpStream("https://twitch.tv/" + _stream);
            if (streams != null)
            {
                var streamDict = streams.Streams;

                streamUrl = streamDict["720p60"].Url.ToString();
            }
            else Log.Logger.Debug("Failed to get the stream url {Streamer} ", _stream);
        }

        if (streamUrl != null) captureTask.Start(streamUrl, _captureType);
    }

    public void Start(string? arg)
    {
        if (_backgroundWorker.IsBusy)
        {
            throw new Exception("Already running");
        }

        if (_captureType == StreamCaptureType.Clip && string.IsNullOrEmpty(arg))
        {
            throw new ArgumentException("Argument can't be null, need clip id");
        }

        _backgroundWorker.RunWorkerAsync(arg);
    }

    public void Enqueue(byte[] img)
    {
    }
}