using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMenuListCreator : MonoBehaviour
{
    [SerializeField]
    private GameObject emptyText;
    [SerializeField]
    private GameObject menuItem = null;

    [SerializeField]
    private RectTransform content = null;
    private int itemSize = 60;

    public void RefreshList(PlayerObject[] playerList)
    {
        emptyText.SetActive(playerList.Length == 0);

        //Clear old servers;
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        for (int i = 0; i < playerList.Length; i++)
        {
            GameObject spawnedMenuItem = Instantiate(menuItem);//, pos, SpawnPoint.rotation);

            //setParent
            spawnedMenuItem.transform.SetParent(content, false);
            PlayerMenuObject playerMenuObject = spawnedMenuItem.GetComponent<PlayerMenuObject>();

            if (playerMenuObject == null)
                return;

            playerMenuObject.UpdatePlayer(i, playerList[i]);
            if(i % 2 == 0)
                playerMenuObject.background.color = new Color32(210, 210, 210, 191);
        }
    }
}
