﻿using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ClipHunta2.Tasks.FrameTesting.OW;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Serilog;
using Tesseract;

namespace ClipHunta2;

public class ImageScannerTask : LongTask<(StreamDefinition streamDefinition,
    byte[] bytes, StreamCaptureType captureType
    , StreamCaptureStatus streamCaptureStatus, int frameNumber, int second,
    int fps)>
{
    protected override LongTask<(StreamDefinition streamDefinition, byte[] bytes, StreamCaptureType captureType,
        StreamCaptureStatus streamCaptureStatus, int frameNumber, int second, int fps)>? GetTop()
    {
        return ImageScannerTaskManager.GetInstance().GetTopTasker();
    }


    protected override async Task _action(
        (StreamDefinition streamDefinition, byte[] bytes, StreamCaptureType
            captureType, StreamCaptureStatus streamCaptureStatus, int frameNumber, int second,
            int fps) value)
    {
        using var pix = Pix.LoadFromMemory(value.bytes);

        var text = await TesseractLongTaskManager.GetInstance().GetLongTasker()?.GetText(pix);


        if (string.IsNullOrWhiteSpace(text))
        {
            value.streamCaptureStatus.IncrementFinishedCount();
            return;
        }


        EventRouterTaskManager.GetInstance().GetLongTasker()?.Put(
            new LongTaskQueueItem<(StreamDefinition, string? text, int frameNumber, int second,
                int fps, StreamCaptureStatus)>
            ((value.streamDefinition, text, value.frameNumber, value.second,
                value.fps, value.streamCaptureStatus)));

        value.streamCaptureStatus.IncrementImagesScanned();
        text = null;
    }

    public void PutInQueue(
        (StreamDefinition streamDefinition, byte[] bytes, StreamCaptureType captureType, StreamCaptureStatus
            streamCaptureStatus, int frameNumber, int second,
            int fps) value)
    {
        Put(new LongTaskQueueItem<(StreamDefinition, byte[], StreamCaptureType, StreamCaptureStatus, int, int, int)>(
            value)).Wait();
    }

    public ImageScannerTask(CancellationTokenSource cts) : base(cts)
    {
    }
}