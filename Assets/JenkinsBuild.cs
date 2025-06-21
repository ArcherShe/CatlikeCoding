using System;
using UnityEngine;
using System.IO;
using UnityEditor;

public class JenkinsBuild
{
    [MenuItem("Jenkins/Build for Jenkins")]
    public static void JenkinsTestBuild()
    {
        fileName = "Build_qa.txt";
        var filePath = Path.Combine(Application.dataPath, fileName);
        if (File.Exists(filePath)) File.Delete(filePath);
        
        string[] args = Environment.GetCommandLineArgs();
        string buildTarget = GetArgument(args, "-platform", "StandaloneOSX");

        var file = File.OpenWrite(filePath);
        using var writer = new StreamWriter(file);
        writer.WriteLine("Build started at: " + System.DateTime.Now);
        writer.WriteLine($"Build target: {buildTarget}");
        writer.WriteLine("Build path: " + Application.persistentDataPath);
        writer.WriteLine("Unity version: " + Application.unityVersion);
        writer.WriteLine("Platform: " + Application.platform);
        writer.Close();
        file.Close();
        
        AssetDatabase.Refresh();
    }
    
    private static string GetArgument(string[] args, string name, string defaultValue)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }
        return defaultValue;
    }
}
