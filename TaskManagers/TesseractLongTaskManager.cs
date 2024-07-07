using ClipHunta2.TaskManagers.LongTask;
using ClipHunta2.Tasks;

namespace ClipHunta2.TaskManagers;

public sealed class TesseractLongTaskManager : LongTaskManger<TesseractTask>
{
    public TesseractLongTaskManager()
    {
        LongTasks = [];
    }

    private static TesseractLongTaskManager? _instance;

    protected override TesseractTask createOne()
    {
        var tessPath = System.Environment.GetEnvironmentVariable("TESSERACT_DATA") ??  @"c:\tmp\tessdata_best-4.1.0";
        return new TesseractTask(CancellationToken,tessPath);
    }

 

    public static TesseractLongTaskManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = new TesseractLongTaskManager();
            CancellationToken = new CancellationTokenSource();
      
        }

        return _instance;
    }

    public void Free()
    {
        foreach (var tesseractTask in LongTasks)
        {
            tesseractTask.Dispose();
        }
    }
}