using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInServerMenuObject : BaseMenuObject
{
    public TextMeshProUGUI scoreText;

    public override void UpdateObject(int index, BaseObject inObj)
    {
        PlayerObject player = (PlayerObject)inObj;
        if (player == null)
            return;
        nameText.text = index + ": " + player.Name;
        scoreText.text = "XP: " + player.Score.ToString();
    }
}
