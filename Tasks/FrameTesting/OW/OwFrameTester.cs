namespace ClipHunta2.Tasks.FrameTesting.OW;

public sealed class OwFrameTester
{
    private static OwFrameTester? _instance = null;

    public static OwFrameTester GetInstance()
    {
        if (_instance == null)
        {
            _instance = new OwFrameTester();
        }

        return _instance;
    }

    private readonly FrameTester[] _testers = { OwOrbHarmonyTester.GetInstance(), OwHealingTester.GetInstance(), OwDeathFrameTester.GetInstance(), OwElimFrameTester.GetInstance(), OwHeroSelectTester.GetInstance(), OwBlockingTester.GetInstance(), OwAssistTester.GetInstance() };

    public IEnumerable<string> TestFrame(string text)
    {
        return string.IsNullOrWhiteSpace(text) ? Array.Empty<string>() : (from tester in _testers where tester.Test(text) select tester.GetName()).ToArray();
    }
}