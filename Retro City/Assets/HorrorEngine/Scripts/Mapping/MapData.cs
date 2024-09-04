using System;
using System.Collections.Generic;
using UnityEngine;

namespace HorrorEngine
{
    [System.Serializable]
    public class ShapeData
    {
        public Vector3[] Points;
    }

    [Serializable]
    public class MapElementTransform
    {
        public Vector2 Offset;
        public Vector3 Scale;
        public float Rotation;
    }

    [Serializable]
    public class MapImageSerialized
    {
        public MapElementTransform Transform;
        public Texture2D Texture;
        [Range(0, 100)]
        public int Order;
    }

    [Serializable]
    public class MapDetailsSerializedData
    {
        public MapElementTransform Transform;
        [Range(0,100)]
        public int Order;
        public ShapeData Shape;
        public ShapeCreationProcess CreationProcess;
    }

    [Serializable]
    public class MapRoomSerializedData
    {
        public string Name;
        public string UniqueId;
        public MapElementTransform Transform;
        public ShapeData[] Shapes;
        public List<string> LinkedElements;
        public MapDetailsSerializedData[] Details;
        public MapImageSerialized[] Images;

        public MapRoomState GetState()
        {
             if (ObjectStateManager.Instance.GetState(UniqueId, out ObjectStateSaveDataEntry savedObjectState))
             {
                string componentData = savedObjectState.GetComponentData<MapRoom>();
                if (!string.IsNullOrEmpty(componentData))
                    return Enum.Parse<MapRoomState>(componentData);
             }

            return MapRoomState.Unknown;
        }

    }

    [Serializable]
    public class MapDoorSerializedData
    {
        public string Name;
        public string UniqueId;
        public MapElementTransform Transform;
        public Vector2 Size;

        public MapDoorState GetState()
        {
            if (ObjectStateManager.Instance.GetState(UniqueId, out ObjectStateSaveDataEntry savedObjectState))
            {
                string componentData = savedObjectState.GetComponentData<MapDoor>();
                if (!string.IsNullOrEmpty(componentData))
                    return Enum.Parse<MapDoorState>(componentData);
            }

            return MapDoorState.Unknown;
        }

    }

    [CreateAssetMenu(menuName = "Horror Engine/Mapping/MapData")]
    public class MapData : Register
    {
        public string ControllerUniqueId;
        public string Name = "NoName";
        public string Abbreviation = "";
        public Vector2Int Size = new Vector2Int(25, 25);
        public int CellSize = 50;
        public float GlobalScale = 10f;
        public MapDataSet MapSet;
        public List<MapRoomSerializedData> Rooms = new List<MapRoomSerializedData>();
        public List<MapDoorSerializedData> Doors = new List<MapDoorSerializedData>();
        
        public Matrix4x4 GetTRS(Vector2 offset, float rotation, Vector3 scale)
        {
            return Matrix4x4.TRS(new Vector3(offset.x, 0, -offset.y), Quaternion.Euler(0, -rotation, 0), GlobalScale * scale);
        }

        public bool GetVisited()
        {
            if (ObjectStateManager.Instance.GetState(ControllerUniqueId, out ObjectStateSaveDataEntry savedObjectState))
            {
                string componentData = savedObjectState.GetComponentData<MapController>();
                if (!string.IsNullOrEmpty(componentData))
                    return Convert.ToBoolean(componentData);
            }

            return false;
        }

        public bool IsKnownByPlayer()
        {
            return GetVisited() || GameManager.Instance.Inventory.Maps.Contains(this);
        }

    }
}