using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public enum CharacterType { Solider, Mage, Hacker };

public class Character : NetworkBehaviour {

    public CharacterType type = CharacterType.Solider;
    public int lane = 1; // from top to bottom 0, 1, 2
    public bool topLane = false;

    public float Speed = 5;
    public bool isLeft;

    public Character OwnCharacter;
    public Character EnemyCharacter;

    [SyncVar]
    public int health = 10;
    [SyncVar]
    public bool isMoving = true;
    private bool attackNexus = false;

    public float damageTimeMin = 1f;
    public float damageTimeMax = 3f;
    private float damageTime = 2f;
    public float elapsedDamageTime = 0f;
    private bool canDamage = true;

    public int damagePowerMin = 1;
    public int damagePowerMax = 3;

    public Nexus myNexus;

	// Use this for initialization
	void Start () {
        // pick a random color
        //Color newColor = new Color(Random.value, Random.value, Random.value, 1.0f);

        // apply it on current object's material
        //gameObject.GetComponent<MeshRenderer>().material.color = newColor; 
        ParticleSystem exp = GetComponentInChildren<ParticleSystem>();
        exp.Stop();
	}
	
	// Update is called once per frame
	void Update () {
        if (!isServer)
        {
            return;
        }

        if (isMoving)
        {
            if (isLeft)
            {
                if (this.transform.position.x < 33f)
                {
                    this.transform.Translate(this.Speed * Time.deltaTime, 0, 0);
                }
                else
                {
                    isMoving = false;
                    attackNexus = true;
                }
            }
            else
            {
                if (this.transform.position.x > -33f)
                {
                    this.transform.Translate(-this.Speed * Time.deltaTime, 0, 0);
                }
                else
                {
                    isMoving = false;
                    attackNexus = true;
                }
            }
        }
        else if (EnemyCharacter != null || attackNexus)
        {
            if (canDamage)
            {
                int damageGoingToGive = Random.Range(this.damagePowerMin, this.damagePowerMax);
                if (!attackNexus && type == CharacterType.Solider)
                {
                    //int enemyHealth = EnemyCharacter.health;
                    EnemyCharacter.TakeDamage(damageGoingToGive);
                    if (EnemyCharacter != null)
                    {
                        Debug.Log("Enemy health " + EnemyCharacter.health);
                    }
                }
                else if (type == CharacterType.Mage)
                {
                    if (lane == 1)
                    {
                        if (topLane)
                        {
                            topLane = false;
                        }
                        else
                        {
                            topLane = true;
                        }
                    }
                }
                else 
                {
                    Nexus[] nexuses = GameObject.FindObjectsOfType<Nexus>();
                    foreach (Nexus nexus in nexuses) {
                        if (nexus.isLeft != this.isLeft)
                        {
                            nexus.TakeDamage(damageGoingToGive);
                            break;
                        }
                    }
                }
                canDamage = false;
                damageTime = Random.Range(this.damageTimeMin, this.damageTimeMax);
                elapsedDamageTime = 0f;
            }
            else
            {
                elapsedDamageTime += Time.deltaTime;
                if (elapsedDamageTime > damageTime)
                {
                    canDamage = true;
                }
            }
        }
        else if (OwnCharacter != null)
        {
            if (OwnCharacter.isMoving)
            {
                this.isMoving = true;
                this.OwnCharacter = null;
            }
        }
	}

    void OnCollisionEnter(Collision collision)
    {
        if (!isServer)
        {
            return;
        }


        if (collision.gameObject.tag == "Character")
        {
            Character collCharacter = collision.gameObject.GetComponent<Character>();
            this.isMoving = false;
            if (collCharacter.isLeft != this.isLeft)
            {
                if (EnemyCharacter == null )
                {   
                    this.EnemyCharacter = collCharacter;
                }
            }
            else if (OwnCharacter == null)
            {
                if (isLeft && collCharacter.gameObject.transform.position.x > this.gameObject.transform.position.x)
                {
                    this.OwnCharacter = collCharacter;
                }
                else if (collCharacter.gameObject.transform.position.x < this.gameObject.transform.position.x)
                {
                    this.OwnCharacter = collCharacter;
                }

            }
        }
    }


    [ClientRpc]
    void RpcDamage(int amount)
    {
        if (GetComponent<NetworkIdentity>().isClient)
        {
            ParticleSystem exp = GetComponentInChildren<ParticleSystem>();
            exp.Play();
        }
    }

    public void TakeDamage(int amount)
    {
        if (!isServer)
        {
            return;
        }

        this.health -= amount;

        if (this.health <= 0)
        {
            this.health = 0;
            this.EnemyCharacter.myNexus.ModifyResources(4);
            if (myNexus.charactersSpawned <= 1 && myNexus.resources < 5)
            {
                myNexus.ModifyResources(5);
            }
            else
            {
                myNexus.ModifyResources(3);
            }
            myNexus.charactersSpawned--;
            this.EnemyCharacter.isMoving = true;
            this.EnemyCharacter.EnemyCharacter = null;
            this.EnemyCharacter = null;
            NetworkServer.Destroy(this.gameObject);
            return;
        }

        RpcDamage(amount);
    }
}
