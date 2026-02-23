
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class s_moveRobot : MonoBehaviour
{
    public List<GameObject> grabableObjects;
    public GameObject iKMover;
    public GameObject prefab;

    public float reachableDistance;
    public float reachableHight;

    public List<string> grabableTags;

    public s_ChipSpawner chipSpawner;
    private Material lockedMaterial;

    public s_Board s_Board;

    private void Start()
    {
        findGrabableObjects();
    }

    private void findGrabableObjects()
    {
        grabableObjects.Clear();
        foreach (string tag in grabableTags)
        {
            GameObject[] foundWithTag = GameObject.FindGameObjectsWithTag(tag);
            grabableObjects.AddRange(foundWithTag);
        }
        Debug.Log($"<color=lime>[s_moveRobot] Initialisierung: {grabableObjects.Count} greifbare Objekte gefunden.</color>");
    }

    public void moveObjectToRandomPosition()
    {
        float minRadius = 0.5f;
        float maxRadius = 2.0f;

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float distance = Random.Range(minRadius, maxRadius);

        Vector3 finalPos = new Vector3(randomDir.x * distance, Random.Range(0f, 0.5f), randomDir.y * distance);
        Debug.Log($"<color=lime>[s_moveRobot] Zufällige Bewegung zu {finalPos} angefordert.</color>");
        moveObjectToPosition(finalPos, null);
    }

    public void moveObjectToPosition(Vector3 goalPos, Material teamMat)
    {
        lockedMaterial = teamMat;
        GameObject selectedObject = selectAObject();

        if (selectedObject == null)
        {
            Debug.LogError("<color=red>[s_moveRobot] FEHLER: Kein erreichbares Objekt gefunden!</color>");
            return;
        }

        Debug.Log($"<color=lime>[s_moveRobot] Starte Sequenz: Objekt '{selectedObject.name}' -> Ziel {goalPos}</color>");
        startRobotSequence(selectedObject, goalPos);
    }

    private bool isReachable(GameObject testObject)
    {
        if (testObject == null) return false;

        float dist = Vector3.Distance(gameObject.transform.position, testObject.transform.position);
        bool reachable = dist <= reachableDistance;

        if (!reachable)
            Debug.Log($"<color=orange>[s_moveRobot] Objekt {testObject.name} zu weit weg ({dist:F2}m > {reachableDistance}m).</color>");

        return reachable;
    }

    public GameObject selectAObject()
    {
        if (grabableObjects.Count == 0)
        {
            // Falls Liste leer, versuche sie einmalig neu zu füllen
            findGrabableObjects();
            if (grabableObjects.Count == 0) return null;
        }

        GameObject grabableObject = null;
        while (grabableObjects.Count > 0)
        {
            int randomIndex = Random.Range(0, grabableObjects.Count);
            grabableObject = grabableObjects[randomIndex];
            grabableObjects.RemoveAt(randomIndex);

            if (isReachable(grabableObject))
            {
                Debug.Log($"<color=lime>[s_moveRobot] Objekt ausgewählt: {grabableObject.name}. Verbleibende Reserve: {grabableObjects.Count}</color>");
                return grabableObject;
            }
        }

        return null;
    }

    public void startRobotSequence(GameObject movingObject, Vector3 endPos)
    {
        Vector3 startPos = movingObject.transform.position;
        Vector3 ikMoverOriginalPos = iKMover.transform.position;

        // Pfad-Berechnungen
        Vector3[] pathArrayToStart = createPath(ikMoverOriginalPos, startPos);
        Vector3[] pathArrayToEnd = createPath(startPos, endPos);
        Vector3[] pathArrayToOriginalPos = createPath(endPos, ikMoverOriginalPos);

        Sequence mySequence = DOTween.Sequence();

        // 1. Zum Objekt fahren
        mySequence.Append(iKMover.transform.DOPath(pathArrayToStart, 2f, PathType.CatmullRom)
            .SetEase(Ease.InOutSine)
            .OnStart(() => Debug.Log("<color=lime>[s_moveRobot] Phase 1: Fahre zum Chip...</color>")));

        // 2. Greifen
        mySequence.AppendCallback(() => {
            Debug.Log("<color=lime>[s_moveRobot] Phase 2: Greife Chip.</color>");
            grapObject(movingObject);
        });

        // 3. Zum Ziel fahren
        mySequence.Append(iKMover.transform.DOPath(pathArrayToEnd, 2f, PathType.CatmullRom)
            .SetEase(Ease.InOutSine)
            .OnStart(() => Debug.Log("<color=lime>[s_moveRobot] Phase 3: Transportiere Chip zum Board...</color>"))
            .OnComplete(() => {
                Debug.Log("<color=lime>[s_moveRobot] Phase 4: Ziel erreicht. Lasse los und fahre zurück.</color>");
                DropAndReturn(movingObject, pathArrayToOriginalPos);
            }));
    }

    private Vector3[] createPath(Vector3 startPos, Vector3 endPos)
    {
        Vector3 midPoint = (startPos + endPos) * 0.5f;
        midPoint.y = Mathf.Clamp(midPoint.y + 0.3f, 0f, reachableHight);

        Vector3 circularMid = getCircularMidPoint(startPos, endPos, gameObject.transform.position, midPoint.y);
        return new Vector3[] { circularMid, endPos };
    }

    private Vector3 getCircularMidPoint(Vector3 startPos, Vector3 endPos, Vector3 centerPos, float liftHeight)
    {
        Vector3 dirStart = startPos - centerPos;
        Vector3 dirEnd = endPos - centerPos;
        dirStart.y = 0;
        dirEnd.y = 0;

        Vector3 dirMid = (dirStart.normalized + dirEnd.normalized).normalized;

        if (dirMid == Vector3.zero)
            dirMid = Vector3.Cross(endPos - startPos, Vector3.up).normalized;

        float radius = (dirStart.magnitude + dirEnd.magnitude) * 0.5f;
        Vector3 midPoint = centerPos + (dirMid * radius);
        midPoint.y = liftHeight;

        return midPoint;
    }

    private void DropAndReturn(GameObject heldObject, Vector3[] pathBack)
    {
        GameObject clone = releaseObject(heldObject);

        if (s_Board != null)
        {
            s_Board.InsertChip(clone);
        }
        else
        {
            Debug.LogError("<color=red>[s_moveRobot] KRITISCH: s_Board Referenz fehlt!</color>");
        }

        iKMover.transform.DOPath(pathBack, 2f, PathType.CatmullRom).SetEase(Ease.InOutSine);
    }

    private void grapObject(GameObject grabedObject)
    {
        Rigidbody rb = grabedObject.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        Collider col = grabedObject.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        grabedObject.transform.SetParent(iKMover.transform, true);
    }

    private GameObject releaseObject(GameObject releasedObject)
    {
        int playerID = s_Board.GetActivePlayer() ? 1 : 2;
        GameObject clone = chipSpawner.SpawnChip(playerID, releasedObject.transform.position, Quaternion.Euler(0f, 0f, 90f));

        if (lockedMaterial != null)
        {
            Renderer chipRend = clone.GetComponentInChildren<Renderer>();
            if (chipRend != null) chipRend.material = lockedMaterial;
        }

        clone.transform.localScale = releasedObject.transform.lossyScale;

        // Wichtig: Der Chip muss für den Fall im Board kurzzeitig ohne Physik sein, 
        // bis s_Board ihn übernimmt
        clone.GetComponent<Rigidbody>().isKinematic = true;

        Collider col = clone.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Destroy(releasedObject);
        Debug.Log("<color=lime>[s_moveRobot] Chip am Ziel ausgetauscht und freigegeben.</color>");
        return clone;
    }
}