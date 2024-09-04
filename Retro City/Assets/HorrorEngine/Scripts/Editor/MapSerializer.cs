using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HorrorEngine
{
    [InitializeOnLoad]
    public static class MapSerializer
    {
        static MapSerializer()
        {
            EditorSceneManager.sceneSaved -= OnSceneSaved;
            EditorSceneManager.sceneSaved += OnSceneSaved;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        // --------------------------------------------------------------------

        static void OnSceneSaved(Scene scene)
        {
            Serialize();
        }

        // --------------------------------------------------------------------

        static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
                Serialize();
        }

        // --------------------------------------------------------------------

        static void Serialize()
        {
            MapController[] maps = Object.FindObjectsOfType<MapController>();
            foreach(var map in maps)
            {
                Debug.Assert(map.Data, "Map data couldn't be serialized. No data assigned on MapController");
                SerializeAndSaveMap(map);
            }
        }

        // --------------------------------------------------------------------

        public static void SerializeAndSaveMap(MapController mapCtrl)
        {
            MapData data = mapCtrl.Data;

            data.ControllerUniqueId = mapCtrl.GetComponent<ObjectUniqueId>().Id;
            data.Rooms.Clear();
            data.Doors.Clear();

            var doors = mapCtrl.GetComponentsInChildren<MapDoor>(true);
            foreach (var door in doors)
            {
                data.Doors.Add(new MapDoorSerializedData()
                {
                    Name = door.Name,
                    UniqueId = door.GetComponent<ObjectUniqueId>().Id,
                    Size = door.Size,
                    Transform = new MapElementTransform()
                    {
                        Offset = door.Offset,
                        Rotation = door.Rotation,
                        Scale = door.Scale
                    }
                });
            }

            var rooms = mapCtrl.GetComponentsInChildren<MapRoom>(true);
            foreach (var room in rooms)
            {

                Shape[] shapes = room.GetComponents<Shape>();
                ShapeData[] shapesData = new ShapeData[shapes.Length];

                for (int i = 0; i < shapes.Length; ++i)
                {
                    shapesData[i] = new ShapeData()
                    {
                        Points = shapes[i].Points.ToArray()
                    };
                }

                List<string> linkedElements = new List<string>();
                foreach (var l in room.LinkedElements)
                {
                    if (l && l.TryGetComponent(out ObjectUniqueId objId))
                        linkedElements.Add(objId.Id);
                }

                MapDetailingShape[] detailing = room.GetComponentsInChildren<MapDetailingShape>();
                MapDetailsSerializedData[] detailingSerialized = new MapDetailsSerializedData[detailing.Length];
                for (int i =0; i < detailing.Length; ++i)
                {
                    MapDetailingShape details = detailing[i];
                    if (details.TryGetComponent(out Shape shape)) 
                    {
                        detailingSerialized[i] = new MapDetailsSerializedData()
                        {
                            Transform = new MapElementTransform()
                            {
                                Offset = details.transform.localPosition.ToXZ(),
                                Rotation = details.transform.localRotation.eulerAngles.y,
                                Scale = details.transform.localScale,
                            },
                            Shape = new ShapeData()
                            {
                                Points = shape.Points.ToArray()
                            },
                            CreationProcess = details.CreationProcess
                        };
                    }
                }

                MapImage[] images = room.GetComponentsInChildren<MapImage>();
                MapImageSerialized[] imagesSerialized = new MapImageSerialized[images.Length];
                for (int i = 0; i < images.Length; ++i)
                {
                    MapImage image = images[i];
                    imagesSerialized[i] = new MapImageSerialized()
                    {
                        Transform = new MapElementTransform()
                        {
                            Offset = image.transform.localPosition.ToXZ(),
                            Rotation = image.transform.localRotation.eulerAngles.y,
                            Scale = image.transform.localScale
                        },
                        Texture = image.Texture
                    };
                    
                }

                data.Rooms.Add(new MapRoomSerializedData()
                {
                    UniqueId = room.GetComponent<ObjectUniqueId>().Id,
                    Name = room.Name,
                    Transform = new MapElementTransform()
                    {

                        Offset = room.Offset,
                        Rotation = room.Rotation,
                        Scale = room.Scale,
                    },
                    Shapes = shapesData,
                    LinkedElements = linkedElements,
                    Details = detailingSerialized,
                    Images = imagesSerialized
                });
            }

            EditorUtility.SetDirty(data);
        }
    }
}