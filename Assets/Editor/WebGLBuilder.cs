using System;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class WebGLBuilder
{
    // Published to GitHub Pages by .github/workflows/pages.yml.
    private const string OutputPath = "PublicBuild";

    // Batch mode: Unity -batchmode -quit -executeMethod WebGLBuilder.Build
    [MenuItem("Tools/Build WebGL To PublicBuild")]
    public static void Build()
    {
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = false;
        PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;

        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Zoo.unity" },
            locationPathName = OutputPath,
            target = BuildTarget.WebGL,
            targetGroup = BuildTargetGroup.WebGL,
            options = BuildOptions.None
        };

        BuildSummary summary = BuildPipeline.BuildPlayer(options).summary;
        Console.WriteLine($"WebGL build {summary.result}, size={summary.totalSize}, errors={summary.totalErrors}");

        if (summary.result != BuildResult.Succeeded)
            EditorApplication.Exit(1);
    }
}
