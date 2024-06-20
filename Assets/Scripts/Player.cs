using Ui;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public const string GameObjectPrefix = "Player-";
    
    private const float Speed = 2.5f;

    
    private Rigidbody _rigidbody;
    private HudUiController _hudUiController;
    
    private int _health = 100;
    private bool _isBlocked = false;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _hudUiController = GameObject.Find("hud").GetComponent<HudUiController>();
    }

    void Update()
    {
        if (!IsOwner) return;

        if (_isBlocked) return;
        
        var direction = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) direction.z += 1;
        if (Input.GetKey(KeyCode.S)) direction.z -= 1;
        if (Input.GetKey(KeyCode.A)) direction.x -= 1;
        if (Input.GetKey(KeyCode.D)) direction.x += 1;
        

        MoveServerRpc(direction);
    }

    [ServerRpc]
    private void MoveServerRpc(Vector3 direction)
    {
        Move(direction);
        //_rigidbody.MovePosition(transform.position + direction * Speed);
    }

    private void Move(Vector3 direction)
    {
        _rigidbody.velocity = direction.normalized * Speed;
    }
    
    [ClientRpc]
    public void DoDamageClientRpc(int damage, ClientRpcParams clientRpcParams = default)
    {
        _health -= damage;
        
        _hudUiController.SetHp(_health);
    }

    public void Block()
    {
        _isBlocked = true;
    }

    public void Unblock()
    {
        _isBlocked = false;
    }
}
