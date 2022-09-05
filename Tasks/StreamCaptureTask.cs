using System.ComponentModel;
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

    private void _watch(object? sender, DoWorkEventArgs e)
    {
        if (e.Argument == null) return;

        var (streamUrl, captureType, streamCaptureStatus) =
            (ValueTuple<string, StreamCaptureType, StreamCaptureStatus>)e.Argument;


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
            fps = 120;
        }
        try
        {

            while (!_cts.IsCancellationRequested)
            {
                if (!videocapture.IsOpened())
                {
                    break;
                }

                using var frameMat = videocapture.RetrieveMat();


                if (frameMat.Empty()) break;


                if (frameNumber % 5 == 0)
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
        }catch(Exception ex)
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