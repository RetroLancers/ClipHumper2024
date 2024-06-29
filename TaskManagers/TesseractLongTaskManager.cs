﻿namespace ClipHunta2;

public sealed class TesseractLongTaskManager : LongTaskManger<TesseractTask>
{
    public TesseractLongTaskManager()
    {
        _longTasks = [];
    }

    private static TesseractLongTaskManager? _instance;

    public override TesseractTask createOne()
    {
        return new TesseractTask(_cancellationToken);
    }

 

    public static TesseractLongTaskManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = new TesseractLongTaskManager();
            _cancellationToken = new CancellationTokenSource();
      
        }

        return _instance;
    }

    public void Free()
    {
        foreach (var tesseractTask in _longTasks)
        {
            tesseractTask.Dispose();
        }
    }
}