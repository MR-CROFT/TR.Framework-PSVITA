using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Profiling;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Rendering;
#endif

namespace RetroConsoleAnalyzers
{
    public class PSVitaPerformanceAnalyzer2018 : MonoBehaviour
    {
        [Header("PS Vita Hardware Limits")]
        private const int MAX_POLYGONS_PER_FRAME = 150000;
        private const int MAX_VRAM_MB = 100;
        private const int MAX_DRAW_CALLS = 250;
        private const int MAX_MATERIAL_CHANGES = 100;
        private const int MAX_REALTIME_LIGHTS = 4;
        private const int TARGET_FPS = 30;
        private const int TARGET_RESOLUTION_WIDTH = 960;
        private const int TARGET_RESOLUTION_HEIGHT = 544;
        
        [Header("Display Settings")]
        [SerializeField] private Vector2 statsPosition = new Vector2(10, 10);
        
        [Header("Analysis Options")]
        [Tooltip("Count only visible meshes (those with renderers enabled)")]
        [SerializeField] private bool countOnlyVisible = true;
        
        [Tooltip("Estimate VRAM as if textures were compressed for Vita (PVRTC/ETC)")]
        [SerializeField] private bool useVitaVRAMEstimate = true;
        
        [Tooltip("Show detailed optimization suggestions")]
        [SerializeField] private bool showOptimizationSuggestions = true;
        
        private int currentPolygons;
        private int currentDrawCalls;
        private int currentMaterialChanges;
        private int currentRealtimeLights;
        private int currentShadowCasters;
        private float currentVRAMUsage;
        private float currentFPS;
        private int compatibilityLevel;
        private int totalMeshes;
        
        private List<string> optimizationSuggestions = new List<string>();
        
        private float fpsTimer = 0f;
        private int frameCount = 0;
        private const float fpsUpdateInterval = 0.5f;
        
        private GUIStyle headerStyle;
        private GUIStyle normalStyle;
        private GUIStyle warningStyle;
        private GUIStyle greenStyle;
        private GUIStyle orangeStyle;
        private GUIStyle redStyle;
        private GUIStyle smallStyle;
        private bool stylesInitialized;
        
        private void Update()
        {
            AnalyzeScene();
            UpdateFPS();
        }
        
        private void AnalyzeScene()
        {
            currentPolygons = CountScenePolygons();
            currentDrawCalls = GetDrawCalls();
            currentMaterialChanges = CountMaterialChanges();
            currentRealtimeLights = CountRealtimeLights();
            currentShadowCasters = CountShadowCasters();
            currentVRAMUsage = CalculateVRAMUsage();
            
            GenerateOptimizationSuggestions();
            CheckVitaCompatibility();
        }
        
        private int CountScenePolygons()
        {
            int totalPolygons = 0;
            totalMeshes = 0;
            int skippedMeshes = 0;
            
            MeshFilter[] meshFilters = FindObjectsOfType<MeshFilter>();
            
            foreach (MeshFilter meshFilter in meshFilters)
            {
                if (meshFilter == null || meshFilter.sharedMesh == null)
                    continue;
                    
                if (countOnlyVisible)
                {
                    Renderer renderer = meshFilter.GetComponent<Renderer>();
                    if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                        continue;
                }
                
                Mesh mesh = meshFilter.sharedMesh;
                
                if (mesh.isReadable)
                {
                    totalPolygons += mesh.triangles.Length / 3;
                }
                else
                {
                    totalPolygons += mesh.vertexCount / 3;
                    skippedMeshes++;
                }
                
                totalMeshes++;
            }
            
            SkinnedMeshRenderer[] skinnedRenderers = FindObjectsOfType<SkinnedMeshRenderer>();
            
            foreach (SkinnedMeshRenderer skinnedRenderer in skinnedRenderers)
            {
                if (skinnedRenderer == null || skinnedRenderer.sharedMesh == null)
                    continue;
                    
                if (countOnlyVisible && (!skinnedRenderer.enabled || !skinnedRenderer.gameObject.activeInHierarchy))
                    continue;
                
                Mesh mesh = skinnedRenderer.sharedMesh;
                
                if (mesh.isReadable)
                {
                    totalPolygons += mesh.triangles.Length / 3;
                }
                else
                {
                    totalPolygons += mesh.vertexCount / 3;
                    skippedMeshes++;
                }
                
                totalMeshes++;
            }
            
            return totalPolygons;
        }
        
        private int GetDrawCalls()
        {
#if UNITY_EDITOR
            return UnityStats.drawCalls;
#else
            return 0;
#endif
        }

        private int CountMaterialChanges()
        {
            HashSet<Material> uniqueMaterials = new HashSet<Material>();
            
            Renderer[] renderers = FindObjectsOfType<Renderer>();
            
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                    continue;
                
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material != null)
                    {
                        uniqueMaterials.Add(material);
                    }
                }
            }
            
            return uniqueMaterials.Count;
        }

        private int CountRealtimeLights()
        {
            Light[] lights = FindObjectsOfType<Light>();
            int realtimeCount = 0;
            
            foreach (Light light in lights)
            {
                if (light != null && light.enabled && light.gameObject.activeInHierarchy)
                {
                    if (light.lightmapBakeType == LightmapBakeType.Realtime || light.lightmapBakeType == LightmapBakeType.Mixed)
                    {
                        realtimeCount++;
                    }
                }
            }
            
            return realtimeCount;
        }
        
        private int CountShadowCasters()
        {
            int shadowCasterCount = 0;
            Renderer[] renderers = FindObjectsOfType<Renderer>();
            
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null && renderer.enabled && renderer.gameObject.activeInHierarchy)
                {
                    if (renderer.shadowCastingMode != ShadowCastingMode.Off)
                    {
                        shadowCasterCount++;
                    }
                }
            }
            
            return shadowCasterCount;
        }
        
        private float CalculateVRAMUsage()
        {
            long totalTextureMemory = 0;
            HashSet<Texture> processedTextures = new HashSet<Texture>();
            
            Renderer[] renderers = FindObjectsOfType<Renderer>();
            
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                    continue;
                
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material == null)
                        continue;
                    
                    Shader shader = material.shader;
                    int propertyCount = ShaderUtil.GetPropertyCount(shader);
                    
                    for (int i = 0; i < propertyCount; i++)
                    {
                        if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                        {
                            string propertyName = ShaderUtil.GetPropertyName(shader, i);
                            Texture texture = material.GetTexture(propertyName);
                            
                            if (texture != null && !processedTextures.Contains(texture))
                            {
                                processedTextures.Add(texture);
                                
                                long textureSize = Profiler.GetRuntimeMemorySizeLong(texture);
                                
                                if (useVitaVRAMEstimate)
                                {
                                    int width = texture.width;
                                    int height = texture.height;
                                    
                                    int vitaWidth = Mathf.Min(width, 1024);
                                    int vitaHeight = Mathf.Min(height, 1024);
                                    
                                    float scaleFactor = (vitaWidth * vitaHeight) / (float)(width * height);
                                    long estimatedVitaSize = (long)(textureSize * scaleFactor * 0.25f);
                                    
                                    totalTextureMemory += estimatedVitaSize;
                                }
                                else
                                {
                                    totalTextureMemory += textureSize;
                                }
                            }
                        }
                    }
                }
            }
            
            return totalTextureMemory / (1024f * 1024f);
        }
        
        private void GenerateOptimizationSuggestions()
        {
            optimizationSuggestions.Clear();
            
            float polygonPercentage = (currentPolygons / (float)MAX_POLYGONS_PER_FRAME) * 100f;
            if (polygonPercentage > 80f)
            {
                optimizationSuggestions.Add("HIGH POLY COUNT: Reduce geometry or use LOD system");
            }
            
            float materialPercentage = (currentMaterialChanges / (float)MAX_MATERIAL_CHANGES) * 100f;
            if (materialPercentage > 80f)
            {
                optimizationSuggestions.Add("TOO MANY MATERIALS: Use texture atlases to combine materials");
            }
            
            float vramPercentage = (currentVRAMUsage / MAX_VRAM_MB) * 100f;
            if (vramPercentage > 80f)
            {
                optimizationSuggestions.Add("HIGH VRAM: Reduce texture sizes or use compressed formats");
            }
            
            if (currentRealtimeLights > MAX_REALTIME_LIGHTS)
            {
                optimizationSuggestions.Add(string.Format("TOO MANY LIGHTS: Use baked lighting ({0} lights over limit)", currentRealtimeLights - MAX_REALTIME_LIGHTS));
            }
            
            if (currentShadowCasters > 50)
            {
                optimizationSuggestions.Add(string.Format("TOO MANY SHADOWS: Disable shadows on small objects ({0} casters)", currentShadowCasters));
            }
            
            float drawCallPercentage = (currentDrawCalls / (float)MAX_DRAW_CALLS) * 100f;
            if (drawCallPercentage > 80f)
            {
                optimizationSuggestions.Add("HIGH DRAW CALLS: Enable static batching and GPU instancing");
            }
            
            if (Screen.width > TARGET_RESOLUTION_WIDTH || Screen.height > TARGET_RESOLUTION_HEIGHT)
            {
                optimizationSuggestions.Add(string.Format("RESOLUTION: Target 960x544 for Vita (current: {0}x{1})", Screen.width, Screen.height));
            }
            
            if (currentFPS < TARGET_FPS - 5)
            {
                optimizationSuggestions.Add("LOW FPS: Consider simplifying post-processing effects");
            }
        }
        
        private void CheckVitaCompatibility()
        {
            float polygonPercentage = (currentPolygons / (float)MAX_POLYGONS_PER_FRAME) * 100f;
            float materialPercentage = (currentMaterialChanges / (float)MAX_MATERIAL_CHANGES) * 100f;
            float vramPercentage = (currentVRAMUsage / MAX_VRAM_MB) * 100f;
            float lightPercentage = (currentRealtimeLights / (float)MAX_REALTIME_LIGHTS) * 100f;
            
            float maxPercentage = Mathf.Max(polygonPercentage, materialPercentage, vramPercentage, lightPercentage);
            
            bool isCriticalFPS = currentFPS < 20f;
            bool requiresMajorWork = maxPercentage > 150f || isCriticalFPS;
            bool needsOptimization = maxPercentage > 100f && maxPercentage <= 150f;
            
            if (requiresMajorWork)
            {
                compatibilityLevel = 0;
            }
            else if (needsOptimization)
            {
                compatibilityLevel = 1;
            }
            else
            {
                compatibilityLevel = 2;
            }
        }
        
        private void UpdateFPS()
        {
            frameCount++;
            fpsTimer += Time.unscaledDeltaTime;
            
            if (fpsTimer >= fpsUpdateInterval)
            {
                currentFPS = frameCount / fpsTimer;
                frameCount = 0;
                fpsTimer = 0f;
            }
        }
        
        private void OnGUI()
        {
            InitializeStyles();
            
            float width = 450;
            float baseHeight = 450;
            float suggestionHeight = showOptimizationSuggestions ? optimizationSuggestions.Count * 22 : 0;
            float height = baseHeight + suggestionHeight;
            
            Rect backgroundRect = new Rect(statsPosition.x, statsPosition.y, width, height);
            
            GUI.Box(backgroundRect, "");
            GUI.color = new Color(0, 0, 0, 0.85f);
            GUI.DrawTexture(backgroundRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            GUILayout.BeginArea(new Rect(statsPosition.x + 10, statsPosition.y + 10, width - 20, height - 20));
            
            GUILayout.Label("PS VITA PORT ANALYZER", headerStyle);
            GUILayout.Space(10);
            
            GUILayout.Label(string.Format("Current FPS: {0:F1} / {1}", currentFPS, TARGET_FPS), normalStyle);
            GUILayout.Label(string.Format("Target Resolution: {0}x{1} (current: {2}x{3})", TARGET_RESOLUTION_WIDTH, TARGET_RESOLUTION_HEIGHT, Screen.width, Screen.height), smallStyle);
            
            GUILayout.Space(8);
            GUILayout.Label(string.Format("Polygons/Frame: {0:N0} / {1:N0}", currentPolygons, MAX_POLYGONS_PER_FRAME), normalStyle);
            float polygonPercentage = (currentPolygons / (float)MAX_POLYGONS_PER_FRAME) * 100f;
            DrawProgressBar(polygonPercentage, polygonPercentage > 100f);
            
            GUILayout.Space(5);
            GUILayout.Label(string.Format("Material Changes: {0} / {1}", currentMaterialChanges, MAX_MATERIAL_CHANGES), normalStyle);
            float materialPercentage = (currentMaterialChanges / (float)MAX_MATERIAL_CHANGES) * 100f;
            DrawProgressBar(materialPercentage, materialPercentage > 100f);
            
            GUILayout.Space(5);
            string vramLabel = useVitaVRAMEstimate 
                ? string.Format("VRAM Usage: {0:F2} MB / {1} MB (Vita estimate)", currentVRAMUsage, MAX_VRAM_MB)
                : string.Format("VRAM Usage: {0:F2} MB / {1} MB (actual)", currentVRAMUsage, MAX_VRAM_MB);
            GUILayout.Label(vramLabel, normalStyle);
            float vramPercentage = (currentVRAMUsage / MAX_VRAM_MB) * 100f;
            DrawProgressBar(vramPercentage, vramPercentage > 100f);
            
            GUILayout.Space(5);
            GUILayout.Label(string.Format("Realtime Lights: {0} / {1}", currentRealtimeLights, MAX_REALTIME_LIGHTS), normalStyle);
            float lightPercentage = (currentRealtimeLights / (float)MAX_REALTIME_LIGHTS) * 100f;
            DrawProgressBar(lightPercentage, lightPercentage > 100f);
            
            GUILayout.Space(5);
            GUILayout.Label(string.Format("Shadow Casters: {0}", currentShadowCasters), smallStyle);
            GUILayout.Label(string.Format("Draw Calls: {0} / {1}", currentDrawCalls, MAX_DRAW_CALLS), smallStyle);
            
            GUILayout.Space(15);
            
            string compatibilityText;
            GUIStyle resultStyle;
            
            switch (compatibilityLevel)
            {
                case 2:
                    compatibilityText = "✓ EASY PORT TO PS VITA";
                    resultStyle = greenStyle;
                    break;
                case 1:
                    compatibilityText = "⚠ NEEDS OPTIMIZATION";
                    resultStyle = orangeStyle;
                    break;
                default:
                    compatibilityText = "✗ REQUIRES MAJOR WORK";
                    resultStyle = redStyle;
                    break;
            }
            
            GUILayout.Label(compatibilityText, resultStyle);
            
            if (showOptimizationSuggestions && optimizationSuggestions.Count > 0)
            {
                GUILayout.Space(10);
                GUILayout.Label("OPTIMIZATION SUGGESTIONS:", warningStyle);
                foreach (string suggestion in optimizationSuggestions)
                {
                    GUILayout.Label("• " + suggestion, smallStyle);
                }
            }
            
            GUILayout.Space(10);
            GUILayout.Label("Meshes: " + totalMeshes, smallStyle);
            
            GUILayout.EndArea();
        }
        
        private void DrawProgressBar(float percentage, bool isOverLimit)
        {
            Rect barBackgroundRect = GUILayoutUtility.GetRect(380, 20);
            
            GUI.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            GUI.DrawTexture(barBackgroundRect, Texture2D.whiteTexture);
            
            float clampedPercentage = Mathf.Clamp01(percentage / 100f);
            Rect barFillRect = new Rect(barBackgroundRect.x, barBackgroundRect.y, barBackgroundRect.width * clampedPercentage, barBackgroundRect.height);
            
            if (percentage > 150f)
                GUI.color = new Color(0.8f, 0f, 0f, 1f);
            else if (percentage > 100f)
                GUI.color = new Color(1f, 0.6f, 0f, 1f);
            else if (percentage > 80f)
                GUI.color = new Color(1f, 1f, 0f, 1f);
            else
                GUI.color = new Color(0f, 0.8f, 0f, 1f);
            
            GUI.DrawTexture(barFillRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            GUI.Label(barBackgroundRect, string.Format("{0:F1}%", percentage), normalStyle);
        }
        
        private void InitializeStyles()
        {
            if (stylesInitialized) return;
            
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 16;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            headerStyle.normal.textColor = Color.white;
            
            normalStyle = new GUIStyle(GUI.skin.label);
            normalStyle.fontSize = 12;
            normalStyle.alignment = TextAnchor.MiddleLeft;
            normalStyle.normal.textColor = Color.white;
            
            smallStyle = new GUIStyle(GUI.skin.label);
            smallStyle.fontSize = 10;
            smallStyle.alignment = TextAnchor.MiddleLeft;
            smallStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            
            warningStyle = new GUIStyle(GUI.skin.label);
            warningStyle.fontSize = 11;
            warningStyle.fontStyle = FontStyle.Bold;
            warningStyle.alignment = TextAnchor.MiddleLeft;
            warningStyle.normal.textColor = new Color(1f, 0.8f, 0f, 1f);
            
            greenStyle = new GUIStyle(GUI.skin.label);
            greenStyle.fontSize = 18;
            greenStyle.fontStyle = FontStyle.Bold;
            greenStyle.alignment = TextAnchor.MiddleCenter;
            greenStyle.normal.textColor = new Color(0f, 1f, 0f, 1f);
            
            orangeStyle = new GUIStyle(GUI.skin.label);
            orangeStyle.fontSize = 18;
            orangeStyle.fontStyle = FontStyle.Bold;
            orangeStyle.alignment = TextAnchor.MiddleCenter;
            orangeStyle.normal.textColor = new Color(1f, 0.6f, 0f, 1f);
            
            redStyle = new GUIStyle(GUI.skin.label);
            redStyle.fontSize = 18;
            redStyle.fontStyle = FontStyle.Bold;
            redStyle.alignment = TextAnchor.MiddleCenter;
            redStyle.normal.textColor = new Color(1f, 0f, 0f, 1f);
            
            stylesInitialized = true;
        }
    }
}
