using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartUISet : MonoBehaviour
{
    [SerializeField] private Animator ImageAnim
        ;

    public void StartGame() 
    {
        ImageAnim.SetTrigger("OpenGame");
    }
}
