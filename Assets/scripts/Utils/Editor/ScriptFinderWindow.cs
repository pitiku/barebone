using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScriptFinderWindow : EditorWindow
{
    private string scriptNamesInput = "";
    private List<string> scriptNames = new List<string>();
    private Vector2 scrollPrefabs;
    private Vector2 scrollScenes;
    private string resultTextPrefabs = "";
    private string resultTextScenes = "";
    private int matchCountPrefabs = 0;
    private int matchCountScenes = 0;
    private bool matchFullName = true;
    private bool caseSensitive = false;
    private int selectedTab = 0;
    private readonly string[] tabNames = { "Prefabs", "Scenes" };

    // Folder exclusion support
    private string excludeFolders = "ThirdParty,Plugins";
    private bool showExclusionSettings = false;

    // Scene search options
    private bool searchInScenes = false;
    private bool showSceneSettings = false;

    // Cache for Type lookups to avoid repeated reflection
    private static readonly Dictionary<string, Type> typeCache = new Dictionary<string, Type>();

    [MenuItem("UPTools/Script Finder", false, 73)]
    private static void Open()
    {
        var win = GetWindow<ScriptFinderWindow>();
        win.titleContent = new GUIContent("Script Finder");
        win.minSize = new Vector2(500, 550);
        win.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Find assets containing script(s) (in children):", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Script Names (comma-separated or one per line):", EditorStyles.label);
        EditorGUILayout.HelpBox("Examples: PlayerController,EnemyAI,UIManager\nOr paste multiple names, each on a new line.", MessageType.Info);

        // Multi-line text area for script names
        scriptNamesInput = EditorGUILayout.TextArea(scriptNamesInput, GUILayout.Height(80));

        EditorGUILayout.Space(4);

        EditorGUILayout.BeginHorizontal();
        matchFullName = EditorGUILayout.ToggleLeft("Match FullName (Namespace.Class)", matchFullName, GUILayout.Width(240));
        caseSensitive = EditorGUILayout.ToggleLeft("Case-sensitive", caseSensitive, GUILayout.Width(140));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(2);

        // Folder exclusion foldout
        showExclusionSettings = EditorGUILayout.Foldout(showExclusionSettings, "Exclusion Settings", true);
        if (showExclusionSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Exclude folders (comma-separated)", GUILayout.Width(220));
            excludeFolders = EditorGUILayout.TextField(excludeFolders);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("Example: ThirdParty,Plugins,Samples\nWill exclude any path containing these folder names.", MessageType.Info);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(2);

        // Scene search settings
        showSceneSettings = EditorGUILayout.Foldout(showSceneSettings, "Scene Search Settings", true);
        if (showSceneSettings)
        {
            EditorGUI.indentLevel++;
            searchInScenes = EditorGUILayout.ToggleLeft(
                "Search also in scenes",
                searchInScenes,
                GUILayout.Width(250)
            );
            EditorGUILayout.HelpBox(
                "Check it to search ALL scenes in the project (slower but comprehensive).",
                MessageType.Info
            );
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(4);

        GUI.enabled = !string.IsNullOrWhiteSpace(scriptNamesInput);
        if (GUILayout.Button("Search", GUILayout.Height(28)))
        {
            ParseScriptNames();
            SearchAll();
        }
        GUI.enabled = true;

        EditorGUILayout.Space();

        // Tabs
        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
        EditorGUILayout.Space(2);

        if (selectedTab == 0) // Prefabs tab
        {
            EditorGUILayout.LabelField($"Prefab Matches: {matchCountPrefabs}");
            EditorGUILayout.Space(2);

            using (var scrollScope = new EditorGUILayout.ScrollViewScope(scrollPrefabs, GUILayout.ExpandHeight(true)))
            {
                scrollPrefabs = scrollScope.scrollPosition;
                EditorGUILayout.TextArea(resultTextPrefabs, GUILayout.ExpandHeight(true));
            }

            EditorGUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Copy to Clipboard", GUILayout.Width(160)))
                {
                    EditorGUIUtility.systemCopyBuffer = resultTextPrefabs;
                }

                if (GUILayout.Button("Clear", GUILayout.Width(100)))
                {
                    resultTextPrefabs = "";
                    matchCountPrefabs = 0;
                }
            }
        }
        else // Scenes tab
        {
            EditorGUILayout.LabelField($"Scene Matches: {matchCountScenes}");
            EditorGUILayout.Space(2);

            using (var scrollScope = new EditorGUILayout.ScrollViewScope(scrollScenes, GUILayout.ExpandHeight(true)))
            {
                scrollScenes = scrollScope.scrollPosition;
                EditorGUILayout.TextArea(resultTextScenes, GUILayout.ExpandHeight(true));
            }

            EditorGUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Copy to Clipboard", GUILayout.Width(160)))
                {
                    EditorGUIUtility.systemCopyBuffer = resultTextScenes;
                }

                if (GUILayout.Button("Clear", GUILayout.Width(100)))
                {
                    resultTextScenes = "";
                    matchCountScenes = 0;
                }
            }
        }
    }

    private void ParseScriptNames()
    {
        scriptNames.Clear();

        // Split by both commas and newlines
        var names = scriptNamesInput
            .Split(new[] { ',', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries)
            .Select(name => name.Trim())
            .Where(name => !string.IsNullOrEmpty(name))
            .Distinct()
            .ToList();

        scriptNames = names;
        Debug.Log($"Script Finder: Searching for {scriptNames.Count} script(s): {string.Join(", ", scriptNames)}");
    }

    private void SearchAll()
    {
        try
        {
            if (scriptNames.Count == 0)
            {
                Debug.LogWarning("Script Finder: No script names provided.");
                return;
            }

            // Search prefabs
            SearchPrefabsForScript(scriptNames, matchFullName, caseSensitive);

            // Search scenes
            if(searchInScenes) SearchScenesForScript(scriptNames, matchFullName, caseSensitive);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            Repaint();
        }
    }

    private void SearchPrefabsForScript(List<string> targetNames, bool useFullName, bool isCaseSensitive)
    {
        try
        {
            AssetDatabase.StartAssetEditing();

            try
            {
                string filter = "t:prefab t:model";
                string[] searchFolders = new[] { "Assets" };
                string[] guids = AssetDatabase.FindAssets(filter, searchFolders);

                // Parse exclusion list
                HashSet<string> excludedFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (!string.IsNullOrWhiteSpace(excludeFolders))
                {
                    foreach (var folder in excludeFolders.Split(','))
                    {
                        var trimmed = folder.Trim();
                        if (!string.IsNullOrEmpty(trimmed))
                        {
                            excludedFolders.Add(trimmed);
                        }
                    }
                }

                var comparison = isCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

                // Pre-filter paths
                var validPaths = new List<(string guid, string path)>(guids.Length);
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!IsPathExcluded(path, excludedFolders))
                    {
                        validPaths.Add((guid, path));
                    }
                }

                int validCount = validPaths.Count;
                Debug.Log($"Script Finder: Searching {validCount} prefabs for {targetNames.Count} script(s) (excluded {guids.Length - validCount})");

                // Use thread-safe collection for parallel processing
                var matches = new ConcurrentBag<(string prefabName, List<string> foundScripts)>();
                int processedCount = 0;
                object progressLock = new object();

                // Parallel processing for significantly faster searching
                Parallel.ForEach(validPaths, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                    (item) =>
                    {
                        var (guid, path) = item;

                        GameObject contentsRoot = null;
                        try
                        {
                            contentsRoot = PrefabUtility.LoadPrefabContents(path);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"Script Finder: Could not load prefab at {path}: {ex.Message}");
                            return;
                        }

                        if (contentsRoot == null)
                            return;

                        try
                        {
                            var foundScripts = FindScriptsInGameObject(contentsRoot, targetNames, useFullName, comparison);
                            if (foundScripts.Count > 0)
                            {
                                matches.Add((Path.GetFileNameWithoutExtension(path), foundScripts));
                            }
                        }
                        finally
                        {
                            PrefabUtility.UnloadPrefabContents(contentsRoot);
                        }

                        // Update progress on main thread (thread-safe)
                        lock (progressLock)
                        {
                            processedCount++;
                            if (processedCount % 10 == 0)
                            {
                                float progress = (float)processedCount / validCount;
                                EditorUtility.DisplayProgressBar(
                                    "Script Finder - Prefabs",
                                    $"Scanning prefabs ({processedCount}/{validCount})",
                                    progress
                                );
                            }
                        }
                    }
                );

                matchCountPrefabs = matches.Count;

                if (matchCountPrefabs > 0)
                {
                    var sb = new StringBuilder();
                    foreach (var (prefabName, foundScripts) in matches.OrderBy(x => x.prefabName))
                    {
                        sb.AppendLine($"{prefabName}");
                        foreach (var script in foundScripts.Distinct())
                        {
                            sb.AppendLine($"  └─ {script}");
                        }
                    }
                    resultTextPrefabs = sb.ToString();
                }
                else
                {
                    resultTextPrefabs = "No matches found in prefabs.";
                }

                Debug.Log($"Script Finder: Prefab search complete. Found {matchCountPrefabs} prefabs containing the scripts.");
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Script Finder prefab search error: {e.Message}\n{e.StackTrace}");
            resultTextPrefabs = $"Error during prefab search: {e.Message}";
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private void SearchScenesForScript(List<string> targetNames, bool useFullName, bool isCaseSensitive)
    {
        var comparison = isCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        var matches = new ConcurrentBag<(string sceneName, List<string> foundScripts)>();

        try
        {
            // Search all scenes in the project (comprehensive but slower)
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
            var scenePaths = sceneGuids.Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToArray();

            Debug.Log($"Script Finder: Found {scenePaths.Length} scenes in project");

            // Parse exclusion list
            HashSet<string> excludedFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(excludeFolders))
            {
                foreach (var folder in excludeFolders.Split(','))
                {
                    var trimmed = folder.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        excludedFolders.Add(trimmed);
                    }
                }
            }

            // Filter excluded paths
            var validScenePaths = scenePaths.Where(path => !IsPathExcluded(path, excludedFolders)).ToArray();
            Debug.Log($"Script Finder: Searching {validScenePaths.Length} scenes for {targetNames.Count} script(s) (excluded {scenePaths.Length - validScenePaths.Length})");

            // Save current scene setup to restore later
            var originalScenes = new SceneSetup[SceneManager.sceneCount];
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                originalScenes[i] = new SceneSetup
                {
                    path = scene.path,
                    isLoaded = scene.isLoaded,
                    isActive = SceneManager.GetActiveScene() == scene
                };
            }

            try
            {
                // Check for unsaved changes
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    Debug.Log("Script Finder: Scene search cancelled by user");
                    resultTextScenes = "Scene search cancelled.";
                    return;
                }

                // Search each scene
                for (int i = 0; i < validScenePaths.Length; i++)
                {
                    string scenePath = validScenePaths[i];
                    float progress = (float)i / validScenePaths.Length;

                    if (EditorUtility.DisplayCancelableProgressBar(
                        "Script Finder - Scenes",
                        $"Scanning: {Path.GetFileNameWithoutExtension(scenePath)} ({i + 1}/{validScenePaths.Length})",
                        progress))
                    {
                        Debug.Log("Script Finder: Scene search cancelled by user");
                        break;
                    }

                    try
                    {
                        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                        if (scene.isLoaded)
                        {
                            // Search all root GameObjects in the scene
                            var rootObjects = scene.GetRootGameObjects();
                            var foundScripts = new List<string>();

                            foreach (var rootObj in rootObjects)
                            {
                                foundScripts.AddRange(FindScriptsInGameObject(rootObj, targetNames, useFullName, comparison));
                            }

                            if (foundScripts.Count > 0)
                            {
                                matches.Add((scene.name, foundScripts));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Script Finder: Could not load scene at {scenePath}: {ex.Message}");
                    }
                }
            }
            finally
            {
                // Restore original scene setup
                if (originalScenes.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(originalScenes);
                }
            }

            matchCountScenes = matches.Count;

            if (matchCountScenes > 0)
            {
                var sb = new StringBuilder();
                foreach (var (sceneName, foundScripts) in matches.OrderBy(x => x.sceneName))
                {
                    sb.AppendLine($"{sceneName}");
                    foreach (var script in foundScripts.Distinct())
                    {
                        sb.AppendLine($"  └─ {script}");
                    }
                }
                resultTextScenes = sb.ToString();
            }
            else
            {
                resultTextScenes = "No matches found in scenes.";
            }

            Debug.Log($"Script Finder: Scene search complete. Found {matchCountScenes} scenes containing the scripts.");
            return;
        }
        catch (Exception e)
        {
            Debug.LogError($"Script Finder scene search error: {e.Message}\n{e.StackTrace}");
            resultTextScenes = $"Error during scene search: {e.Message}";
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private List<string> FindScriptsInGameObject(GameObject root, List<string> targetNames, bool useFullName, StringComparison comparison)
    {
        var foundScripts = new List<string>();
        var components = root.GetComponentsInChildren<MonoBehaviour>(true);

        if (components == null || components.Length == 0)
            return foundScripts;

        foreach (var c in components)
        {
            if (c == null) continue;

            var type = c.GetType();
            string nameToCheck = useFullName ? GetCachedFullName(type) : type.Name;

            if (!string.IsNullOrEmpty(nameToCheck))
            {
                foreach (var targetName in targetNames)
                {
                    if (nameToCheck.Equals(targetName, comparison))
                    {
                        foundScripts.Add(nameToCheck);
                        break;
                    }
                }
            }
        }

        return foundScripts;
    }

    private bool IsPathExcluded(string path, HashSet<string> excludedFolders)
    {
        if (excludedFolders.Count == 0)
            return false;

        string normalizedPath = path.Replace('\\', '/');
        foreach (var excluded in excludedFolders)
        {
            if (normalizedPath.IndexOf("/" + excluded + "/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                normalizedPath.StartsWith(excluded + "/", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    // Cache FullName lookups as they can be expensive with reflection
    private string GetCachedFullName(Type type)
    {
        string key = type.AssemblyQualifiedName ?? type.Name;

        if (!typeCache.TryGetValue(key, out Type cachedType))
        {
            typeCache[key] = type;
            return type.FullName ?? type.Name;
        }

        return cachedType.FullName ?? cachedType.Name;
    }

    private void OnDestroy()
    {
        typeCache.Clear();
    }
}