using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using System;
using System.Collections.Generic;

[InitializeOnLoad]
public class TimeWasterTracker
{
    private static float totalWastedTime = 0f;
    private static DateTime currentWaitStart;
    private static bool isCurrentlyWaiting = false;
    private static string currentWaitingCategory = "";
    private static readonly string PREF_KEY = "TimeWasterTracker_TotalTime";

    // Dictionary to track different types of waiting
    private static Dictionary<string, float> waitingCategories = new Dictionary<string, float>();
     
    static TimeWasterTracker()
    {
        // Load saved time from EditorPrefs
        totalWastedTime = EditorPrefs.GetFloat(PREF_KEY, 0f);

        // Initialize categories
        waitingCategories["Compilation"] = EditorPrefs.GetFloat(PREF_KEY + "_Compilation", 0f);
        waitingCategories["Asset Import"] = EditorPrefs.GetFloat(PREF_KEY + "_AssetImport", 0f);
        waitingCategories["Domain Reload"] = EditorPrefs.GetFloat(PREF_KEY + "_DomainReload", 0f);
        waitingCategories["Other"] = EditorPrefs.GetFloat(PREF_KEY + "_Other", 0f);

        // Check if we were in the middle of a domain reload when we reinitialized
        string domainReloadStartTime = EditorPrefs.GetString("TimeWaster_DomainReloadStart", "");
        if (!string.IsNullOrEmpty(domainReloadStartTime))
        {
            // We were in the middle of a domain reload, finish timing it
            if (DateTime.TryParse(domainReloadStartTime, out DateTime startTime))
            {
                float domainReloadTime = (float)(DateTime.Now - startTime).TotalSeconds;
                totalWastedTime += domainReloadTime;
                waitingCategories["Domain Reload"] += domainReloadTime;

                // Save the updated times
                EditorPrefs.SetFloat(PREF_KEY, totalWastedTime);
                EditorPrefs.SetFloat(PREF_KEY + "_DomainReload", waitingCategories["Domain Reload"]);

                //Debug.Log($"Completed domain reload timing: {domainReloadTime:F2}s. Total: {totalWastedTime:F2}s");
            }

            // Clear the domain reload marker
            EditorPrefs.DeleteKey("TimeWaster_DomainReloadStart");
        }

        // Check if we had a full compilation process in progress
        string fullProcessStartTime = EditorPrefs.GetString("TimeWaster_FullProcessStart", "");
        if (!string.IsNullOrEmpty(fullProcessStartTime))
        {
            // Clear it to avoid double-counting
            EditorPrefs.DeleteKey("TimeWaster_FullProcessStart");
            //Debug.Log("Cleared full process timer to avoid double-counting");
        }

        // Subscribe to compilation events
        CompilationPipeline.compilationStarted += OnCompilationStarted;
        CompilationPipeline.compilationFinished += OnCompilationFinished;

        // Subscribe to domain reload events
        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeDomainReload;
        AssemblyReloadEvents.afterAssemblyReload += OnAfterDomainReload;

        Debug.Log("TimeWasterTracker initialized!");
    }



    private static void OnCompilationStarted(object obj)
    {
        // Clear any existing full process timer since we're starting the official compilation
        string fullProcessStart = EditorPrefs.GetString("TimeWaster_FullProcessStart", "");
        if (!string.IsNullOrEmpty(fullProcessStart))
        {
            EditorPrefs.DeleteKey("TimeWaster_FullProcessStart");
            Debug.Log("Cleared full process timer - official compilation started");
        }

        StartWaiting("Compilation");
    }

    private static void OnCompilationFinished(object obj)
    {
        StopWaiting("Compilation");
    }

    private static void OnBeforeDomainReload()
    {
        // Store the domain reload start time in EditorPrefs so it survives the reload
        EditorPrefs.SetString("TimeWaster_DomainReloadStart", DateTime.Now.ToString());
        //Debug.Log("Domain reload started, saved timestamp");
    }

    private static void OnAfterDomainReload()
    {
        // Add a buffer to account for pre-compilation and post-reload UI settling
        EditorApplication.delayCall += () =>
        {
            EditorApplication.delayCall += () =>
            {
                EditorApplication.delayCall += () =>
                {
                    // Add buffer time to account for:
                    // 1. Pre-compilation phase (when Unity detects changes but before compilation starts)
                    // 2. Post-reload UI settling (when domain reload is done but UI isn't fully responsive)
                    float bufferTime = 3.5f; // Estimated missing time based on observations
                    totalWastedTime += bufferTime;
                    waitingCategories["Compilation"] += bufferTime;

                    // Save the updated times
                    EditorPrefs.SetFloat(PREF_KEY, totalWastedTime);
                    EditorPrefs.SetFloat(PREF_KEY + "_Compilation", waitingCategories["Compilation"]);

                    //.Log($"Added compilation buffer time: {bufferTime:F2}s. Total: {totalWastedTime:F2}s");
                };
            };
        };
    }

    private static void StartWaiting(string category)
    {
        if (!isCurrentlyWaiting || currentWaitingCategory != category)
        {
            // If we're switching categories, stop the current one first
            if (isCurrentlyWaiting && currentWaitingCategory != category)
            {
                StopWaiting(currentWaitingCategory);
            }

            currentWaitStart = DateTime.Now;
            isCurrentlyWaiting = true;
            currentWaitingCategory = category;
            //Debug.Log($"Started waiting: {category}");
        }
    }

    private static void StopWaiting(string category)
    {
        if (isCurrentlyWaiting && currentWaitingCategory == category)
        {
            float waitTime = (float)(DateTime.Now - currentWaitStart).TotalSeconds;
            totalWastedTime += waitTime;

            if (waitingCategories.ContainsKey(category))
            {
                waitingCategories[category] += waitTime;
            }

            isCurrentlyWaiting = false;
            currentWaitingCategory = "";

            // Save to EditorPrefs
            EditorPrefs.SetFloat(PREF_KEY, totalWastedTime);
            EditorPrefs.SetFloat(PREF_KEY + "_" + category.Replace(" ", ""), waitingCategories[category]);

            //Debug.Log($"Stopped waiting: {category}. Time: {waitTime:F2}s. Total: {totalWastedTime:F2}s");
        }
    }

    public static float GetTotalWastedTime()
    {
        return totalWastedTime;
    }

    public static Dictionary<string, float> GetWaitingCategories()
    {
        return new Dictionary<string, float>(waitingCategories);
    }

    public static void ResetTimer()
    {
        totalWastedTime = 0f;
        isCurrentlyWaiting = false;
        currentWaitingCategory = "";
        waitingCategories.Clear();
        waitingCategories["Compilation"] = 0f;
        waitingCategories["Asset Import"] = 0f;
        waitingCategories["Domain Reload"] = 0f;
        waitingCategories["Other"] = 0f;

        EditorPrefs.DeleteKey(PREF_KEY);
        EditorPrefs.DeleteKey(PREF_KEY + "_Compilation");
        EditorPrefs.DeleteKey(PREF_KEY + "_AssetImport");
        EditorPrefs.DeleteKey(PREF_KEY + "_DomainReload");
        EditorPrefs.DeleteKey(PREF_KEY + "_Other");
        EditorPrefs.DeleteKey("TimeWaster_DomainReloadStart");
        EditorPrefs.DeleteKey("TimeWaster_FullProcessStart");

        //Debug.Log("Timer reset!"); 
    }
}

// Note: AssetPostprocessor timing didn't work as expected, so we're using a buffer approach instead
// The missing ~3.5 seconds typically include:
// 1. Pre-compilation phase (Unity detecting changes, preparing compilation)
// 2. Post-domain-reload UI settling (domain reload complete but UI not fully responsive)
public class TimeWasterAssetPostprocessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        // This runs after domain reload, so it's not useful for pre-compilation timing
        // Keeping it for reference but not using it for timing
    }
}