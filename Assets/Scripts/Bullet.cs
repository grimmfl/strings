using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Vector3 direction;
    public float speed = 0.005f;
    
    private IDictionary<ulong, Player> _players;
    
    // Start is called before the first frame update
    void Start()
    {
        LoadPlayers();
    }
    
    void LoadPlayers()
    {
        _players = GameObject.FindGameObjectsWithTag("Player")
            .Select(g => g.GetComponent<Player>())
            .ToDictionary(p => p.OwnerClientId);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(direction);
        Debug.Log(speed);
        transform.position += direction * speed;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            GetComponent<NetworkObject>().Despawn();
            Destroy(gameObject);
        }
        
        if (other.gameObject.CompareTag("Player"))
        {
            GetComponent<NetworkObject>().Despawn();
            Destroy(gameObject);
            
            HitPlayerServerRpc(other.gameObject.GetComponent<Player>().OwnerClientId);
        }
    }
    
    [ServerRpc]
    private void HitPlayerServerRpc(ulong clientId)
    {
        _players[clientId].DoDamageClientRpc(10, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new [] { clientId }
            }
        });
    }
}
