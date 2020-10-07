using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuListCreator : MonoBehaviour
{
    [SerializeField]
    private GameObject emptyText;
    [SerializeField]
    private GameObject menuItem = null;

    public void RefreshList(BaseObject[] objectList)
    {
        emptyText.SetActive(objectList.Length == 0);

        //Clear old servers;
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        for (int i = 0; i < objectList.Length; i++)
        {
            GameObject spawnedMenuItem = Instantiate(menuItem, GetComponent<RectTransform>());

           BaseMenuObject baseMenuObject = spawnedMenuItem.GetComponent<BaseMenuObject>();

            if (baseMenuObject == null)
                return;

            baseMenuObject.UpdateObject(i, objectList[i]);
            if(i % 2 == 0)
                baseMenuObject.panelBackground.color = new Color32(210, 210, 210, 191);
        }
    }
}
