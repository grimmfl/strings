using Ui;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public const string GameObjectPrefix = "Player-";
    private const float Speed = 2.5f;

    public GameObject bulletPrefab;
    
    private Rigidbody _rigidbody;
    private HudUiController _hudUiController;
    private Camera _camera;
    
    private int _health = 100;
    private bool _isBlocked = false;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _hudUiController = GameObject.Find("hud").GetComponent<HudUiController>();
        _camera = Camera.main;
    }

    void Update()
    {
        if (!IsOwner) return;

        if (_isBlocked) return;
        
        // get move input
        var direction = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) direction.z += 1;
        if (Input.GetKey(KeyCode.S)) direction.z -= 1;
        if (Input.GetKey(KeyCode.A)) direction.x -= 1;
        if (Input.GetKey(KeyCode.D)) direction.x += 1;
        
        MoveServerRpc(direction);
        
        // get shoot input
        if (!Input.GetMouseButtonDown(0)) return;

        var ray = _camera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out var hit, Mathf.Infinity)) return;

        var pos = transform.position;
        pos.y = 0;
        var target = hit.point;
        target.y = 0;
        SpawnBulletServerRpc(pos, target);
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

    [ServerRpc]
    private void SpawnBulletServerRpc(Vector3 position, Vector3 target)
    {
        var direction = (target - transform.position).normalized;

        var spawn = position + 0.4f * direction;

        var prefab = Instantiate(bulletPrefab, spawn, new Quaternion());
        prefab.GetComponent<Bullet>().direction = direction;
        prefab.GetComponent<NetworkObject>().Spawn();
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
