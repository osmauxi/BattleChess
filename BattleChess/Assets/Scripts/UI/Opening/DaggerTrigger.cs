using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DaggerTrigger : MonoBehaviour
{
    private AudioSource AudioSource;

    private void Start()
    {
        AudioSource = GetComponent<AudioSource>();
    }

    public void AudioTrigger() 
    {
        AudioSource.Play();
    }
    
    public void NextStage() 
    {
        SceneManager.LoadScene(1);
    
    }

}
