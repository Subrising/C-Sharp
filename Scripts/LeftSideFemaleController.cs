using UnityEngine;
using System.Collections;

public class LeftSideFemaleController : MonoBehaviour {

    public enum FSMState
    {
        None,
        Walk,
        Dance,
        Stop
    }

    public FSMState curState;

    private Animator animator;

    public GameObject[] waypointList; // List of waypoints for patrolling
    public int currentDest = -1;
    public bool setDest = false;

    private NavMeshAgent nav;
    private Vector3 PauseVelocity;

    // Use this for initialization
    void Start()
    {
        animator = GetComponent<Animator>();
        nav = GetComponent<NavMeshAgent>();
        curState = FSMState.Walk;
    }

    // Update is called once per frame
    void Update()
    {
        switch (curState)
        {
            case FSMState.Walk: UpdateWalkState(); break;
        }
    }

    void UpdateWalkState()
    {
        if (currentDest > -1)
        {
            if (Vector3.Distance(transform.position, waypointList[currentDest].gameObject.transform.position) <= 2.0f)
            {
                currentDest = (currentDest + 1) % waypointList.Length;
                setDest = false;
            }

            if (!setDest)
            {
                nav.SetDestination(waypointList[currentDest].gameObject.transform.position);
                setDest = true;
            }
        }
    }

    void Dance()
    {
        if (curState != FSMState.Dance)
        {
            GameObject objPlayer = GameObject.FindGameObjectWithTag("Player");
            Transform playerTransform = objPlayer.transform;
            playerTransform.gameObject.SendMessage("UpdateScore", (int)-25);

            nav.Stop();

            curState = FSMState.Dance;

            int animation = Random.Range(0, 2);
            switch (animation)
            {
                case 0:
                    animator.Play("hip_hop_dancing");
                    break;
                case 1:
                    animator.Play("samba_dancing");
                    break;
            }
        }
    }

    void EndDance()
    {
        if (curState == FSMState.Dance)
        {
            curState = FSMState.Walk;

            animator.Play("walking");

            nav.Resume();
        }
    }

    void Stop()
    {
        curState = FSMState.Stop;
        nav.Stop();
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        PauseVelocity = rigidbody.velocity;
        rigidbody.isKinematic = true;
        animator.enabled = false;
    }

    void Resume()
    {
        curState = FSMState.Walk;
        nav.Resume();
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = PauseVelocity;
        rigidbody.isKinematic = false;
        animator.enabled = true;
    }

}
