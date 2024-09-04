using Cinemachine;
using UnityEngine;

namespace HorrorEngine
{
    public class CameraSystem : SingletonBehaviour<CameraSystem>
    {
        public static readonly int CamPreviewOverride = -1;

        private Cinemachine.CinemachineBrain m_Brain;
        private Camera m_MainCamera;
        
        public Cinemachine.CinemachineVirtualCamera ActiveCamera => m_Brain.ActiveVirtualCamera as CinemachineVirtualCamera;
        public Camera MainCamera => m_MainCamera;

        // --------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();

            m_Brain = GetComponentInChildren<Cinemachine.CinemachineBrain>();
            m_Brain.ReleaseCameraOverride(CamPreviewOverride);

            m_MainCamera = m_Brain.GetComponent<Camera>();
        }

        // --------------------------------------------------------------------

        void OnDestroy()
        {
            CameraStack.Instance.ClearAllCameras();
        }
        

    }
}