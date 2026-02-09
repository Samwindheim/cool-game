using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

public class WebGLFileSaver
{

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void UNITY_SAVE(string content, string name, string MIMEType);
    
    [DllImport ("__Internal")]
    private static extern void UNITY_SAVE_BYTEARRAY(byte[] array, int byteLength, string name, string MIMEType);

    [DllImport("__Internal")]
    private static extern void init();

    [DllImport("__Internal")]
    private static extern bool UNITY_IS_SUPPORTED();
#endif

    static bool hasinit = false;

    public static void SaveFile(string content, string fileName, string MIMEType = "text/plain;charset=utf-8")
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!CheckSupportAndInit()) return;
        UNITY_SAVE (content, fileName, MIMEType);
#else
        Debug.LogWarning("WebGLFileSaver.SaveFile(string, ...) is only supported in WebGL builds.");
#endif
    }
    
    public static void SaveFile(byte[] content, string fileName, string MIMEType = "text/plain;charset=utf-8")
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (content == null)
        {
            Debug.LogError("null parameter passed for content byte array");
            return;
        }
        if (!CheckSupportAndInit()) return;

        UNITY_SAVE_BYTEARRAY (content, content.Length, fileName, MIMEType);
#else
        Debug.LogWarning("WebGLFileSaver.SaveFile(byte[], ...) is only supported in WebGL builds.");
#endif
    }

    static bool CheckSupportAndInit()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (Application.isEditor)
        {
            Debug.Log("Saving will not work in editor.");
            return false;
        }
        if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
            Debug.Log("Saving must be on a WebGL build.");
            return false;
        }

        CheckInit();

        if (!IsSavingSupported())
        { 
            Debug.LogWarning("Saving is not supported on this device.");
            return false;
        }
        return true;
#else
        // Non-WebGL platforms: saving is not supported by this helper.
        return false;
#endif
    }

    static void CheckInit()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!hasinit)
        {
            init();
            hasinit = true;
        }
#endif
    }

    public static bool IsSavingSupported()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (Application.isEditor)
        {
            Debug.Log("Saving will not work in editor.");
            return false;
        }
        if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
            Debug.Log("Saving must be on a WebGL build.");
            return false;
        }
        CheckInit();
        return UNITY_IS_SUPPORTED();
#else
        Debug.Log("WebGLFileSaver.IsSavingSupported() is only relevant for WebGL builds.");
        return false;
#endif
    }
}
