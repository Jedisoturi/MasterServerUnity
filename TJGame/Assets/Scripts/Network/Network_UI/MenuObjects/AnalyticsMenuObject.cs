using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnalyticsMenuObject : BaseMenuObject
{
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI dateText;

    public override void UpdateObject(int index, BaseObject inObj)
    {
        AnalyticsEventObject analytics = (AnalyticsEventObject)inObj;
        if (analytics == null)
            return;
         nameText.text = (index) + ". " + analytics.PlayerId;
         typeText.text = Enum.GetName(typeof(EventType), analytics.Type);
         messageText.text = analytics.Message;
         dateText.text = String.Format("{0:d/M/yyyy}", analytics.CreationTime.Date);//
    } 
}
