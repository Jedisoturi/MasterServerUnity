using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ServerMenuListCreator : MonoBehaviour
{
    [SerializeField]
    private GameObject emptyText;
    [SerializeField]
    private GameObject menuItem = null;

    [SerializeField]
    private RectTransform content = null;

    [SerializeField]
    private UIManager _UIManager = null;



    public void RefreshList(ServerObject[] serverList)
    {
        emptyText.SetActive(serverList.Length == 0);

        //Clear old servers;
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        for (int i = 0; i < serverList.Length; i++)
        {
            GameObject spawnedMenuItem = Instantiate(menuItem);//, pos, SpawnPoint.rotation);

            //setParent
            spawnedMenuItem.transform.SetParent(content, false);
            ServerMenuObject serverMenuObject = spawnedMenuItem.GetComponent<ServerMenuObject>();

            if (serverMenuObject == null)
                return;

            serverMenuObject.Init(_UIManager, serverList[i]);
            if(i % 2 == 0)
                serverMenuObject.background.color = new Color32(210, 210, 210, 191);
        }
    }
}
