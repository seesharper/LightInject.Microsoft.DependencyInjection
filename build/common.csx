#r "nuget:System.Diagnostics.Process, 4.3.0"
#load "logging.csx"
using System.Diagnostics;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Threading;


public static class DotNet
{
    public static void Test(string pathToTestProject)
    {
        Command.Execute("dotnet.exe","test " + pathToTestProject + " --configuration Release" , ".");   
    }
  
    public static void Pack(string pathToProjectFile)
    {
        Command.Execute("dotnet.exe","pack " + pathToProjectFile + " --configuration Release" , ".");   
    }

    public static void Build(string pathToProjectFile)
    {
        Command.Execute("dotnet.exe","--version", ".");
        Command.Execute("dotnet.exe","restore " + pathToProjectFile, ".");        
        Command.Execute("dotnet.exe","build " + pathToProjectFile + " --configuration Release" , ".");   
    }
}


public class Command
{
    private static ILog log = Log.Create<Command>();
    
    private static StringBuilder lastProcessOutput = new StringBuilder();
    
    private static StringBuilder lastStandardErrorOutput = new StringBuilder();    
          
    public static string Execute(string commandPath, string arguments, string capture = null)
    {
        lastProcessOutput.Clear();
        lastStandardErrorOutput.Clear();
        var startInformation = CreateProcessStartInfo(commandPath, arguments);
        var process = CreateProcess(startInformation);
        SetVerbosityLevel(process, capture);        
        process.Start();        
        RunAndWait(process);                
               
        if (process.ExitCode != 0)
        {                      
            log.Error(lastStandardErrorOutput.ToString());            
            throw new InvalidOperationException("Command failed");
        }   
        
        return lastProcessOutput.ToString();
    }

    private static ProcessStartInfo CreateProcessStartInfo(string commandPath, string arguments)
    {
        var startInformation = new ProcessStartInfo(StringUtils.Quote(commandPath));
        startInformation.CreateNoWindow = true;
        startInformation.Arguments =  arguments;
        startInformation.RedirectStandardOutput = true;
        startInformation.RedirectStandardError = true;
        startInformation.UseShellExecute = false;
        
        return startInformation;
    }

    private static void RunAndWait(Process process)
    {        
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();         
        process.WaitForExit();                
    }

    private static void SetVerbosityLevel(Process process, string capture = null)
    {
        if(capture != null)
        {
            process.OutputDataReceived += (s, e) => 
            {
                if (e.Data == null)
                {
                    return;
                }
                              
                if (Regex.Matches(e.Data, capture,RegexOptions.Multiline).Count > 0)
                {
                    lastProcessOutput.AppendLine(e.Data);
                    log.Info(e.Data);                    
                }                                             
            };                        
        }
        process.ErrorDataReceived += (s, e) => 
        {
            lastStandardErrorOutput.AppendLine();
            lastStandardErrorOutput.AppendLine(e.Data);                        
        };
    }

    private static Process CreateProcess(ProcessStartInfo startInformation)
    {
        var process = new Process();
        process.StartInfo = startInformation;               
        return process;
    }
}

public static class StringUtils
{
    public static string Quote(string value)
    {
        return "\"" + value + "\"";
    }
}


    
   

   



   
