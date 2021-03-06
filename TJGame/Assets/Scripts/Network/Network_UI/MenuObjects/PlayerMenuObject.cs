﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMenuObject : BaseMenuObject
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI achievementsText;
    public bool isDetail = false;

    public override void UpdateObject(int index, BaseObject inObj)
    {
        PlayerObject player = (PlayerObject)inObj;
        if (player == null)
            return;
        if (!isDetail)
        {
            nameText.text = (index + 1) + ". " + player.Name;
            scoreText.text = player.Score.ToString();
            levelText.text = player.Level.ToString();
            dateText.text = String.Format("{0:d/M/yyyy}", player.CreationTime.Date);
            achievementsText.text = "";
            int trueCount = 0;
            for (int i = 0; i < player.Achievements.Length; i++)
            {
                if (player.Achievements[i] == true)
                {
                    if (trueCount > 0)
                        achievementsText.text += " | ";
                    trueCount++;
                    achievementsText.text += Enum.GetName(typeof(Achievement), i);
                }

            }

            if (trueCount == 0)
                achievementsText.text = "<color=grey>None</color>";
        }
        else
        {
            nameText.text = "Name: " + player.Name;
            scoreText.text = "Score: " + player.Score.ToString();
            levelText.text = "Level: " + player.Level.ToString();
            dateText.text = "Creation time: " + player.CreationTime.ToString();
            achievementsText.text = "Achievements: \n";
            int trueCount = 0;
            for (int i = 0; i < player.Achievements.Length; i++)
            {
                if (player.Achievements[i] == true)
                {
                    if (trueCount > 0)
                        achievementsText.text += " | ";
                    trueCount++;
                    achievementsText.text += Enum.GetName(typeof(Achievement), i);
                }

            }

            if (trueCount == 0)
                achievementsText.text = "Achievements: \nNone";
        }
        
    }
}
