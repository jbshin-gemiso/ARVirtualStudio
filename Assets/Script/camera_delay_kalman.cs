using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class camera_delay_kalman : MonoBehaviour
{
    private struct PointInSpace
    {
        public Vector3 Position;
        public Quaternion Rotaiton;
        public float Time;
    }

    [SerializeField]
    [Tooltip("The transform to follow")]
    private Transform target;

    [SerializeField]
    [Tooltip("The offset between the target and the camera")]
    private Vector3 offset;

    [Tooltip("The delay before the camera starts to follow the target")]
    [SerializeField]
    private float delay = 0.2f;

    [SerializeField]
    [Tooltip("The speed used in the lerp function when the camera follows the target")]
    private float speed = 5;

    [SerializeField]
    private ZEDManager zedManager;

    public float pos_newQ = 0.001f;
    public float pos_newR = 0.01f;
    public float rot_newQ = 0.001f;
    public float rot_newR = 0.01f;

    KalmanFilterVector3 kf;
    KalmanFilterVector4 kf_rot;

    Vector3 poss;
    Quaternion rotq;
    Vector3 kalmanPos;
    Vector4 kalmanRot;

    ///<summary>
    /// Contains the positions of the target for the last X seconds
    ///</summary>
    private Queue<PointInSpace> pointsInSpace = new Queue<PointInSpace>();
    private PointInSpace pointInSpaceItem;
    private void Start()
    {
        kf = new KalmanFilterVector3();
        kf_rot = new KalmanFilterVector4();
    }

    void FixedUpdate()
    {
        transform.position = target.position;
        transform.rotation = target.rotation;
    }
/*
    void LateUpdate()
    {
        // transform.position = target.position;
        // transform.rotation = target.rotation;

        // Add the current target position to the list of positions
        if(target != null)
        {
            pointInSpaceItem.Position = target.position;
            pointInSpaceItem.Rotaiton = target.rotation;
            pointInSpaceItem.Time = Time.time;

            pointsInSpace.Enqueue(pointInSpaceItem);

            // Move the camera to the position of the target X seconds ago 
            while (pointsInSpace.Count > 0 && pointsInSpace.Peek().Time <= Time.time - delay)
            {
                if(pointsInSpace.Count != 0)
                    poss = pointsInSpace.Dequeue().Position;
                
                if(pointsInSpace.Count != 0)
                    rotq = pointsInSpace.Dequeue().Rotaiton;

                KalmanTransform();
            }
        }
        else
        {
            Debug.Log("#### zed camera is NULL !!");
        }
    }
*/


/*
    void FixedUpdate()
    {
        // Add the current target position to the list of positions
        if(target != null)
        {
            pointInSpaceItem.Position = target.position;
            pointInSpaceItem.Rotaiton = target.rotation;
            pointInSpaceItem.Time = Time.time;

            // pointsInSpace.Enqueue(new PointInSpace() { Position = target.position, Rotaiton = target.rotation, Time = Time.time });
            pointsInSpace.Enqueue(pointInSpaceItem);

            // Move the camera to the position of the target X seconds ago 
            while (pointsInSpace.Count > 0 && pointsInSpace.Peek().Time <= Time.time - delay)
            {
                if(pointsInSpace.Count != 0)
                    poss = pointsInSpace.Dequeue().Position;
                
                if(pointsInSpace.Count != 0)
                    rotq = pointsInSpace.Dequeue().Rotaiton;
                
                //poss = Vector3.MoveTowards(transform.position, pointsInSpace.Dequeue().Position, Time.deltaTime * speed);
                //rotq = Quaternion.Slerp(transform.rotation, pointsInSpace.Dequeue().Rotaiton, Time.deltaTime * speed);

                KalmanTransform();

                //transform.position = Vector3.MoveTowards(transform.position, pointsInSpace.Dequeue().Position, Time.deltaTime * speed);
                //transform.rotation = Quaternion.Slerp(transform.rotation, pointsInSpace.Dequeue().Rotaiton, Time.deltaTime * speed);
            }
        }
        else
        {
            Debug.Log("#### zed camera is NULL !!");
        }
    }
*/

    void KalmanTransform()
    {
        kalmanPos = kf.Update(poss,pos_newQ, pos_newR);
        // kalmanRot = kf_rot.Update(new Vector4(rotq.x, rotq.y, rotq.z, rotq.w), rot_newQ, rot_newR);  // 자체 빌드시에 로테이션이 작동 안하게 됨.

        transform.position = kalmanPos;
        transform.rotation = target.rotation;
        //transform.rotation = new Quaternion(rot.x, rot.y, rot.z, rot.w);
        //transform.rotation = pointsInSpace.Dequeue().Rotaiton;
    }
}