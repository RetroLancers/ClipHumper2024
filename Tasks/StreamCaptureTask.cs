using System.ComponentModel;
using FFMpegCore;
using OpenCvSharp;
using Serilog;
using Tesseract;

namespace ClipHunta2;

public class StreamCaptureTask
{
    private readonly CancellationTokenSource _cts;
    private readonly StreamDefinition _stream;
    private readonly Action<byte[]> _callback;

    private readonly BackgroundWorker _backgroundWorker;

    public StreamCaptureTask(CancellationTokenSource cts, StreamDefinition stream)
    {
        _cts = cts;
        _stream = stream;


        _backgroundWorker = new BackgroundWorker { WorkerSupportsCancellation = true };
        _backgroundWorker.DoWork += _watch;
    }

    private static string CreateClipPath(string filePath, int start, int end)
    {
        string appendText = $" {start} - {end}";

        string directoryName = Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException("Could not get directory name");
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        string extension = Path.GetExtension(filePath);

        string newFileName = $"{fileName}{appendText}{extension}";
        return Path.Combine(directoryName, newFileName);
    }

    private void _watch(object? sender, DoWorkEventArgs e)
    {
        if (e.Argument == null) return;

        var (streamUrl, captureType, streamCaptureStatus) =
            (ValueTuple<string, StreamCaptureType, StreamCaptureStatus>)e.Argument;

        var mediaInfo = FFProbe.Analyse(streamUrl);
        FrameEventHandler.OnMultiKill += args =>
        {
            var group = args.Group;
            if (group.Processed) return;
            group.Processed = true;

            Console.WriteLine(group);
            switch (captureType)
            {
                case StreamCaptureType.Clip:
                    var start = group.Min(a => a.Second);
                    if (start < 6)
                    {
                        start = 0;
                    }
                    else
                    {
                        start -= 6;
                    }

                    var end = group.Max(a => a.Second);
                    if (mediaInfo.Duration.TotalSeconds < end + 6)
                    {
                        end = (int)mediaInfo.Duration.TotalSeconds;
                    }
                    else
                    {
                        end += 6;
                    }

                    var clipPath = CreateClipPath(streamUrl, start, end);
                    ClipTaskManager.GetInstance().GetLongTasker().Put(new LongTaskQueueItem<(string inFile, string outFile, int start, int end)>((streamUrl, clipPath, start, end)));

                    
                    break;
                case StreamCaptureType.Stream:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        };
        using var videocapture = new VideoCapture();
        try
        {
            videocapture.Open(streamUrl, VideoCaptureAPIs.FFMPEG);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "error opening stream - it may be over");
            _cts.Cancel();
            return;
        }

        if (!videocapture.IsOpened())
        {
            streamCaptureStatus.SetFinalFrameCount(0);
            _cts.Cancel();
            return;
        }

        int frameNumber = 0;
        double fps = videocapture.Fps;
        if (fps < 30)
        {
            fps = 30;
        }

        if (fps > 300)
        {
            fps = 60;
        }

        try
        {
            int sleep = 0;
            while (!_cts.IsCancellationRequested)
            {
                if (!videocapture.IsOpened())
                {
                    break;
                }

                using var frameMat = videocapture.RetrieveMat();


                if (frameMat.Empty()) break;

                if (frameNumber < 60 * 60 * 3)
                {
                    //        frameNumber++;
                    //        continue;
                }

                if (frameNumber % 30 == 0)
                {
                    EmitFrame(frameMat, captureType, streamCaptureStatus, frameNumber, fps);

                    streamCaptureStatus.IncrementFrameCount();
                }
                else
                {
                    streamCaptureStatus.IncrementSkippedCount();
                    streamCaptureStatus.IncrementFrameCount();
                    streamCaptureStatus.IncrementFinishedCount();
                }

                if (!videocapture.IsOpened())
                {
                    streamCaptureStatus.SetFinalFrameCount(frameNumber);
                    _cts.Cancel();
                    return;
                }

                frameNumber++;
            }
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "error parsing stream");
            _cts.Cancel();
        }
        finally
        {
            streamCaptureStatus.SetFinalFrameCount(frameNumber);
        }
    }

    private void EmitFrame(Mat frameMat, StreamCaptureType captureType, StreamCaptureStatus streamCaptureStatus,
        int frameNumber, double fps)
    {
        var tmp = frameMat.Clone();

        Task.Run(() =>
        {
            ImagePrepperTaskManager.GetInstance().GetLongTasker()
                ?.PutInQueue((_stream, tmp.ToBytes(), captureType, streamCaptureStatus, frameNumber,
                    (int)(frameNumber / fps),
                    (int)fps));
            tmp.Dispose();
        });
    }

    public void Start(string streamUrl, StreamCaptureType captureType, StreamCaptureStatus streamCaptureStatus)
    {
        if (_backgroundWorker.IsBusy)
        {
            throw new Exception("Already running");
        }


        _backgroundWorker.RunWorkerAsync((streamUrl, captureType, streamCaptureStatus));
    }
}