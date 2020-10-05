using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerMenuListCreator : MonoBehaviour
{

    [SerializeField]
    private Transform SpawnPoint = null;

    [SerializeField]
    private GameObject menuItem = null;

    [SerializeField]
    private RectTransform content = null;

    private int itemSize = 60;

    public void RefreshList(ServerObject[] serverList)
    {
        //Clear old servers;
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        for (int i = 0; i < serverList.Length; i++)
        {
            GameObject spawnedMenuItem = Instantiate(menuItem);//, pos, SpawnPoint.rotation);

            //setParent
            spawnedMenuItem.transform.SetParent(SpawnPoint, false);
            ServerMenuObject serverMenuObject = spawnedMenuItem.GetComponent<ServerMenuObject>();

            if (serverMenuObject == null)
                return;

            serverMenuObject.Init(serverList[i]);
            if(i % 2 == 0)
                serverMenuObject.background.color = new Color(166, 166, 166);
        }
    }
}
