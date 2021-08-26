using System;
using System.IO;
using UnityEngine;
using RenderHeads.Media.AVProDeckLink;

public class ARVirtualStudioManager : MonoBehaviour
{
    [SerializeField]
    private GameObject ZedCameraFrame;
    [SerializeField]
    private DeckLinkOutput DeckOutput;

    public DeckLinkOutput deckOutput
    {
        get { return DeckOutput; }
    }

    private GameObject loadedObject;

    enum StateNumber
    {
        PlaneAnchor,
        MultiplePlane,
        VisibleInScene,
        VisibleInGame,
        AddObjectDrop
    }


    private static ARVirtualStudioManager _instance;
    public static ARVirtualStudioManager Instance
    {
        get
        {
            if(!_instance)
            {
                _instance = FindObjectOfType(typeof(ARVirtualStudioManager)) as ARVirtualStudioManager;

                if(null == _instance)
                    Debug.Log("No Sigleton obj");
            }
            return _instance;
        }
    }

    void Awake()
    {
        if(null == _instance)
        {
            _instance = this;
        }
        else if(this != _instance)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    ///////
    /// UI On/Off
    ////////
    public void ToggleZedCamera()
    {
        if(null != ZedCameraFrame)
            ZedCameraFrame.SetActive(!ZedCameraFrame.activeSelf);
    }


    //////////

    // public void DetectFloorPlane()
    // {
    //     PlaneDetect.DetectFloorPlane(false);
    // }

    // public void PlaneDetectionVisibleInGame(bool flag)
    // {
    //     PlaneDetect.planesVisibleInGame = flag;
    // }

    // public void TogglePlaneDetectionVisibleInGame()
    // {
    //     PlaneDetect.planesVisibleInGame = !PlaneDetect.planesVisibleInGame;
    // }

    // public void PlaneDetectionVisibleInScene(bool flag)
    // {
    //     PlaneDetect.planesVisibleInScene = flag;
    // }

    // public void TogglePlaneDetectionVisibleInScene()
    // {
    //     PlaneDetect.planesVisibleInScene = !PlaneDetect.planesVisibleInScene;
    // }

    // public bool StateAddObjectDrop()
    // {
    //     return addPlaneObjectAnchor.GetComponent<AddObjectPlaneAnchor>().isObjectDrop;
    // }
}
