using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseMenuObject : MonoBehaviour
{
    public Image panelBackground;
    public TextMeshProUGUI nameText;
    public virtual void UpdateObject(int index, BaseObject inObj) { }
}
