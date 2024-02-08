using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    public GameObject player;
    public GameObject[] coinPrefabs;
    private Vector3 spawnCoinPosition;

    // Update is called once per frame
    void Update()
    {
        float distanceToHorizon = Vector3.Distance(player.gameObject.transform.position, spawnCoinPosition);
        if (distanceToHorizon < 120)
        {
            SpawnCoins();
        }
    }

    void SpawnCoins()
    {
        spawnCoinPosition = new Vector3(0f, 0f, spawnCoinPosition.z + 30);
        Instantiate(coinPrefabs[(Random.Range(0, 1))], spawnCoinPosition, Quaternion.identity);
    }
}
