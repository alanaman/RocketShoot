using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class StatsUI : MonoBehaviour
{
    [SerializeField] GameObject singleHealthBarLayout;
    [SerializeField] Transform healthBar;

    private void Awake()
    {
        singleHealthBarLayout.SetActive(false);
    }

    private void Update()
    {
        int currHealth = (int)Mathf.Ceil(GameManager.I.Player.GetHealth());
        currHealth = math.max(currHealth, 0);
        if(currHealth == healthBar.childCount)
        {
            return;
        }
        else if(currHealth > healthBar.childCount)
        {
            for(int i=0;i<currHealth - healthBar.childCount;i++)
            {
                Instantiate(singleHealthBarLayout, healthBar).SetActive(true);
            }
        }
        else if(currHealth < healthBar.childCount)
        {
            for(int i=healthBar.childCount;i>currHealth;i--)
            {
                Destroy(healthBar.GetChild(i-1).gameObject);
            }
        }
    }

}
