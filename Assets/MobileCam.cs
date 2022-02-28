using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using P = UnmanagedPoseTracker;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
public class MobileCam : MonoBehaviour
{
	public int bw_thresh = 20;
	public bool setBWThreshold = false;
	//public Sprite [] sprites ;
	int width = 1280;
	int height = 720;
	private IntPtr poser = IntPtr.Zero;
	IntPtr buffer = IntPtr.Zero;
	public Transform tree;
	private int currentPrivate = -1;
	public int current = 0;
	static string persistentPath = ""; 
	string[] imgPaths = {
		"frame_00010_rotated.png",
		"frame_00020_rotated.png",
		"frame_00030_rotated.png",
		"000004.jpg",
		"000009.jpg",
		"000015.jpg",
	};
	
	int index = 0;
	public bool loop = false;	
	List<string> outputs = new List<string>();
	List<DataZDK> data = new List<DataZDK>();
	int loopCount = 0;
	float delay = 1.0f;
	private void Awake() {
		persistentPath	= Application.dataPath + "/Resources/frames/";
	}
	void Start()
	{
		//print(DataSerializer.dataPath);
		DataZDK [] temp = new DataZDK[0];
		DataSerializer.DeserializeData(out temp);
		print(temp.Length);
		data.AddRange(temp);
		initPoseTracker();
		
	}

	// Update is called once per frame
	void Update()
	{
		
		// Adjust camera from outputs in file
		if (loop){
			delay -= Time.deltaTime;
			if (delay <=0 ){
				if ( loopCount >= data.Count ){
					loopCount = 0;
				}
				Vector3 rot = data[loopCount].EulerAngles;
				Vector3 pos = data[loopCount].Position;
				print(imgPaths[loopCount]);
				adjustCamera(rot, pos);
				loopCount++;
				delay = 1;
			}
			
		} //Debug mode
		else{
			if (current != currentPrivate){
				if (current >= 0 && current < imgPaths.Length ){
					predict2(LoadImage(persistentPath + imgPaths[current]));
					currentPrivate = current;
				}
			}
		}
		
		
	}
	
	private void OnApplicationQuit() {
		
		//DataSerializer.SerializeData(data.ToArray());
	}
	static byte[] LoadImage(string fileName)
	{
		using FileStream pngStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);

		using var image = new Bitmap(pngStream);

		var imgBytes = new Byte[image.Width * image.Height * 3];

		for (int i = 0; i < image.Height; i++)
		{
			for (int j = 0; j < image.Width; j++)
			{
				var pix = image.GetPixel(j, i);
				imgBytes[i * image.Width * 3 + j * 3 + 0] = pix.B;
				imgBytes[i * image.Width * 3 + j * 3 + 1] = pix.G;
				imgBytes[i * image.Width * 3 + j * 3 + 2] = pix.R;

			}
		}
		File.WriteAllBytes("handledFrame.bin", imgBytes);
		return imgBytes;
	}
	public static Texture2D FlipTextureVertically(Texture2D original)
	{
		var originalPixels = original.GetPixels();

		var newPixels = new UnityEngine.Color[originalPixels.Length];

		var width = original.width;
		var rows = original.height;

		for (var x = 0; x < width; x++)
		{
			for (var y = 0; y < rows; y++)
			{
				
				newPixels[x + y * width] = originalPixels[x + (rows - y - 1) * width];
			
			}
		}
		var newTex = new Texture2D(width, rows);
		
		newTex.SetPixels(newPixels);
		newTex.Apply();
		return newTex;
	}


	private void OnDestroy()
    {
		if (buffer != IntPtr.Zero )
			Marshal.FreeHGlobal(buffer);
		if (poser != IntPtr.Zero )
			P.DestroyPoser(poser);
	}

	// Convert texture2D -> byteArray (input of ZDK)
	public static byte[] LoadImage(Texture2D image)
	{

		var imgBytes = new Byte[image.width * image.height * 3];

		for (int i = 0; i < image.height; i++)
		{
			for (int j = 0; j < image.width; j++)
			{
				Color32 pix = image.GetPixel(j, i);
				imgBytes[i * image.width * 3 + j * 3 + 0] = pix.b;
				imgBytes[i * image.width * 3 + j * 3 + 1] = pix.g;
				imgBytes[i * image.width * 3 + j * 3 + 2] = pix.r;

			}
		}

		return imgBytes;
	}
	
	
	void initPoseTracker()
	{
		string ZTD_FILE = "data-android-treeofwishes1-1280x720-v1.2";
		string MARKER_GROUP = "treeofwishes1";

		TextAsset asset = Resources.Load(ZTD_FILE) as TextAsset;
		Byte[] bytes = asset.bytes;
		Debug.Log(bytes.Length);
		IntPtr buffer = Marshal.AllocHGlobal(cb: bytes.Length);
		Marshal.Copy(bytes, 0, buffer, bytes.Length);


		// Initialise devices
		poser = P.MakePoserFromBuffer(MARKER_GROUP, buffer);
		if (setBWThreshold)
			P.PoserSetBWThresh(poser, bw_thresh);
		Marshal.FreeHGlobal(buffer);

	}
	void predict2(byte[] byteArray)
	{
		if (poser == IntPtr.Zero)
			return;

		int max_num_detections = 10;
		int numMarkers = 5;

		// Get current time 
		var t = P.PoserNow(poser);
		int[] shape = { height, width, 3 };
		var c_rvecs = new float[3 * max_num_detections];
		var c_tvecs = new float[3 * max_num_detections];
		var c_class_ids = new int[max_num_detections];

		// Convert Frame to byte array
		Debug.Log(byteArray.Length);
		IntPtr frame = Marshal.AllocHGlobal(cb: byteArray.Length);
		Marshal.Copy(byteArray, 0, frame, byteArray.Length);

		// Make pose prediction
		P.PoserPredict(poser, frame, shape, t, c_rvecs, c_tvecs, c_class_ids, ref numMarkers);
		for (int i = 0; i < numMarkers; i++)
		{
			outputs.Add($"rvecs: {c_rvecs[i * 3 + 0]} {c_rvecs[i * 3 + 1]} {c_rvecs[i * 3 + 2]}" + $" tvecs: {c_tvecs[i * 3 + 0]} {c_tvecs[i * 3 + 1]} {c_tvecs[i * 3 + 2]}\n");
			Debug.Log($"rvecs: {c_rvecs[i * 3 + 0]} {c_rvecs[i * 3 + 1]} {c_rvecs[i * 3 + 2]}");
			Debug.Log($"tvecs: {c_tvecs[i * 3 + 0]} {c_tvecs[i * 3 + 1]} {c_tvecs[i * 3 + 2]}");
		}
		System.IO.File.WriteAllLines(Application.dataPath + "/unity_outputs.txt", outputs.ToArray());
		Marshal.FreeHGlobal(frame);
		P.DestroyTimePoint(t);


		Vector3 rotation = new Vector3(c_rvecs[0], c_rvecs[1] + 90, c_rvecs[2]);
		Vector3 pos = new Vector3(c_tvecs[0], c_tvecs[1], c_tvecs[2]);
		// DataZDK d = new DataZDK(){
		// 	Position = new SerializableVector3(pos),
		// 	EulerAngles = new SerializableVector3(rotation),
		// };
		// data.Add(d);
		adjustCamera(rotation, pos);

	}
	void predict(Texture2D image)
	{
		if (poser == IntPtr.Zero)
			return;

		int max_num_detections = 10;
		int numMarkers = 5;

		// Get current time 
		var t = P.PoserNow(poser);
		int[] shape = { height, width, 3 };
		var c_rvecs = new float[3 * max_num_detections];
		var c_tvecs = new float[3 * max_num_detections];
		var c_class_ids = new int[max_num_detections];

		// Convert Frame to byte array
		byte[] byteArray = LoadImage(image);
		Debug.Log(byteArray.Length);
		IntPtr frame = Marshal.AllocHGlobal(cb: byteArray.Length);
		Marshal.Copy(byteArray, 0, frame, byteArray.Length);

		// Make pose prediction
		P.PoserPredict(poser, frame, shape, t, c_rvecs, c_tvecs, c_class_ids, ref numMarkers);
		for(int i=0; i < numMarkers; i++)
		{
			outputs.Add($"rvecs: {c_rvecs[i * 3 + 0]} {c_rvecs[i * 3 + 1]} {c_rvecs[i * 3 + 2]}" + $" tvecs: {c_tvecs[i * 3 + 0]} {c_tvecs[i * 3 + 1]} {c_tvecs[i * 3 + 2]}");    
			Debug.Log($"rvecs: {c_rvecs[i * 3 + 0]} {c_rvecs[i * 3 + 1]} {c_rvecs[i * 3 + 2]}");
			Debug.Log($"tvecs: {c_tvecs[i * 3 + 0]} {c_tvecs[i * 3 + 1]} {c_tvecs[i * 3 + 2]}");
		}
		System.IO.File.WriteAllLines(Application.dataPath + "/unity_outputs.txt", outputs.ToArray());
		Marshal.FreeHGlobal(frame);
		P.DestroyTimePoint(t);


		Vector3 rotation = new Vector3(c_rvecs[0], c_rvecs[1] + 90, c_rvecs[2]);
		Vector3 pos = new Vector3(c_tvecs[0], c_tvecs[1], c_tvecs[2]);
		adjustCamera(rotation, pos);
	
	}
	void adjustCamera(Vector3 rotationOfTree, Vector3 positionOfTree)
    {
		Vector3 eulerAngle = convertAxisAngleToEulerAngle(rotationOfTree);

		var matrix =  (Matrix4x4.TRS(positionOfTree - tree.position, Quaternion.Euler(eulerAngle), Vector3.one)).inverse;

		Vector3 pos = new Vector3(matrix[0,3], matrix[1,3], matrix[2,3]);
		Quaternion rot = matrix.rotation;

		Camera.main.transform.position = pos;
		Camera.main.transform.rotation = rot;
		Camera.main.transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward, Camera.main.transform.up);
		Camera.main.transform.position -= 3*Camera.main.transform.forward; 
	}
	Vector3 convertAxisAngleToEulerAngle(Vector3 axisAngle)
    {
		float r_x = axisAngle.x;
		float r_y = axisAngle.y ;
		float r_z = axisAngle.z ;
		float theta = (float)(Math.Sqrt(r_x * r_x + r_y * r_y + r_z * r_z) * 180 / Math.PI);
		Vector3 axis = new Vector3(r_x, r_y, r_z);
		Quaternion rot1 = Quaternion.AngleAxis(theta, axis);
		var rot2 = Quaternion.Euler(-90 , 180, 0 );
		return (rot1* rot2).eulerAngles;
	}
	

}