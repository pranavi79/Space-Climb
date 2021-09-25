using UnityEngine;
using MLAPI;

public class MoveCamera : NetworkBehaviour {

    public Transform player;

    void Update() {
        transform.position = player.transform.position;
    }
}
