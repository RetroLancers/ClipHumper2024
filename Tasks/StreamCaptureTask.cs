using System.ComponentModel;
using OpenCvSharp;
using Serilog;
using Tesseract;

namespace ClipHunta2;

public class StreamCaptureTask
{
    private readonly CancellationTokenSource _cts;
    private readonly string _stream;
    private readonly Action<byte[]> _callback;

    private readonly BackgroundWorker _backgroundWorker;

    public StreamCaptureTask(CancellationTokenSource cts, string stream)
    {
        _cts = cts;
        _stream = stream;


        _backgroundWorker = new BackgroundWorker { WorkerSupportsCancellation = true };
        _backgroundWorker.DoWork += _watch;
    }

    private void _watch(object? sender, DoWorkEventArgs e)
    {
        if (e.Argument == null) return;

        var streamUrl = (string)e.Argument;


        using var videocapture = new VideoCapture();
        videocapture.Open(streamUrl, VideoCaptureAPIs.FFMPEG);
        if (!videocapture.IsOpened())
        {
            return;
        }

        while (!_cts.IsCancellationRequested)
        {
            using var frameMat = videocapture.RetrieveMat();
            if (!frameMat.Empty())
            {
                ImagePrepperTaskManager.GetInstance().GetLongTasker()
                    ?.PutInQueue(new Tuple<string, byte[]>(_stream, frameMat.ToBytes()));

                continue;
            }

            Log.Logger.Information("Empty Frame");
        }
    }

    public void Start(string streamUrl, StreamCaptureType captureType)
    {
        if (_backgroundWorker.IsBusy)
        {
            throw new Exception("Already running");
        }


        _backgroundWorker.RunWorkerAsync(new Tuple<string, StreamCaptureType>(streamUrl, captureType));
    }
}