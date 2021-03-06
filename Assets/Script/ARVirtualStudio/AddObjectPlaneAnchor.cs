using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AddObjectPlaneAnchor : MonoBehaviour
{
    public ZEDPlaneDetectionManager zedPlane;
    public GameObject targetObject;
    public GameObject targetPrefab;
    
    private Transform placeholder;
    private GameObject currentObject;
    private bool canspawnObject;
    [SerializeField]
    private Camera cam;
    public bool bCheckBigSize;
    public float minRatio = 3.0f;

    public Renderer[] rendererArray;

    public bool isObjectDrop = false;
 
    private static AddObjectPlaneAnchor _instance;
    public static AddObjectPlaneAnchor Instance
    {
        get
        {
            if(!_instance)
            {
                _instance = FindObjectOfType(typeof(AddObjectPlaneAnchor)) as AddObjectPlaneAnchor;

                if(null == _instance)
                    Debug.Log("no singleton obj");
            }
            return _instance;
        }
    }

    void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
        }

        else if(_instance != this)
        {
            Destroy(gameObject);
        }

        // DontDestroyOnLoad(gameObject);

        bCheckBigSize = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(isObjectDrop == true)
            AddObjectDrop();
    }

    public void AddObjectDrop()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Vector2 screenpos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            Vector3 worldpos;
            if (ZEDSupportFunctions.GetWorldPositionAtPixel(ZEDManager.GetInstance(sl.ZED_CAMERA_ID.CAMERA_ID_01).zedCamera, screenpos, 
                ZEDManager.GetInstance(sl.ZED_CAMERA_ID.CAMERA_ID_01).GetLeftCameraTransform().GetComponent<Camera>(), out worldpos))
            {
                if(targetObject != null && targetObject.GetComponent<Rigidbody>() != null)
                {
                    targetObject.SetActive(true);
                    targetObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    targetObject.transform.position = worldpos + Vector3.up;

                    Debug.Log("Place an Object. ");
                }
                else
                {
                    Debug.Log("No Obecjt or No Rigidboy. Please Check");
                }
            }
        }
    }

    public void AddObjectPlace()
    {
        //ZED Plane Detection Manager??? ?????? plane ????????? ???????????????. 
        if(zedPlane.hitPlaneList.Count == 0)
        {
            foreach (ZEDManager manager in ZEDManager.GetInstances()) 
            {
                if(zedPlane.DetectPlaneAtHit(manager, manager.GetMainCamera().WorldToScreenPoint(placeholder.position)))
                {
                    // plane ??? ????????? ??????.
                    ZEDPlaneGameObject currentPlane = zedPlane.hitPlaneList[zedPlane.hitPlaneList.Count - 1];
                    Vector3 planeNormal = currentPlane.worldNormal;

                    // ????????? ??????????????? ?????? ?????? ???????????? Y(?????????)??? ????????? ????????? ????????? ???????????????.
                    if(Vector3.Dot(planeNormal, Vector3.up) > 0.85f)
                    {
                        if(false == canspawnObject)
                        {
                            canspawnObject = true;
                        }
                        else
                        {
                            for(int i = 0; i < zedPlane.hitPlaneList.Count; i++)
                            {
                                if(i == 0)
                                {
                                    Destroy(zedPlane.hitPlaneList[i].gameObject);
                                    zedPlane.hitPlaneList.RemoveAt(i);
                                }
                            }
                        }
                    }
                    else // ????????? ???????????? ?????????..
                    {
                        // ??????????????? ???????????? ????????? ?????? ??????????????? ?????? ???????????? ???????????????.
                        canspawnObject = false;
                        ClearZedPlaneHitPlaneList();
                    }
                    break; // ????????? ????????? plane ??? ???????????? ????????? ???????????? ?????? ????????? ????????????.
                }
            }
        }
        else if(zedPlane.hitPlaneList.Count > 0)
        {
            /*
            if(!Physics.Raycast(transform.position, placeholder.position - transform.position))
            {
                // ??????????????? ???????????? ????????? ?????? ?????? ????????? ?????? ???????????? ???????????????.
                canspawnObject = false;
                ClearZedPlaneHitPlaneList();
            }
            */

            SpawnObject(zedPlane.hitPlaneList[0].gameObject.transform.position);
        }

        /*
        if(canspawnObject)
        {
            // ??????????????? ??????????????? ?????? ???????????? ??????????????? ???????????? ??????????????????. 
            SpawnObject(placeholder.position);
        }
        else
        {
            ClearZedPlaneHitPlaneList();
        }
        */
    }

    public void SpawnObjectPlaneAnchor()
    {
        // plane ??????????????? ????????????. ?????? plane ??? ????????? plane anchor ??? ?????????????????? ????????? ??????
        if(0 == zedPlane.hitPlaneList.Count)
        {
            if(zedPlane.getFloorPlane)
            {
                SpawnObject(zedPlane.getFloorPlane.gameObject.transform.position);
            }
            else
            {
                Debug.Log("please Set Plane Anchor");
            }
        }
        else
            SpawnObject(zedPlane.hitPlaneList[0].gameObject.transform.position);
    }

    // plane ???????????? ????????????.
    public void ClearZedPlaneHitPlaneList()
    {
        for(int i = 0; i < zedPlane.hitPlaneList.Count; i++)
        {
            Destroy(zedPlane.hitPlaneList[i].gameObject);
            zedPlane.hitPlaneList.RemoveAt(i);
        }
    }

    public void SpawnObject(Vector3 spawnPos)
    {
        GameObject newObject = Instantiate(targetPrefab, spawnPos, Quaternion.identity, null) as GameObject;
        Quaternion rot = Quaternion.LookRotation(cam.transform.position - spawnPos);
        newObject.transform.eulerAngles = new Vector3(0f, rot.eulerAngles.y, 0f);
        currentObject = newObject;
        currentObject.SetActive(true);

        if(true == bCheckBigSize)
            CheckObjectBigSize();
    }

    private void CheckObjectBigSize()
    {
        rendererArray = currentObject.GetComponentsInChildren<Renderer>();

        float bigSize = 0;
        float currentSize;

        // #### ??? ?????? ?????? ????????? ??????. ??????????????? ?????? ????????????.
        foreach(Renderer ren in rendererArray)
        {
            currentSize = GetBigSizeValue(ren.bounds.size);

            if(bigSize < currentSize)
                bigSize = currentSize;            
        }

        float planeSize;
        Renderer planeRenderer = null;

        if(0 == zedPlane.hitPlaneList.Count)
        {
            planeRenderer = zedPlane.getFloorPlane.gameObject.GetComponent<MeshRenderer>();
        }
        else
        {
            if(null != zedPlane.hitPlaneList[0])
            {
                planeRenderer = zedPlane.hitPlaneList[0].gameObject.GetComponent<MeshRenderer>();
            }
        }

        if(null != planeRenderer)
        {
            planeSize = GetBigSizeValue(planeRenderer.bounds.size);
            Debug.Log("Plane size : " + planeSize);
            AdjustObjectSize(planeSize, bigSize);
        }
        else
        {
            Debug.Log("Size correction failed because there is no plane.");
        }
    }

    public float GetBigSizeValue(Vector3 size)
    {
        float val = size.x;
        val += size.y;
        val += size.z;

        return val;
    }

    public void AdjustObjectSize(float origin, float big)
    {
        if(big > origin * minRatio)
        {
            float adjustRatio = minRatio * 0.1f;
            currentObject.transform.localScale = new Vector3(adjustRatio, adjustRatio, adjustRatio);
        }
    }

    public string GetObjectName()
    {
        return targetPrefab.name;
    }

    public float GetMinRatio()
    {
        return minRatio;
    }
    
    public bool GetStateCheckBigSize()
    {
        return bCheckBigSize;
    }

    public void SetObjectDrop(bool flag)
    {
        isObjectDrop = flag;
    }

    public void ToggleObjectDrop()
    {
        isObjectDrop = !isObjectDrop;
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(AddObjectPlaneAnchor))]
    public class AddObjectPlaneAnchorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            AddObjectPlaneAnchor addObject = (AddObjectPlaneAnchor)target;

            if(GUILayout.Button("Add Object Place"))
            {
                addObject.SpawnObjectPlaneAnchor();
            }
        }
    }

    #endif
}
