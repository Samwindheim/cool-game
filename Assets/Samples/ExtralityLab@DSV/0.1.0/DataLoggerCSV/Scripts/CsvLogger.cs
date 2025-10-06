using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CsvLogger : MonoBehaviour
{
    public static CsvLogger Instance { get; private set; }

    [Header("File")]
    [SerializeField] private string filePrefix = "session";

    [Header("Optional Pose Sampling")]
    [SerializeField] private bool logTransform = false;
    [SerializeField] private Transform target;            // e.g., Main Camera (HMD) or a controller
    [SerializeField] private float sampleHz = 5f;         // samples per second (0 = off)

    // Deploying in WebGL does not allow for disk write, so we need
    // a special way to download the data from the memory buffer as a web file.
#if UNITY_WEBGL && !UNITY_EDITOR
    private StringBuilder bufferWebGL;
#else
    private StreamWriter writer;
#endif

    private string filename;
    private string filepath;    
    private float nextSampleTime;
    private bool isConfigured = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        //// Uncomment if you want the logging to begin automatically.
        // StartLogger();
    }

    private void OnApplicationQuit()
    {
        if(isConfigured) StopLogger();
    }

    private void Update()
    {
        if (!logTransform || target == null || sampleHz <= 0f) return;

        if (Time.unscaledTime >= nextSampleTime)
        {
            nextSampleTime = Time.unscaledTime + (1f / sampleHz);
            LogInternal("transform_sample", "");
        }
    }

    public void StartLogger()
    {
        if(isConfigured) StopLogger();

        string ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        filename = $"{filePrefix}_{ts}.csv";
        string dir = Application.persistentDataPath;
        filepath = Path.Combine(dir, filename);

#if UNITY_WEBGL && !UNITY_EDITOR
        bufferWebGL = new StringBuilder(4096);
        bufferWebGL.AppendLine("timestamp_utc,scene,event,details,px,py,pz,rx,ry,rz,rw");
        // Small breadcrumb to find the file path in Console:
        Debug.Log($"[CsvLoggerWebGL] buffering in memory. File will download when stopped: {filename}");
#else
        writer = new StreamWriter(filepath, false, Encoding.UTF8);
        writer.WriteLine("timestamp_now,scene,event,details,px,py,pz,rx,ry,rz,rw");
        writer.Flush();
        // Small breadcrumb to find the file path in Console:
        Debug.Log($"[CsvLogger] Writing to: {filepath}");
#endif

        isConfigured = true;
    }

    public void StopLogger()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        var bytes = Encoding.UTF8.GetBytes(bufferWebGL.ToString());

        // In WEBGL: Download file when closing 
        if (WebGLFileSaver.IsSavingSupported())
        {
            Debug.Log($"[CsvLoggerWebGL] File saved as: {filename}");
            WebGLFileSaver.SaveFile(bytes, filename);
        }
#else
        Debug.Log($"[CsvLogger] File available at: {filepath}");
        writer?.Flush();
        writer?.Close();
#endif

        isConfigured = false; // a new logger must be started
    }

    public static void LogEvent(string eventToLog, string details = "")
    {
        if (Instance == null) return;
        Instance.LogInternal(eventToLog, details);
    }

    private void LogInternal(string evt, string details)
    {
        if(!isConfigured) return;

        string tstamp = DateTime.Now.ToString("o"); // ISO-8601 UTC
        string scene = SceneManager.GetActiveScene().name;

        // Default empty pose
        float px = 0, py = 0, pz = 0, rx = 0, ry = 0, rz = 0, rw = 1;

        // Record the transform of the target
        if (target != null)
        {
            Vector3 p = target.position;
            Quaternion r = target.rotation;
            px = p.x; py = p.y; pz = p.z;
            rx = r.x; ry = r.y; rz = r.z; rw = r.w;
        }

        // Escape commas in details by wrapping in quotes if needed
        if (!string.IsNullOrEmpty(details) && details.Contains(",")) details = $"\"{details}\"";

#if UNITY_WEBGL && !UNITY_EDITOR
        bufferWebGL.AppendLine($"{tstamp},{scene},{evt},{details},{px},{py},{pz},{rx},{ry},{rz},{rw}");
#else
        writer.WriteLine($"{tstamp},{scene},{evt},{details},{px},{py},{pz},{rx},{ry},{rz},{rw}");
        writer.Flush();
#endif
    }

}