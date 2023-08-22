using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField]
    private Transform _tilePrefab;

    [SerializeField]
    private Vector2 _mapSize;

    [SerializeField][Range(0, 1)]
    private float _outlinePercent;

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        if (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        
        GameObject mapHolder = new GameObject("Generated Map");
        mapHolder.transform.parent = transform;

        for (int x = 0; x < _mapSize.x; x++)
        {
            for (int y = 0; y < _mapSize.y; y++)
            {
                Vector3 tilePosition = new Vector3(-_mapSize.x / 2 + 0.5f + x, 0, -_mapSize.y / 2 + 0.5f + y);
                Transform newTile = Instantiate(_tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * (1 - _outlinePercent);
                newTile.parent = mapHolder.transform;
            }
        }
    }

}
