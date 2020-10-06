using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnalyticsMenuObject : BaseMenuObject
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI achievementsText;


    public override void UpdateObject(int index, BaseObject inObj)
    {
        AnalyticsEventObject analytics = (AnalyticsEventObject)inObj;
        if (analytics == null)
            return;
        /* name.text = (index + 1) + ". " + player.Name;
         score.text = player.Score.ToString();
         level.text = player.Level.ToString();
         date.text = String.Format("{0:d/M/yyyy}", player.CreationTime.Date);
         achievements.text = "";
         int trueCount = 0;
         for(int i = 0; i < player.Achievements.Length; i++)
         {
             if(player.Achievements[i] == true)
             {
                 if (trueCount > 0)
                     achievements.text += " | ";
                 trueCount++;
                 achievements.text += Enum.GetName(typeof(Achievement), i);
             }

         }

         if (trueCount == 0)
             achievements.text = "<color=grey>None</color>";
        */
        Debug.Log("new anal");
    }
}
