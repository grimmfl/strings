using Unity.Netcode;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Vector3 direction;
    public float speed = 0.005f;
    
    // Start is called before the first frame update
    void Start()
    {
        
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
            
            // TODO do damage
        }
    }
}
