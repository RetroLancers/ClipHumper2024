namespace ClipHunta2;

public sealed class ImagePrepperTaskManager : LongTaskManger<ImagePrepperTask>
{ 
    private static ImagePrepperTaskManager _instance;

    public override ImagePrepperTask createOne()
    {
        return new ImagePrepperTask(_cancellationToken);
    }

    public ImagePrepperTaskManager()
    {
        _longTasks = Array.Empty<ImagePrepperTask>();
    }


    public static ImagePrepperTaskManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = new ImagePrepperTaskManager();
            _cancellationToken = new CancellationTokenSource();
        }

        return _instance;
    }

  
}