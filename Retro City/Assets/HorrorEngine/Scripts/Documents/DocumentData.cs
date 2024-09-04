using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace HorrorEngine
{
    [Serializable]
    public class DocumentPage
    {
        [TextArea]
        public string Text;
        public Sprite Image;
        public string ImageCaption;
        public bool ChangeColor;
        public Color ImageColor = Color.white;
    }

    [CreateAssetMenu(menuName = "Horror Engine/Document")]
    public class DocumentData : Register
    {
        public string Name;
        public Sprite Image;
        public bool ShowImageOnRead = true;
        public bool ShowPageCount = true;
        
        [TextArea]
        [FormerlySerializedAs("PagesText")]
        [HideInInspector]
        public string[] PagesText_DEPRECATED;

        public DocumentPage[] Pages;

        [Header("Audio")]
        public AudioClip ShowClip;
        public AudioClip PageClip;
        public AudioClip CloseClip;
    }
}