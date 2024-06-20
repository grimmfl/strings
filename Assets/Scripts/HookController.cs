using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

public class HookController : NetworkBehaviour
{
    private const float MaxLength = 10;
    private const string HookPrefabNamePrefix = "hook-";
    
    public GameObject hookPrefab;

    private Camera _camera;
    private LayerMask _groundLayer;
    private LayerMask _wallLayer;
    private LayerMask _playerLayer;
    private IDictionary<ulong, Player> _players;

    private bool _isEnabled;
    private Vector3 _lastPos;
    private float _length;
    private IDictionary<long, GameObject> _partDict = new Dictionary<long, GameObject>();
    
    void Start()
    {
        _camera = Camera.main;
        _groundLayer = LayerMask.GetMask("Ground");
        _wallLayer = LayerMask.GetMask("Wall");
        _playerLayer = LayerMask.GetMask("Player");

        LoadPlayers();
        NetworkManager.Singleton.OnConnectionEvent += (manager, data) => LoadPlayers();
    }

    void LoadPlayers()
    {
        _players = GameObject.FindGameObjectsWithTag("Player")
            .Select(g => g.GetComponent<Player>())
            .ToDictionary(p => p.OwnerClientId);
    }
    
    void Update()
    {
        if (!IsOwner) return;
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            Toggle();
        }

        if (!_isEnabled) return;

        if (!Input.GetMouseButtonDown(0)) return;

        var ray = _camera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, layerMask: _groundLayer)) return;

        SpawnHookPart(hit.point);
    }

    private void Toggle()
    {
        if (_isEnabled)
        {
            _isEnabled = false;
            _length = 0;

            DespawnPartsServerRpc(_partDict.Keys.ToArray());
            
            _partDict.Clear();
            
            _players[OwnerClientId].Unblock();
        }
        else
        {
            _isEnabled = true;
            _lastPos = transform.position;
            
            _players[OwnerClientId].Block();
        }
    }

    private void SpawnHookPart(Vector3 pos)
    {
        if (!_isEnabled) return;

        var delta = pos - _lastPos;
        delta.y = 0;

        var newLength = _length + delta.magnitude;

        if (newLength > MaxLength) return;
        
        if (IsThroughWall(_lastPos, delta)) return;

        // spawn point of the new part (in the middle)
        var spawnPoint = _lastPos + delta * 0.5f;
        // y stays the same
        spawnPoint.y = _lastPos.y;
        
        // calc angle
        var alpha = Vector3.SignedAngle(Vector3.right, delta, Vector3.up) + 90;

        // build quaternion and instance
        var quaternion = Quaternion.AngleAxis(alpha, Vector3.up);

        InstantiateHookPartServerRpc(spawnPoint, quaternion, delta.magnitude, OwnerClientId);
        
        FindHits();
        
        _lastPos = pos;
        _length = newLength;
    }

    private bool IsThroughWall(Vector3 origin, Vector3 direction)
    {
        return Physics.Raycast(origin, direction, out _, direction.magnitude, _wallLayer);
    }

    [ServerRpc]
    private void InstantiateHookPartServerRpc(Vector3 spawnPoint, Quaternion quaternion, float size, ulong clientId)
    {
        var instance = Instantiate(hookPrefab, spawnPoint, quaternion);

        // calc scale
        instance.transform.localScale = new Vector3(instance.transform.localScale.x, instance.transform.localScale.y,
            size);

        var id = IdGenerator.Generate();
        instance.name = $"{HookPrefabNamePrefix}{id}";
        
        instance.GetComponent<NetworkObject>().Spawn();

        AddHookPartClientRpc(id, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new [] { clientId }
            }
        });
    }

    [ServerRpc]
    private void DespawnPartsServerRpc(long[] ids)
    {
        foreach (var id in ids)
        {
            var part = GameObject.Find($"{HookPrefabNamePrefix}{id}");
            part.GetComponent<NetworkObject>().Despawn();
            Destroy(part);
        }
    }

    [ClientRpc]
    private void AddHookPartClientRpc(long partId, ClientRpcParams clientRpcParams = default)
    {
        var instance = GameObject.Find($"{HookPrefabNamePrefix}{partId}");

        _partDict[partId] = instance;
    }

    private void FindHits()
    {
        var ray = _camera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, layerMask: _playerLayer)) return;
        
        HitPlayerServerRpc(hit.transform.GetComponent<Player>().OwnerClientId);
        
        Toggle();
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