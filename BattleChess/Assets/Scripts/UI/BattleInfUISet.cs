using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleInfUISet : MonoBehaviour
{
    public static BattleInfUISet Instance;
    [SerializeField] private GameObject AllayP;
    [SerializeField] private GameObject EnemyP;
    [SerializeField] private GameObject SkillP;
    public bool EnemyUIVisible = false;
    public bool AllayUIVisible = false;
    [Header("友军UI")]
    [SerializeField] private TextMeshProUGUI NameText;
    [SerializeField] private TextMeshProUGUI HealthText;
    [SerializeField] private TextMeshProUGUI DamageText;
    [SerializeField] private TextMeshProUGUI EvationText;
    [SerializeField] private TextMeshProUGUI MovePInf;
    [SerializeField] private TextMeshProUGUI ManaInf;
    [Header("敌人UI")]
    [SerializeField] private TextMeshProUGUI ENameText;
    [SerializeField] private TextMeshProUGUI EHealthText;
    [SerializeField] private TextMeshProUGUI EDamageText;
    [SerializeField] private TextMeshProUGUI EEvationText;
    [SerializeField] private TextMeshProUGUI EMovePInf;
    [Header("技能UI")]
    [SerializeField] private TextMeshProUGUI SNameText;
    [SerializeField] private TextMeshProUGUI SDamageText;
    [SerializeField] private TextMeshProUGUI SMagicDamText;
    [SerializeField] private TextMeshProUGUI SManaCostText;
    [SerializeField] private TextMeshProUGUI SDamFixInf;
    [SerializeField] private TextMeshProUGUI SAtRangeInf;
    void Awake()
    {//单例模式
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    private void Start()
    {
        AllayP.SetActive(false);
        EnemyP.SetActive(false);
        SkillP.SetActive(false);
    }

    public void SetAllayUIVisible(bool set) 
    {
        if (set)
        {
            SetAllayText();
        }
        else
            SkillP.SetActive(false);
        AllayUIVisible = set;
        AllayP.SetActive(set);
    }
    public void SetEnemyUIVisible(bool set,Enemy target)
    {
        if (set)
        {
            SetEnemyText(target);
        }
        else 
        {
            UnitActionSystem.Instance.choosedEne = null;
        }
        EnemyUIVisible = set;
        EnemyP.SetActive(set);
    }

    public void SetSkillText(Skill skill) 
    {
        SkillP.SetActive(true);
        SNameText.text = new string("SkillName : " + skill.name);
        SDamageText.text = new string("Damage : " + skill.damage);
        if(skill.firedamage == 0 && skill.icedamage != 0)
            SMagicDamText.text = new string("IceDamage : " + skill.icedamage);
        else if(skill.firedamage != 0 && skill.icedamage == 0)
            SMagicDamText.text = new string("FireDamage : " + skill.firedamage);
        else
            SMagicDamText.text = new string("MagicDamage : 0");
        SManaCostText.text = new string("ManaCost : " + skill.manaCost);
        SDamFixInf.text = new string("DamageFix : " + skill.skillDamageFix);
        SAtRangeInf.text = new string("AttackRange : " + skill.attackRange);

    }
    private void SetEnemyText(Enemy enemy)
    {
        CharacterStat enemystat = enemy.GetComponent<CharacterStat>();
        ENameText.text = new string( "Name :"+enemy.name.ToString());
        EHealthText.text = new string("Health : " + enemystat.maxhealth.GetValue().ToString() + "/" +enemystat.currenthealth.ToString());
        EDamageText.text = new string("Damage :" + enemystat.damage.GetValue().ToString());
        EEvationText.text = new string("Evation : " + enemystat.evasion.GetValue().ToString());
        EMovePInf.text =  new string("MovePoint : " + enemy.MaxMovepoint.ToString());
    }

    private void SetAllayText()
    {
        CharacterStat selectedUnitStat = UnitActionSystem.Instance.selectedUnit.GetComponent<CharacterStat>();
        NameText.text = selectedUnitStat.name;
        HealthText.text = new string(selectedUnitStat.currenthealth + "/" + selectedUnitStat.maxhealth.GetValue().ToString());
        ManaInf.text = new string(selectedUnitStat.currentMana.ToString() + "/" + selectedUnitStat.maxMana.GetValue().ToString());
        DamageText.text = selectedUnitStat.damage.GetValue().ToString();
        EvationText.text = selectedUnitStat.evasion.GetValue().ToString();
        MovePInf.text = UnitActionSystem.Instance.selectedUnit.movePoint.ToString();
    }
    public void updateAllayText() 
    {
        CharacterStat selectedUnitStat = UnitActionSystem.Instance.selectedUnit.GetComponent<CharacterStat>();
        HealthText.text = new string(selectedUnitStat.currenthealth + "/" + selectedUnitStat.maxhealth.GetValue().ToString());
        ManaInf.text = new string(selectedUnitStat.currentMana.ToString() + "/" + selectedUnitStat.maxMana.GetValue().ToString());
        MovePInf.text = UnitActionSystem.Instance.selectedUnit.movePoint.ToString();
    }


}
