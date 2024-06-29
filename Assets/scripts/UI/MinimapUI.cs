using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.UI;

public class MinimapUI : MonoBehaviour
{
    [SerializeField] RawImage minimapImage;
    [SerializeField] Vector2 WorldSize;

    void Update()
    {
        Vector3 playerPos = GameManager.I.Player.transform.position;
        Vector2 playerPos2D = new Vector2(playerPos.x, playerPos.z);

        Vector2 playerPosOnMap = new Vector2(
            playerPos2D.x / WorldSize.x + .5f,
            playerPos2D.y / WorldSize.y + .5f
        );

        Vector2 mapSize = minimapImage.uvRect.size;

        minimapImage.uvRect = new Rect(
            playerPosOnMap.x - mapSize.x / 2,
            playerPosOnMap.y - mapSize.y / 2,
            mapSize.x,
            mapSize.y
        );
    }
}
