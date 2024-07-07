using ClipHunta2.TaskManagers.LongTask;
using ClipHunta2.Tasks;

namespace ClipHunta2.TaskManagers;

public sealed class ImageScannerTaskManager : LongTaskManger<ImageScannerTask>
{ 
    private static ImageScannerTaskManager? _instance; 

    public ImageScannerTaskManager()
    {
        LongTasks = [];
    }

    protected override ImageScannerTask createOne()
    {
        return new ImageScannerTask(CancellationToken);
    }

    public static ImageScannerTaskManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = new ImageScannerTaskManager();
            CancellationToken = new CancellationTokenSource();
        }

        return _instance;
    }

  
}