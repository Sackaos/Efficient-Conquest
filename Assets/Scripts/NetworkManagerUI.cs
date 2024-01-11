using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{

    [SerializeField] Button serverBtn;
    [SerializeField] Button hostBtn;
    [SerializeField] Button clientBtn;

    private void Awake()
    {
        serverBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartServer();
            serverBtn.enabled = false;
            hostBtn.enabled = false;
            clientBtn.enabled = false;
        });
        hostBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
            serverBtn.gameObject.SetActive(false);
            hostBtn.gameObject.SetActive(false);
            clientBtn.gameObject.SetActive(false);
            serverBtn.enabled = false;
            hostBtn.enabled = false;
            clientBtn.enabled = false;
        });
        clientBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
            serverBtn.gameObject.SetActive(false);
            hostBtn.gameObject.SetActive(false);
            clientBtn.gameObject.SetActive(false);
            serverBtn.enabled = false;
            hostBtn.enabled = false;
            clientBtn.enabled = false;
        });


    }
    
}
