using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace HorrorEngine
{
    [Serializable]
    public class SocketAttachment
    {
        [FormerlySerializedAs("Handle")]
        public SocketHandle Socket;
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale = Vector3.one;
    }

    public class SocketController : MonoBehaviour
    {
        private Socket[] m_Sockets;
        private Dictionary<SocketHandle, Socket> m_HashedSockets = new Dictionary<SocketHandle, Socket>();

        // --------------------------------------------------------------------

        private void Awake()
        {
            Init();
        }

        // --------------------------------------------------------------------

        public Socket GetSocket(SocketHandle handle)
        {
            if (m_Sockets == null || m_HashedSockets.Count == 0)
                Init();

            Debug.Assert(m_HashedSockets.ContainsKey(handle), "SocketController doesn't contain handle : " + handle.name);

            return m_HashedSockets[handle];
        }

        // --------------------------------------------------------------------

        public void Attach(GameObject go, SocketAttachment attachment)
        {
            var socket = GetSocket(attachment.Socket);
            go.transform.SetParent(socket.transform);
            go.transform.localPosition = attachment.Position;
            go.transform.localRotation = Quaternion.Euler(attachment.Rotation);
            go.transform.localScale = attachment.Scale;
        }

        // --------------------------------------------------------------------

        public void Init()
        {
            m_Sockets = GetComponentsInChildren<Socket>();
            foreach (var socket in m_Sockets)
            {
                Debug.Assert(!m_HashedSockets.ContainsKey(socket.Handle), "Duplicated socket handle");
                m_HashedSockets.Add(socket.Handle, socket);
            }
        }
    }
}