using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AICharacterScript : MonoBehaviour
{

    public GameObject startEndPanel;

    public List<GameObject> hookPoints = new List<GameObject>();
    public Animator thisAnim;
    public float speed;
    public bool gameStart;
    public Rigidbody thisRb;
    public float jumpForce;

    public Transform hookPoint;

    public RopeScript thisRope;

    bool isSwingin;
    bool isGrounded;

    public int currentHookPoint;

    public bool curvedPoints;
    public float yAngle;
    public bool isGrabable;

    public GameObject hitEffect;

    public Text airTime;
    public float inAirScore;
    public int multiplier;
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
            swingMultiplier.text = "Swing X" + multiplier.ToString("F0");
        }
        else
        {
            swingMultiplier.gameObject.SetActive(false);
        }
        if (currentHookPoint == 3 || currentHookPoint == 4)
        {
            curvedPoints = true;
        }
        else
        {
            curvedPoints = false;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            gameStart = true;
        }


        if (!isGrounded)
        {
            inAirScore += Time.deltaTime;
            airTime.text = "Total Air Time : " + (multiplier * inAirScore).ToString("F2") + " seconds";
        }


    }
    // Update is called once per frame
    void FixedUpdate()
    {
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
        if (collision.gameObject.tag == "ground")
        {
            multiplier = 0;
            isGrounded = true;
            StartCoroutine(SetAngle());
            Instantiate(hitEffect, transform.position, transform.rotation);
        }
        if (collision.gameObject.tag == "end")
        {
            GameObject parentObj = collision.gameObject.transform.parent.gameObject;
            for (int i = 0; i <= parentObj.transform.childCount - 1; i++)
            {
                parentObj.transform.GetChild(i).GetComponent<Rigidbody>().isKinematic = false;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "ground")
        {
            isGrounded = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "turn")
        {
            yAngle = 215;
            Transform targetPoint = other.GetComponent<ColliderTurnReference>().targetTurn.transform;
            Vector3 targetDirection = other.GetComponent<ColliderTurnReference>().targetTurn.position - transform.position;
            float singleStep = 1 * Time.deltaTime;
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
            transform.localRotation = Quaternion.LookRotation(newDirection);
            transform.RotateAround(other.GetComponent<ColliderTurnReference>().rotateReference.position, Vector3.up, 65 * Time.deltaTime / 2);
        }
        if (other.gameObject.tag == "turn2")
        {
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
        if (other.gameObject.tag == "turn")
        {
            //thisRb.AddForce(Vector3.back * jumpForce * 2);
        }
        if (other.gameObject.tag == "turn2")
        {
            thisRb.AddForce(Vector3.up * jumpForce * 1.75f);
            thisRb.AddForce(Vector3.left * jumpForce);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("hookstart"))
        {
            isGrabable = true;
            hookPoints[currentHookPoint].GetComponent<MeshRenderer>().material.color = Color.green;

            if (!isSwingin)
            {
                multiplier += 1;
                if (isGrabable)
                {
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
        if (other.CompareTag("hookend"))
        {
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
        if (other.CompareTag("fail"))
        {
            Time.timeScale = 0;
        }
        if (other.CompareTag("finish"))
        {
            Time.timeScale = 0;
            gameStart = false;
            startEndPanel.SetActive(true);
        }
    }

    IEnumerator SetAngle()
    {
        if (transform.rotation.y != yAngle)
        {
            transform.eulerAngles = Vector3.MoveTowards(transform.eulerAngles, new Vector3(0, yAngle, 0), Time.deltaTime * speed);
            yield return new WaitForEndOfFrame();
            StartCoroutine(SetAngle());
        }
        else
        {
            StopCoroutine(SetAngle());
        }
    }

    public void StartButton() {
        gameStart = true;
    }

    public void RestartButton() {
        Application.LoadLevel(Application.loadedLevel);
    }
}
