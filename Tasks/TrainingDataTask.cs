using OpenCvSharp;
using Serilog;
using Tesseract;

namespace ClipHunta2;

public class TrainingDataTask : LongTask<(  string[]  ,   Pix  )>
{ 
    public bool Complete { get => _complete; }

    private const string TRAININGDATAFOLDER = "./trainingdata/";
    private bool _complete = false;
    private bool _elimcomplete = false;
    private bool _elimedcomplete = false;
    public TrainingDataTask(CancellationTokenSource cts) : base(cts)
    {
    }

    protected bool IsSameEvent(FrameEvent a, FrameEvent b)
    {
        return IsSameSecond(a, b);
    }

    protected bool IsSameSecond(FrameEvent a, FrameEvent b)
    {
        return b.EventName == a.EventName && b.Second == a.Second;
    }
    protected string[] blockedByElim = new[] { "elim" };
    protected override async Task<string?> _action(
       (string[] frameEvents,  Pix copy) value)
    {
        if (value.frameEvents.Length == 0)
            return null;

        if (!Directory.Exists(TRAININGDATAFOLDER))
        {
            Directory.CreateDirectory(TRAININGDATAFOLDER);
        }
        foreach (string e in value.frameEvents)
        {
            int trainIndex = 0;
            while (trainIndex < 10)
            {
                if (!File.Exists($"{TRAININGDATAFOLDER}{e}{trainIndex}.png"))
                {
                    string fileName = $"{TRAININGDATAFOLDER}{e}{trainIndex}.png";
                    string gtFileName = $"{TRAININGDATAFOLDER}{e}{trainIndex}.gt.txt";
                    value.copy.Save(fileName,ImageFormat.Png);
                    switch (e) {

                        case "elim":
                            File.WriteAllText(gtFileName, "ELIMINATED");
                            break;
                        case "elimed":
                            File.WriteAllText(gtFileName, "YOU WERE ELIMINATED BY");
                            break;
                        case "heroselect":
                            File.WriteAllText(gtFileName, "TO CHANGE HERO");
                            break;
                        case "orbharmony":
                            File.WriteAllText(gtFileName, "OF HARMONY GAINED FROM");
                            break;
                        default:
                            File.WriteAllText(gtFileName, e.ToUpper());
                            break;
                    }
                    break;

                }
                trainIndex++;
            }
            if(trainIndex == 10)
            {
                switch (e)
                {
                    case "elim":
                        _elimcomplete = true;
                        break;
                    case "elimed":
                        _elimedcomplete = true;
                        break;
                }
            }
            if(_elimedcomplete && _elimcomplete)
            {
                _complete = true;
            }
            // e.EventName
        }


        value.copy.Dispose();


        return null;
    }

   
}

 