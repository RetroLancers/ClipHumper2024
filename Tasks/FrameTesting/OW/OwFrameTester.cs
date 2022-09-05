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

    public string[] TestFrame(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<string>();
        }

        List<string> list = new();

        foreach (var tester in _testers)
        {

            if (tester.Test(text))
            {
                list.Add(tester.GetName());
         
            }
        }

        string[] strings = list.ToArray();
        list.Clear();
        return strings;
    }
}