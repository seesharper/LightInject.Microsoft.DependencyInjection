#r "nuget:Microsoft.Extensions.Logging,1.1.1"
#r "nuget:Microsoft.Extensions.Logging.Console,1.1.1"

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

public interface ILog
{
    void Info(string message);
    void Error(string message);
}

public class Log : ILog
{    
    private static ILoggerFactory loggerFactory = new LoggerFactory();
    private ILogger logger;

    public Log(ILogger logger)
    {
        this.logger = logger;
    }

    static Log()
    {        
        loggerFactory.AddConsole();        
    }

    public static ILog Create(string category)
    {
        return new Log(loggerFactory.CreateLogger(category));
    }

    public static ILog Create<T>()
    {
        return new Log(loggerFactory.CreateLogger<T>());
    }


    public void Info(string message)
    {
        WriteLine(message);
        // logger.LogInformation(message);
    }

    public void Error(string message)
    {
        logger.LogInformation(message);
    }
}

public static ILogger CreateLogger(Type type)
{
    return null;
}
