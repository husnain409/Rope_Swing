using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeScript : MonoBehaviour
{
    // Start is called before the first frame update
    public LineRenderer rope;
    public Transform hookPoint;         //Hook point will get reference from charctermover script
    public Transform ropeStartPoint;
    public float maxDistance;
    public Transform player;
    public SpringJoint joint;
    public Vector3 currentHookPosition;

    private void Awake()
    {
        rope = GetComponent<LineRenderer>();
    }

    void LateUpdate()
    {
        DrawRope();
    }

    public void StartSwing() {

        //Adding jounts to charcters 
        joint = player.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = hookPoint.position;

        float distanceFromPoint = Vector3.Distance(player.position, hookPoint.position);

        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;


        //Springs of the rope, Reduce the value to increase the spring
        joint.spring = 18f;
        joint.damper = 28f;
        joint.massScale = 18f;

        rope.positionCount = 2;
        currentHookPosition = ropeStartPoint.position;
    }

    public void StopSwing()
    {

        //Hiding the rope and destroying the joints
        rope.positionCount = 0;
        Destroy(joint);
    }

    void DrawRope()
    {
        //If not grappling, don't draw rope
        if (!joint) return;

        currentHookPosition = Vector3.Lerp(currentHookPosition, hookPoint.position, Time.deltaTime * 8f);

        rope.SetPosition(0, ropeStartPoint.position);
        rope.SetPosition(1, currentHookPosition);
    }


}
