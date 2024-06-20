using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class NetwotkButtonUi : MonoBehaviour
{
    private VisualElement _root;
    private Button _hostButton;
    private Button _clientButton;

    private NetworkManager _networkManager;

    void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _hostButton = _root.Q<Button>("HostButton");
        _clientButton = _root.Q<Button>("ClientButton");

        _networkManager = NetworkManager.Singleton;
        
        _hostButton.clicked += () =>
        {
            _networkManager.StartHost();
        };
        _clientButton.clicked += () => _networkManager.StartClient();
    }
}
