using System;
using System.Runtime.InteropServices;

public class UnmanagedPoseTracker
{
    /*
     * https://docs.microsoft.com/en-us/previous-versions/dotnet/netframework-4.0/hk9wyw21%28v%3dvs.100%29
     * https://docs.microsoft.com/en-us/previous-versions/dotnet/netframework-4.0/2k1k68kw%28v%3dvs.100%29
     */
    [DllImport("zvision.dll", EntryPoint = "MakePoser", CharSet = CharSet.Ansi)]
    public static extern IntPtr MakePoser(string marker_group, string data_path);

    [DllImport("zvision.dll", EntryPoint = "MakePoserFromFileDescriptor", CharSet = CharSet.Ansi)]
    public static extern IntPtr MakePoserFromFileDescriptor(string marker_group, int file_descriptor, int start, int end);

    [DllImport("zvision.dll", EntryPoint = "MakePoserFromBuffer", CharSet = CharSet.Ansi)]
    public static extern IntPtr MakePoserFromBuffer(string marker_group, IntPtr buffer);

    [DllImport("zvision.dll", EntryPoint = "PoserEnableFileLogging")]
    public static extern int EnableFileLogging(IntPtr poser);

    [DllImport("zvision.dll", EntryPoint = "DestroyPoser")]
    public static extern int DestroyPoser(IntPtr poser);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ZDevice
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public String serialNumber;

        public int id;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
        public String model;

        public int fps;
        public float brightness;
        public float threshold;
        public float autoExposure;
        public float exposure;
        public int frameWidth;
        public int frameHeight;
        public int contrast;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public String name;
    }

    [DllImport("zvision.dll", EntryPoint = "MakeDeviceManager")]
    public static extern IntPtr MakeDeviceManager();

    [DllImport("zvision.dll", EntryPoint = "MakeDeviceManager1")]
    public static extern IntPtr MakeDeviceManager1(int default_frame_width, int default_frame_height);

    [DllImport("zvision.dll", EntryPoint = "DestroyDeviceManager")]
    public static extern int DestroyDeviceManager(IntPtr device_manager);

    [DllImport("zvision.dll", EntryPoint = "GetNumDevices")]
    public static extern int GetNumDevices(IntPtr device_manager);

    [DllImport("zvision.dll", EntryPoint = "GetDeviceList")]
    public static extern void GetDeviceList(IntPtr device_manager, [In, Out] ZDevice[] device_list,
        int num_devices);

    [DllImport("zvision.dll", EntryPoint = "PoserPredict1")]
    public static extern void PoserPredict1(IntPtr poser, IntPtr frame, IntPtr t, [In, Out] float[] rvecs,
        [In, Out] float[] tvecs, [In, Out] int[] class_ids, ref int num_markers);

    [DllImport("zvision", EntryPoint = "PoserPredictTransformation")]
    public static extern int PoserPredictTransformation(IntPtr poser, IntPtr frame, int[] frame_shape, IntPtr t, [In, Out] float[] tmats, [In, Out] int[] class_ids, ref int num_markers);

    [DllImport("zvision.dll", EntryPoint = "PoserPredictTransformation1")]
    public static extern void PoserPredictTransformation1(IntPtr poser, IntPtr frame, IntPtr t, [In, Out] float[] tmats, [In, Out] int[] class_ids, ref int num_markers);

    [DllImport("zvision", EntryPoint = "PoserPredict")]
    public static extern int PoserPredict(IntPtr poser, IntPtr frame, int[] frame_shape, IntPtr t, [In, Out] float[] rvecs, [In, Out] float[] tvecs, [In, Out] int[] class_ids, ref int num_markers);

    [DllImport("zvision", EntryPoint = "PoserGetMarkerIds")]
    public static extern int PoserGetMarkerIds(IntPtr poser, int[] marker_ids, ref int num_markers);

    [DllImport("zvision", EntryPoint = "PoserGetMarkerName")]
    public static extern int PoserGetMarkerName(IntPtr poser, int marker_id, [In, Out] char[] marker_name);

    [DllImport("zvision", EntryPoint = "PoserIsReady")]
    public static extern int PoserIsReady(IntPtr poser);

    [DllImport("zvision", EntryPoint = "PoserSetBWThresh")]
    public static extern int PoserSetBWThresh(IntPtr poser, int bw_thresh);

    [DllImport("zvision", EntryPoint = "PoserSetMarkerThresholds")]
    public static extern int PoserSetMarkerThresholds(IntPtr poser, int min, int max);

    [DllImport("zvision", EntryPoint = "PoserSetDetectionMode")]
    public static extern int PoserSetDetectionMode(IntPtr poser, int detection_mode);

    [DllImport("zvision", EntryPoint = "PoserSetUseMotionFilter")]
    public static extern int PoserSetUseMotionFilter(IntPtr poser, int use_motion_filter);

    [DllImport("zvision", EntryPoint = "PoserSetMotionFilterParams")]
    public static extern int PoserSetMotionFilterParams(IntPtr poser, float alpha, float beta, float gamma);

    [DllImport("zvision.dll", EntryPoint = "ReadFrameFromSensor")]
    public static extern IntPtr ReadFrameFromSensor(IntPtr device_manager, int sensor_index);

    [DllImport("zvision.dll", EntryPoint = "DestroyFrame")]
    public static extern int DestroyFrame(IntPtr frame);

    [DllImport("zvision.dll", EntryPoint = "PoserNow")]
    public static extern IntPtr PoserNow(IntPtr poser);

    [DllImport("zvision.dll", EntryPoint = "DestroyTimePoint")]
    public static extern int DestroyTimePoint(IntPtr t);

    [DllImport("zvision.dll", EntryPoint = "DebugWriteFrame1")]
    public static extern int DebugWriteFrame1(IntPtr frame);
}

public class PoseTracker
{
    public PoseTracker(String markerGroup, String dataPath)
    {
        this.Poser = UnmanagedPoseTracker.MakePoser(markerGroup, dataPath);
    }

    public int Predict()
    {
        return 0;
    }

    protected IntPtr Poser;
}

public class DeviceManager
{
    public DeviceManager()
    {

    }
}
