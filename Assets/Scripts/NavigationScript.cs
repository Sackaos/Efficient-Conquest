using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavigationScript : MonoBehaviour
{
    public Transform player;
    private NavMeshAgent agent;
    void Start()
    {
        agent=GetComponent<NavMeshAgent>(); 
    }
    void Update()
    {
        agent.destination=player.position;
        
    }
}
