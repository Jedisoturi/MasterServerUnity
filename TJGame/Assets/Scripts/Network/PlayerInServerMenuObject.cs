using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInServerMenuObject : MonoBehaviour
{
    public TextMeshProUGUI name;
    public TextMeshProUGUI score;
    public Image background;

    public void UpdatePlayer(int index, PlayerObject player)
    {
        name.text = index + ": " + player.Name;
        score.text = "XP: " + player.Score.ToString();
    }
}
