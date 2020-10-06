using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnalyticsMenuObject : BaseMenuObject
{
    public TextMeshProUGUI indexText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI dateText;

    public override void UpdateObject(int index, BaseObject inObj)
    {
        AnalyticsEventObject analytics = (AnalyticsEventObject)inObj;
        if (analytics == null)
            return;
         indexText.text = (index.ToString());
         nameText.text = "PlayerID: " + analytics.PlayerId.ToString();
         typeText.text = Enum.GetName(typeof(EventType), analytics.Type);
         messageText.text = "Message: " + analytics.Message;
        dateText.text = analytics.CreationTime.ToString(); //String.Format("{0:d/M/yyyy}", analytics.CreationTime.Date);//
    } 
}
