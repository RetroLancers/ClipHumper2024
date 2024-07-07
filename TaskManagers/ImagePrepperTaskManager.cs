using ClipHunta2.TaskManagers.LongTask;
using ClipHunta2.Tasks;

namespace ClipHunta2.TaskManagers;

public sealed class ImagePrepperTaskManager : LongTaskManger<ImagePrepperTask>
{ 
    private static ImagePrepperTaskManager? _instance;

    protected override ImagePrepperTask createOne()
    {
        return new ImagePrepperTask(CancellationToken);
    }

    public ImagePrepperTaskManager()
    {
        LongTasks = [];
    }


    public static ImagePrepperTaskManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = new ImagePrepperTaskManager();
            CancellationToken = new CancellationTokenSource();
        }

        return _instance;
    }

  
}