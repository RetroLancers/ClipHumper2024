using Tesseract;

namespace ClipHunta2;


public class TesseractTask : LongTaskWithReturn<Pix, string>, IDisposable
{
    public TesseractTask(CancellationTokenSource _ctr, string tesseractDataPath=@"c:\tmp\tessdata_fast", string tesseractLanguage = "eng",
        EngineMode mode = EngineMode.LstmOnly) : base(_ctr)
    {
        _engine = new TesseractEngine(tesseractDataPath, tesseractLanguage, mode);
    }

    private TesseractEngine _engine;

    protected override async Task<string?> _action(Pix value)
    {
        using var page = _engine.Process(value);
        return page.GetText();
    }

    public void Dispose()
    {
        _engine.Dispose();
    }

    
}