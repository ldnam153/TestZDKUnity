using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

public static class DataSerializer
{
    public static readonly string dataPath = Path.Combine(Application.persistentDataPath, "data.txt");

    public static void SerializeData(DataZDK [] data)
    {
      

        using(var stream = File.Open(dataPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
        {
            var bf = new BinaryFormatter();
            bf.Serialize(stream, data);
        }
    }

    public static bool DeserializeData(out DataZDK [] data)
    {
        data = null;

        if (!File.Exists(dataPath)) 
        {
            Debug.LogError("File does not exist.");
            return false;
        }
        
        using(var stream = File.Open(dataPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var bf = new BinaryFormatter();                
            data = bf.Deserialize(stream) as DataZDK [];    
        } 

        return data != null; 
    }            
}

[Serializable]
public class DataZDK
{
    public SerializableVector3 Position {get; set;}
    public SerializableVector3 EulerAngles {get; set;}
}

[Serializable]
public class SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3(Vector3 vec)
    {
        x = vec.x;
        y = vec.y;
        z = vec.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x,y,z);
    }

    public static implicit operator Vector3(SerializableVector3 vec)
    {
        return vec.ToVector3();
    }

    public static implicit operator SerializableVector3(Vector3 vec)
    {
        return new SerializableVector3(vec);
    }
}