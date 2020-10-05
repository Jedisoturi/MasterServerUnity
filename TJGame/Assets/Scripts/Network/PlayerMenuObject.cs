using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMenuObject : MonoBehaviour
{
    public TextMeshProUGUI name;
    public TextMeshProUGUI score;
    public TextMeshProUGUI level;
    public TextMeshProUGUI date;
    public TextMeshProUGUI achievements;
    public Image background;

    public void UpdatePlayer(int index, PlayerObject player)
    {
        name.text = (index + 1) + ". " + player.Name;
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
    }
}
