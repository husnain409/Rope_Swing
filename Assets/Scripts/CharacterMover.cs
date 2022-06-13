using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterMover : MonoBehaviour
{
    public List<GameObject> hookPoints = new List<GameObject>();    //List of all the attachable hookpoints
    public Animator thisAnim;                                       //Character Animator
    public float speed;                                             //Speed variable for the Player
    public bool gameStart;                                          
    public Rigidbody thisRb;                                        //Character's Rigidbody
    public float jumpForce;                                         //Force that will be applied once the player start to swing

    public RopeScript thisRope;                                     //Reference to rope creation script

    bool isSwingin;                                                 //Check if player is swinging
    bool isGrounded;                                                //Check if player is grounded

    public int currentHookPoint;                                    //Current attachable hookPoint

    public bool curvedPoints;                                       //Special functionality for curved path
    public float yAngle;                                            //Angle which character should set to automatically on landing
    public bool isGrabable;                                         //Check if hook is close enough for grabbing

    public GameObject hitEffect;                                    //Particle effect for when player will land

    public Text airTime;                                            
    public float inAirScore;                                        //Score float
    public int multiplier;                                          //Score multiplier float
    public Text swingMultiplier;                    


    // Start is called before the first frame update
    void Start()
    {
        multiplier = 0;
        inAirScore = 0;
        gameStart = false;
        isSwingin = false;
        isGrounded = false;
        currentHookPoint = 0;
        curvedPoints = false;
        yAngle = 180;
        isGrabable = false;
    }

    private void Update()
    {
        if (multiplier > 0)
        {
            swingMultiplier.gameObject.SetActive(true);
            swingMultiplier.text = "X" + multiplier.ToString("F0");
        }
        else {
            swingMultiplier.gameObject.SetActive(false);
        }

        //Checking if the current hookpoints are on a curved path or not
        if (currentHookPoint == 3 || currentHookPoint == 4)
        {
            curvedPoints = true;
        }
        else {
            curvedPoints = false;
        }


        //Baisc Rope grabbing and swinging
        if (Input.GetMouseButtonDown(0))
        {
            if (!isSwingin)
            {
                
                if (isGrabable)
                {
                    multiplier += 1;
                    isSwingin = true;
                    if (isGrounded)
                    {
                        thisRb.AddForce(Vector3.up * jumpForce);
                    }
                    thisAnim.SetBool("falling", false);
                    thisAnim.SetBool("grounded", false);
                    thisAnim.SetBool("jumping", true);
                    thisRope.hookPoint = hookPoints[currentHookPoint].transform;
                    thisRope.StartSwing();
                    if (!curvedPoints)
                    {
                        thisRb.AddRelativeForce(Vector3.forward * jumpForce * 2);
                    }
                }
            }
        }


        //Score Increment
        if (!isGrounded) {
            inAirScore += Time.deltaTime;
            airTime.text = (multiplier * inAirScore).ToString("F2") + " Points";
        }


    }
    // Update is called once per frame
    void FixedUpdate()
    {

        //Functionality for moving character in forward direction
        if (gameStart)
        {
            if (!isSwingin && isGrounded)
            {
                thisAnim.SetBool("falling", false);
                thisAnim.SetBool("jumping", false);
                thisAnim.SetBool("grounded", true);
                transform.position += transform.forward * Time.deltaTime * speed;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //when character will hit the ground or land
        if (collision.gameObject.tag == "ground") {
            multiplier = 0;
            isGrounded = true;
            StartCoroutine(SetAngle());
            Instantiate(hitEffect, transform.position, transform.rotation);
        }

        //When character hit the end bricks
        if (collision.gameObject.tag == "end") {
            GameObject parentObj = collision.gameObject.transform.parent.gameObject;
            for (int i = 0; i <= parentObj.transform.childCount - 1; i++) {
                parentObj.transform.GetChild(i).GetComponent<Rigidbody>().isKinematic = false;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        //When character Strat to swing or is in air
        if (collision.gameObject.tag == "ground") {
            isGrounded = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {

        //Curved hook point path handling
        if (other.tag == "turn") {
            yAngle = 215;
            Transform targetPoint = other.GetComponent<ColliderTurnReference>().targetTurn.transform;
            Vector3 targetDirection = other.GetComponent<ColliderTurnReference>().targetTurn.position - transform.position;
            float singleStep = 1 * Time.deltaTime;
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
            transform.localRotation = Quaternion.LookRotation(newDirection);
            transform.RotateAround(other.GetComponent<ColliderTurnReference>().rotateReference.position, Vector3.up, 65 * Time.deltaTime / 2);
        }
        if (other.gameObject.tag == "turn2") {
            yAngle = 270;
            Transform targetPoint = other.GetComponent<ColliderTurnReference>().targetTurn.transform;
            Vector3 targetDirection = other.GetComponent<ColliderTurnReference>().targetTurn.position - transform.position;
            float singleStep = 1 * Time.deltaTime;
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
            transform.localRotation = Quaternion.LookRotation(newDirection);
            transform.RotateAround(other.GetComponent<ColliderTurnReference>().rotateReference.position, Vector3.up, 200 * Time.deltaTime / 3);
        }
    }
    private void OnTriggerExit(Collider other)
    {

        //Curved hook point final force applying
        if (other.gameObject.tag == "turn") {
            //thisRb.AddForce(Vector3.back * jumpForce * 2);
        }
        if (other.gameObject.tag == "turn2") {
            thisRb.AddForce(Vector3.up * jumpForce * 1.75f);
            thisRb.AddForce(Vector3.left * jumpForce);
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        //Check if hook is grabable
        if (other.CompareTag("hookstart")) {
            isGrabable = true;
            hookPoints[currentHookPoint].GetComponent<MeshRenderer>().material.color = Color.green;
        }

        //Auto rope disconnection functionality
        if (other.CompareTag("hookend")) {
            isGrabable = false;
            thisRope.StopSwing();
            isSwingin = false;
            if (!isGrounded)
            {
                thisAnim.SetBool("jumping", false);
                thisAnim.SetBool("falling", true);
            }
            currentHookPoint += 1;
        }

        //If character falls down
        if (other.CompareTag("fail")) {
            UIScript.fail = true;
        }

        //Finish line
        if (other.CompareTag("finish"))
        {
            gameStart = false;
            UIScript.complete = true;
        }
    }

    //Functionality to set chartacter's angle properly once it lands on a curved platform
    IEnumerator SetAngle() {
        if (transform.rotation.y != yAngle)
        {
            transform.eulerAngles = Vector3.MoveTowards(transform.eulerAngles, new Vector3(0, yAngle, 0), Time.deltaTime * speed);
            yield return new WaitForEndOfFrame();
            StartCoroutine(SetAngle());
        }
        else {
            StopCoroutine(SetAngle());
        }
    }
}
