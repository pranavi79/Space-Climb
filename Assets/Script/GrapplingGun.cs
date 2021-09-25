using System.Collections;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public class GrapplingGun : NetworkBehaviour {

    private Vector3 grapplePoint;
    private Vector3 currentGrapplePosition;
    private SpringJoint joint;

    public Transform gunTip;
    public Transform camera;
    public Transform player;
    public LayerMask whatIsGrappleable;
    
    private float maxDistance = 100f;

    private LineRenderer lr;
    private ulong clientId;
    private bool isGrappling = false;
    private Hashtable lineRenderers = new Hashtable();
    public Material ropeMaterial;

    void Awake() {
        clientId = NetworkManager.Singleton.LocalClientId;
        lr = GetComponent<LineRenderer>();
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            StartGrapple();
        }
        else if (Input.GetMouseButtonUp(0)) {
            StopGrapple();
        }
    }

    //Called after Update
    void LateUpdate() {
        if(joint)
        {
            DrawRopeCurrentClient();
            InitiateDrawRopeServerRpc(gunTip.position, grapplePoint, clientId);
        }
    }

    /// <summary>
    /// Call whenever we want to start a grapple
    /// </summary>
    void StartGrapple() {
        RaycastHit hit;
        if (Physics.Raycast(camera.position, camera.forward, out hit, maxDistance, whatIsGrappleable)) {
            grapplePoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

            //The distance grapple will try to keep from grapple point. 
            joint.maxDistance = distanceFromPoint * 0.5f;
            joint.minDistance = distanceFromPoint * 0.25f;

            //Adjust these values to fit your game.
            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;

            lr.positionCount = 2;
            currentGrapplePosition = gunTip.position;

            isGrappling = true;
        }
    }

    /// <summary>
    /// Call whenever we want to stop a grapple
    /// </summary>
    void StopGrapple() {
        lr.positionCount = 0;
        Destroy(joint);
        InitiateDestroyRopeServerRpc(clientId);
        isGrappling = false;
    }
  
    /// <summary>
    /// Draw rope for current client
    /// </summary>
    void DrawRopeCurrentClient() {
        //If not grappling, don't draw rope
        if (!joint) return;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 8f);
        
        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, currentGrapplePosition);
    }

    /// <summary>
    ///  Draw rope for all clients except current client
    /// </summary>
    [ClientRpc]
    void DrawRopeClientRpc(Vector3 start, Vector3 end, ulong clientId)
    {
        // Do not render line if the current client is grappling
        if (isGrappling)
            return;

        GameObject lineObj;
        if (lineRenderers.ContainsKey(clientId))
        {
            Debug.Log("Line exists!");
            lineObj = (GameObject) lineRenderers[clientId];
        }
        else
        {
            Debug.Log("Drawing new line.");
            lineObj = new GameObject("line");
            lineRenderers.Add(clientId, lineObj);
            lineObj.AddComponent<LineRenderer>();
        }

        LineRenderer lr = lineObj.GetComponent<LineRenderer>();
        lr.material = ropeMaterial;
        lr.SetWidth(0.1f, 0.1f);
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

    }

    /// <summary>
    /// Remove line from all clients
    /// </summary>
    [ClientRpc]
    void DestroyRopeClientRpc(ulong clientId)
    {
        GameObject lineObj = (GameObject)lineRenderers[clientId];
        Destroy(lineObj);
        lineRenderers.Remove(clientId);
    }

    public bool IsGrappling() {
        return joint != null;
    }

    public Vector3 GetGrapplePoint() {
        return grapplePoint;
    }

    /// <summary>
    /// Request server to render rope on all clients
    /// </summary>
    [ServerRpc]
    void InitiateDrawRopeServerRpc(Vector3 start, Vector3 end, ulong clientId)
    {
        DrawRopeClientRpc(start, end, clientId);
    }

    /// <summary>
    /// Request server to remove rope from all clients
    /// </summary>
    [ServerRpc]
    void InitiateDestroyRopeServerRpc(ulong clientId)
    {
        DestroyRopeClientRpc(clientId);
    }
}
