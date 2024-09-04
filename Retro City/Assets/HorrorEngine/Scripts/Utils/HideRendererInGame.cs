using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class HideRendererInGame : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Renderer>().enabled = false;
    }
}
