using UnityEngine;
using System.Collections;

public class BruteController : MonoBehaviour {

    public enum FSMState
    {
        None,
        Idle,
        Chase,
        Attack,
        Dead,
        Stop
    }

    public FSMState curState;

    private Animator bruteAnimator;

    public float chaseRange = 35.0f;
    public float attackRange = 4.0f;

    protected Transform playerTransform;// Player Transform

    public GameObject[] waypointList; // List of waypoints for patrolling

    // Attack Rate
    public float attackRate = 3.0f;
    protected float elapsedTime;

    // Whether the NPC is destroyed or not
    protected bool bDead;
    public float health = 100.0f;

    private NavMeshAgent nav;

    // current waypoint in list
    //private int curWaypoint = -1;
    //private bool setDest = false;

    public float pathCheckTime = 1.0f;
    private float elapsedPathCheckTime;

    public float rotSpeed = 3.0f;

    public float runSpeed;

	private bool bruteDead = false;

    public float damage = 10.0f;

    private Vector3 PauseVelocity;

    // Use this for initialization
    void Start () {
        bruteAnimator = GetComponent<Animator>();
        curState = FSMState.Chase;

        nav = GetComponent<NavMeshAgent>();

        bDead = false;
        elapsedTime = 0.0f;

        // Get the target enemy(Player)
        GameObject objPlayer = GameObject.FindGameObjectWithTag("Player");
        playerTransform = objPlayer.transform;

        if (!playerTransform)
            print("Player doesn't exist.. Please add one with Tag named 'Player'");
    }

    // Update is called once per frame
    void Update () {
		if (health <= 0) {
			curState = FSMState.Dead;
			nav.Stop ();
		}
		
        switch (curState)
        {
            case FSMState.Idle: UpdateIdleState(); break;
            case FSMState.Chase: UpdateChaseState(); break;
            case FSMState.Attack: UpdateAttackState(); break;
            case FSMState.Dead: UpdateDeadState(); break;
        }

        elapsedTime += Time.deltaTime;
        elapsedPathCheckTime += Time.deltaTime;
    }

    void UpdateIdleState()
    {
        /* if (Vector3.Distance(transform.position, playerTransform.position) <= chaseRange)
        {

            // see if playerTank is Line of Sight
            RaycastHit hit;
            if (Physics.Linecast(transform.position + new Vector3(0f, 1f, 0f), playerTransform.position + new Vector3(0f, 1f, 0f), out hit))
            {
                if (hit.collider.gameObject.tag == "Player")
                {
                    curState = FSMState.Chase;
                }
            }
        }
        */

        if (Vector3.Distance(transform.position, playerTransform.position) <= chaseRange)
            curState = FSMState.Chase;
    }

    void UpdateChaseState()
    {

        Quaternion targetRotation = Quaternion.LookRotation(playerTransform.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotSpeed);

        if (Vector3.Distance(transform.position, playerTransform.position) <= attackRange)
            curState = FSMState.Attack;

        if (elapsedPathCheckTime >= pathCheckTime)
        {
            nav.SetDestination(playerTransform.position);
            elapsedPathCheckTime = 0f;
        }

        bruteAnimator.Play("zombie_run");


        //transform.Translate(Vector3.forward * Time.deltaTime * runSpeed);
    }

    void UpdateAttackState()
    {
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist > attackRange)
        {
            curState = FSMState.Chase;
        }

        int chooseAttack = (int)Random.Range(0, 2);

        attackPlayer(chooseAttack);
    }

    private void attackPlayer(int pickAttack)
    {
        if (elapsedTime >= attackRate)
        {
            if (pickAttack == 0)
                bruteAnimator.Play("zombie_attack");
            if (pickAttack == 1)
                bruteAnimator.Play("zombie_headbutt");
            if (pickAttack == 2)
                bruteAnimator.Play("zombie_punching");

            elapsedTime = 0.0f;
        }
    }

    void UpdateDeadState()
    {
		if (bruteDead != true) 
		{
            GetComponent<Collider>().enabled = false;

            Destroy (this.gameObject, 4.0f);

			int chooseDeath = (int)Random.Range (0, 2);
			if (chooseDeath == 1)
				bruteAnimator.Play ("zombie_dying");
			else
				bruteAnimator.Play ("zombie_death");

			playerTransform.gameObject.SendMessage ("UpdateScore", (int)200);

			bruteDead = true;
		}
    }

	public void ApplyDamage(float damage)
	{
		if (health > 0)
			health -= damage;
	}

    void Stop()
    {
        curState = FSMState.Stop;
        nav.Stop();
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        PauseVelocity = rigidbody.velocity;
        rigidbody.isKinematic = true;
        bruteAnimator.enabled = false;
    }

    void Resume()
    {
        curState = FSMState.Idle;
        nav.Resume();
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = PauseVelocity;
        rigidbody.isKinematic = false;
        bruteAnimator.enabled = true;
    }

    void DealDamage()
    {
        GameObject.FindGameObjectWithTag("Player").SendMessage("ApplyDamage", damage);
    }
}
