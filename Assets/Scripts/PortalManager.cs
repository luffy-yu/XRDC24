using UnityEngine;
using System.Collections.Generic;

public struct PortalSet
{
    public GameObject portal;
    public Camera camera;
    public RenderTexture rt;

    public PortalSet(GameObject _portal, Camera _camera, RenderTexture _rt)
    {
        portal = _portal;
        camera = _camera;
        rt = _rt;
    }
}

public class PortalManager : MonoBehaviour
{
    [SerializeField] Camera m_CenterEyeCamera;
    public GameObject m_PortalPrefab;
    public GameObject m_RenderCamPrefab;

    private List<PortalSet> portals = new List<PortalSet>();
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SpawnPortal(Vector3 pos, Quaternion rot)
    {

    }



}
