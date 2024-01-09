using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using System.Threading;
public class NetworkRunnerHandler : MonoBehaviour
{
    public NetworkRunner networkRunnerPrefab;
    NetworkRunner networkRunner;
    void Start()
    {
     networkRunner = Instantiate(networkRunnerPrefab);
        networkRunner.name = "Network runner";
    }

   
}
