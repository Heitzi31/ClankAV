using Photon.Pun;
using UnityEngine;

public class NetworkSync : MonoBehaviourPun, IPunObservable
{
    private Vector3 latestPos;
    private Quaternion latestRot;
    void Start()
    {
        Transform parent = GameObject.Find("NetworkedObjectsRoot")?.transform;
        if (parent != null)
            transform.SetParent(parent, true);
    }


    void Update()
    {
        if (!photonView.IsMine)
        {
            transform.position = Vector3.Lerp(transform.position, latestPos, Time.deltaTime * 10);
            transform.rotation = Quaternion.Lerp(transform.rotation, latestRot, Time.deltaTime * 10);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            latestPos = (Vector3)stream.ReceiveNext();
            latestRot = (Quaternion)stream.ReceiveNext();
        }
    }
}
