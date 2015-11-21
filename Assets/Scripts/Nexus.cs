using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class Nexus : NetworkBehaviour {

    public GameObject CharacterPrefab;

    public GameObject middleSpawn;
    public GameObject topSpawn;
    public GameObject bottomSpawn;

    public GameObject blueNexus;
    public GameObject greenNexus;

    public int charactersSpawned = 0;

    private NetworkIdentity identity;

    private Text resourceText;
    private int localResources = 15;

    public bool isLeft = true;

    [SyncVar]
    public int Health = 25;
    [SyncVar]
    public int resources = 15;

    public int MaxHealth = 100;

	// Use this for initialization
	void Start () {
        identity = GetComponent<NetworkIdentity>();

        if (transform.position.x > 0)
        {
            isLeft = false;
            blueNexus.SetActive(true);
            greenNexus.SetActive(false);
            resourceText = GameObject.Find("RightResources").GetComponent<Text>();
            //Debug.Log("Is left");
        }
        else
        {
            this.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
            blueNexus.SetActive(false);
            greenNexus.SetActive(true);
            resourceText = GameObject.Find("LeftResources").GetComponent<Text>();
            //Debug.Log("is right");
        }

        RpcModifyResources(0);
	}

    
	
	// Update is called once per frame
	void Update () {
        if (!identity.isLocalPlayer)
        {
            return;
        }

        if (this.resources >= 5)
        {
            if (Input.GetButtonDown("SpawnMiddle"))
            {
                CmdDoSpawnMiddle();
                //Debug.Log("Fire! " + isServer);
            }
            else if (Input.GetButtonDown("SpawnTop"))
            {
                if (isLeft)
                {
                    CmdDoSpawnBottom();
                }
                else
                {
                    CmdDoSpawnTop();
                }
            }
            else if (Input.GetButtonDown("SpawnBottom"))
            {
                if (isLeft)
                {
                    CmdDoSpawnTop();
                }
                else
                {
                    CmdDoSpawnBottom();
                }
            }
        }
	}

    [Command]
    void CmdDoSpawnMiddle()
    {
        GameObject character = (GameObject)Instantiate(CharacterPrefab, middleSpawn.transform.position ,Quaternion.identity);
        Character charScript = character.GetComponent<Character>();
        charScript.isLeft = this.isLeft;
        charScript.myNexus = this;
        NetworkServer.Spawn(character);
        this.charactersSpawned++;
        ModifyResources(-5);
    }

    [Command]
    void CmdDoSpawnTop()
    {
        GameObject character = (GameObject)Instantiate(CharacterPrefab, topSpawn.transform.position, Quaternion.identity);
        Character charScript = character.GetComponent<Character>();
        charScript.isLeft = this.isLeft;
        charScript.myNexus = this;
        NetworkServer.Spawn(character);
        this.charactersSpawned++;
        ModifyResources(-5);
    }

    [Command]
    void CmdDoSpawnBottom()
    {
        GameObject character = (GameObject)Instantiate(CharacterPrefab, bottomSpawn.transform.position, Quaternion.identity);
        Character charScript = character.GetComponent<Character>();
        charScript.isLeft = this.isLeft;
        charScript.myNexus = this;
        NetworkServer.Spawn(character);
        this.charactersSpawned++;
        ModifyResources(-5);
    }

    [ClientRpc]
    void RpcTakeDamage(int amount)
    {
        //visual stuff
    }

    public void TakeDamage(int amount)
    {
        if (!isServer)
        {
            return;
        }

        this.Health -= amount;
        this.resources += 3;

        if (this.Health <= 0)
        {
            NetworkServer.Destroy(this.gameObject);
        }
    }

    [ClientRpc]
    void RpcModifyResources(int amount)
    {
        if (identity.isLocalPlayer)
        {
            localResources += amount;
            if (localResources < 0)
            {
                localResources = 0;
            }
            resourceText.text = this.localResources.ToString();
        }
        //Debug.Log("Client side text update");
    }

    public void ModifyResources(int amount)
    {
        if (!isServer)
        {
            return;
        }

        this.resources += amount;
        if (this.resources < 0)
        {
            resources = 0;
        }

        RpcModifyResources(amount);
    }
}
