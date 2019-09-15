using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[InitializeOnLoad]
public static class RFX4_Unity_Pain
{
#if UNITY_2018_3_OR_NEWER
    public static readonly string[] LWRP_Defines = new string[] {
        "KRIPTO_FX_LWRP_RENDERING"
    };

    public static readonly string[] HDRP_Defines = new string[] {
        "KRIPTO_FX_HDRP_RENDERING"
    };

    const string distortionShaderFileName = "UberDistortion";


    static RFX4_Unity_Pain()
    {
        var renderingType = GetRenderingType();
        switch (renderingType)
        {
            case FX_RenderingType.Old:
                break;
            case FX_RenderingType.LWRP:
            {
                var selectedGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
                AddDefineSymbols(LWRP_Defines, selectedGroup);
                if (selectedGroup != BuildTargetGroup.Standalone) AddDefineSymbols(LWRP_Defines, BuildTargetGroup.Standalone);
                DebugLog();
                break;
            }

            case FX_RenderingType.HDRP:
            {
                var selectedGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
                AddDefineSymbols(HDRP_Defines, selectedGroup);
                if (selectedGroup != BuildTargetGroup.Standalone) AddDefineSymbols(HDRP_Defines, BuildTargetGroup.Standalone);
                DebugLog();
                break;
            }

            default:
                throw new ArgumentOutOfRangeException();
        }


    }

    static FX_RenderingType GetRenderingType()
    {
        var rpa = GraphicsSettings.renderPipelineAsset;
        if (rpa == null) return FX_RenderingType.Old;
        if (rpa.name.ToUpper().Contains("LWR")) return FX_RenderingType.LWRP;
        if (rpa.name.ToUpper().Contains("HDR")) return FX_RenderingType.HDRP;
        return FX_RenderingType.Old;
    }

    enum FX_RenderingType
    {
        Old,
        LWRP,
        HDRP
    }

    public static void AddDefineSymbols(string[] defineStrings, BuildTargetGroup group)
    {
        string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
        List<string> allDefines = definesString.Split(';').ToList();
        allDefines.AddRange(defineStrings.Except(allDefines));
        PlayerSettings.SetScriptingDefineSymbolsForGroup(
            EditorUserBuildSettings.selectedBuildTargetGroup,
            string.Join(";", allDefines.ToArray()));
    }

    #region Upgrade To LWRP

    static void ReplaceShadersForLWRP()
    {
        var allShaders = Directory.GetFiles(Application.dataPath, "*.shader", SearchOption.AllDirectories);

        FixDistortionForLWRP(allShaders);

        var upgradeSrpFiles = Directory.GetFiles(Application.dataPath, "*.UpgradeSRP", SearchOption.AllDirectories);
        ReplaceShadersFromUpgradeSRP(upgradeSrpFiles, true);

    }

    private static void FixDistortionForLWRP(string[] allShaders)
    {
        string distortionShaderPath = null;
        string distortionShaderCode = null;
        foreach (var currentShader in allShaders)
        {
            if (currentShader.ToUpper().Contains(distortionShaderFileName.ToUpper()))
            {
                distortionShaderPath = currentShader;
                distortionShaderCode = File.ReadAllText(distortionShaderPath);
                if (distortionShaderCode.Contains("KriptoFX")) break;
            }
        }

        if (distortionShaderPath == null) return;

        distortionShaderCode = RemoveGrabPass(distortionShaderCode);
        distortionShaderCode = AddTag(distortionShaderCode, " \"LightMode\" = \"CustomDistortion\" ");

        File.WriteAllText(distortionShaderPath, distortionShaderCode);
    }

    #endregion

    #region Upgrade To HDRP

    static void ReplaceShadersForHDRP()
    {
        var allShaders = Directory.GetFiles(Application.dataPath, "*.shader", SearchOption.AllDirectories);
        var kriptoShaders = new List<string>();
        foreach (var shader in allShaders)
        {
            if(shader.Contains("KriptoFX")) kriptoShaders.Add(shader);
        }

        Add_DepthPyramidScale_Prefix(kriptoShaders);
        Remove_SOFTPARTICLES_ON(kriptoShaders);

        var upgradeSrpFiles = Directory.GetFiles(Application.dataPath, "*.UpgradeSRP", SearchOption.AllDirectories);
        ReplaceShadersFromUpgradeSRP(upgradeSrpFiles);

    }

    static void Add_DepthPyramidScale_Prefix(List<string> shaders)
    {
        foreach (var shader in shaders)
        {
            if (shader.Contains(distortionShaderFileName)) continue;

            var shaderCode = File.ReadAllText(shader);
            var depthScale = shaderCode.IndexOf("_DepthPyramidScale", StringComparison.OrdinalIgnoreCase);
            if(depthScale != -1) continue;

            var depthTex = shaderCode.IndexOf("_CameraDepthTexture", StringComparison.OrdinalIgnoreCase);
            if (depthTex == -1) continue;

            var cgincIDX = shaderCode.LastIndexOf("cginc", StringComparison.OrdinalIgnoreCase);
            if (cgincIDX == -1) continue;

            shaderCode = shaderCode.Insert(cgincIDX + 7, Environment.NewLine + " float4 _DepthPyramidScale;");

            var computeScreenIDX = shaderCode.LastIndexOf("ComputeScreenPos", StringComparison.OrdinalIgnoreCase);
            if(computeScreenIDX == -1) continue;
            var computeScreenEndIDX = shaderCode.IndexOf(";", computeScreenIDX, StringComparison.OrdinalIgnoreCase);

            var computeNameIDX = shaderCode.IndexOf(Environment.NewLine, computeScreenIDX - 25, StringComparison.OrdinalIgnoreCase);
            var computeNameLastIDX = shaderCode.IndexOf("=", computeNameIDX, StringComparison.OrdinalIgnoreCase);
            var computeName = shaderCode.Substring(computeNameIDX, computeNameLastIDX - computeNameIDX);
            computeName = computeName.Trim(' ');
            shaderCode = shaderCode.Insert(computeScreenEndIDX + 2, computeName + ".xy *= _DepthPyramidScale.xy;");
            File.WriteAllText(shader, shaderCode);
        }
    }

    static void Remove_SOFTPARTICLES_ON(List<string> shaders)
    {
        foreach (var shader in shaders)
        {
            if (shader.Contains(distortionShaderFileName)) continue;

            var shaderCode = File.ReadAllText(shader);

            int softIDX = 0;

            bool needUpdate = false;
            while (softIDX != -1)
            {
                
                softIDX = shaderCode.IndexOf("SOFTPARTICLES_ON", softIDX + 16, StringComparison.OrdinalIgnoreCase);
                if (softIDX > 0)
                {
                    needUpdate = true;
                    var stardIDX = shaderCode.IndexOf(Environment.NewLine, softIDX - 20, StringComparison.OrdinalIgnoreCase);
                    var endIDX = shaderCode.IndexOf(Environment.NewLine, stardIDX + 5, StringComparison.OrdinalIgnoreCase);
                    shaderCode = shaderCode.Remove(stardIDX, endIDX - stardIDX);

                    var endifIDX = shaderCode.IndexOf("#endif", endIDX, StringComparison.OrdinalIgnoreCase);
                    shaderCode = shaderCode.Remove(endifIDX, 6);

                    var elseIDX = shaderCode.IndexOf("#else", endIDX, StringComparison.OrdinalIgnoreCase);
                    if (elseIDX != -1 && elseIDX < endifIDX) shaderCode = shaderCode.Remove(elseIDX, 5);

                   
                }
            }
            if(needUpdate) File.WriteAllText(shader, shaderCode);
        }
    }



    #endregion

    static void ReplaceShadersFromUpgradeSRP(string[] upgradeSrpFiles, bool ignoreDistortion = false)
    {
        foreach (var srpFile in upgradeSrpFiles)
        {
            if(ignoreDistortion && srpFile.ToUpper().Contains(distortionShaderFileName.ToUpper())) return;
            var srpShaderPath = srpFile.Replace("UpgradeSRP", "shader");
            if (File.Exists(srpShaderPath))
            {
                var newText = File.ReadAllText(srpFile);
                File.WriteAllText(srpShaderPath, newText);
            }
        }
    }

    static string AddTag(string code, string tag)
    {
        var isAdded = code.IndexOf("CustomDistortion", StringComparison.OrdinalIgnoreCase);
        if (isAdded > 0) return code;

        var tagIdx = code.IndexOf("Tags", StringComparison.OrdinalIgnoreCase);
        var lastTagIdx = code.IndexOf("}", tagIdx, StringComparison.OrdinalIgnoreCase);
        return code.Insert(lastTagIdx, tag);
    }

    static string RemoveGrabPass(string code)
    {
        var grabIdx = code.IndexOf("GrabPass", StringComparison.OrdinalIgnoreCase);
        if (grabIdx <= 0) return code;

        var lastTagIdx = code.IndexOf("}", grabIdx, StringComparison.OrdinalIgnoreCase);
        return code.Remove(grabIdx, lastTagIdx - grabIdx + 1);
    }

#if KRIPTO_FX_LWRP_RENDERING
    public static void DebugLog()
    {
        //Debug.Log("KriptoFX shaders replaced for LWRP");
        ReplaceShadersForLWRP();
    }
#elif KRIPTO_FX_HDRP_RENDERING
    public static void DebugLog()
    {
        //Debug.Log("KriptoFX shaders replaced for HDRP");
        ReplaceShadersForHDRP();
    }
#else 
    public static void DebugLog()
    {
        //Debug.Log("OLD!");
    }
#endif
#endif
}