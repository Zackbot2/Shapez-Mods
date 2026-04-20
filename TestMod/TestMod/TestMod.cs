using ILogger = Core.Logging.ILogger;

public class TestMod : IMod
{
    public TestMod(ILogger logger)
    {
        logger.Info?.Log("Hello, shapez 2");
    }

    public void Dispose()
    {
    }
}
