using UnityEngine;
using System.Collections;

public class FemaleEnemyController : MonoBehaviour {

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

    private Animator stdAnimator;

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

	private bool stdDead = false;

    public float damage = 5.0f;

    // Use this for initialization
    void Start()
    {
        stdAnimator = GetComponent<Animator>();
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
    void Update()
    {
		if (health <= 0) {
			curState = FSMState.Dead;
			nav.Stop();
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

        stdAnimator.Play("zombie_run");


        //transform.Translate(Vector3.forward * Time.deltaTime * runSpeed);
    }

    void UpdateAttackState()
    {
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist > attackRange)
        {
            curState = FSMState.Chase;
        }

        if (elapsedTime >= attackRate)
        {
            stdAnimator.Play("zombie_attack");
            elapsedTime = 0.0f;
        }

    }

    void UpdateDeadState()
	{
		if (stdDead != true) {
            GetComponent<Collider>().enabled = false;

            Destroy(this.gameObject, 4.0f);

			int chooseDeath = (int)Random.Range (0, 2);
			if (chooseDeath == 1)
				stdAnimator.Play ("zombie_dying");
			else
				stdAnimator.Play ("zombie_death");

			playerTransform.gameObject.SendMessage ("UpdateScore", 100);

			stdDead = true;
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
        GetComponent<Rigidbody>().isKinematic = true;
        stdAnimator.enabled = false;
    }

    void Resume()
    {
        curState = FSMState.Idle;
        nav.Resume();
        GetComponent<Rigidbody>().isKinematic = false;
        stdAnimator.enabled = true;
    }

    void DealDamage()
    {
        GameObject.FindGameObjectWithTag("Player").SendMessage("ApplyDamage", damage);
    }
}
