using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleCheck : MonoBehaviour
{
    public static BattleCheck Instance;
    [SerializeField] private TextMeshProUGUI text;
    //private TurnBasedManager instance => TurnBasedManager.Instance;
    private void Awake()
    {
        // 单例模式初始化
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        text.text = new string("当前敌人数量 ： " + TurnBasedManager.Instance.enemyUnits.Count);
    }

    private void Update()
    {

    }
    public void UpdateTextCount()
    {
        text.text = new string("当前敌人数量 ： " + TurnBasedManager.Instance.enemyUnits.Count);
    }
    public void WinningCheck()
    {
        Debug.Log(TurnBasedManager.Instance.enemyUnits.Count);
        Debug.Log(TurnBasedManager.Instance.playerUnits.Count);
        text.text = new string("当前敌人数量 ： " + TurnBasedManager.Instance.enemyUnits.Count);
        if (TurnBasedManager.Instance.enemyUnits.Count == 0)
        {
            SceneManager.LoadScene(2);
            Debug.Log("Win");
        }
        else if (TurnBasedManager.Instance.playerUnits.Count == 0) 
        {
            SceneManager.LoadScene(2);
            Debug.Log("Lose");
        }  
    }
}
