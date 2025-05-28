using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelComplete : MonoBehaviour
{
    [SerializeField]
    private int levelName;

    [SerializeField]
    private LevelSelector levelSelector;

    void Start()
    {
        string levelString = transform.parent.gameObject.name;
        levelString = levelString.Replace("End", "");
        levelString = levelString.Replace("Level", "");
        levelName = int.Parse(levelString);
    }

    void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has completed the level: " + levelName);

            levelSelector = GameObject.Find("LevelSelector").GetComponent<LevelSelector>();
            if (levelSelector != null)
            {
                levelSelector.SetLevelComplete(levelName);
            }
            else
            {
                Debug.LogError("LevelSelector not found in the scene.");
            }

        }
    }
}
