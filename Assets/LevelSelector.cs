using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelector : MonoBehaviour
{
    [SerializeField]
    GameObject[] levels;

    [SerializeField]
    Color lockedColor = Color.red;
    [SerializeField]
    Color unlockedColor = Color.yellow;
    [SerializeField]
    Color completedColor = Color.green;

    [SerializeField]
    Material unlockedMaterial;
    [SerializeField]
    Material completedMaterial;
    [SerializeField]
    Material lockedMaterial;

    [SerializeField]
    int currentLevel = 1;

    void Start()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            if (i + 1 < currentLevel)
            {
                SetActivity(levels[i], completedColor, completedMaterial, false);
            }
            else if (i + 1 == currentLevel)
            {
                SetActivity(levels[i], unlockedColor, unlockedMaterial, true);
            }
            else
            {
                SetActivity(levels[i], lockedColor, lockedMaterial, false);
            }
        }
    }

    public void SetActivity(GameObject level, Color statusColor, Material statusMaterial, bool isActive)
    {

        PortalScript portal = level.GetComponentInChildren<PortalScript>();
        if (portal != null)
        {
            portal.enabled = isActive;
        }
        GameObject door = level.transform.Find("DoorsTrigger").gameObject;
        if (door != null)
        {
            door.SetActive(isActive);
        }
        GameObject panelLight = level.transform.Find("Panel_Light").gameObject;
        if (panelLight != null)
        {
            MeshRenderer meshRenderer = panelLight.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                Material[] materials = meshRenderer.materials;
                materials[1] = statusMaterial;
                meshRenderer.materials = materials;
            }

            Light light = panelLight.GetComponentInChildren<Light>();
            if (light != null)
            {
                light.color = statusColor;
            }
        }
    }

    public void SetLevelComplete(int levelIndex)
    {
        if (levelIndex < 1 || levelIndex > levels.Length) return; // Out of bounds check

        if (levelIndex == 4)
        {
            GameObject level = levels[levelIndex-1];
            SetActivity(level, completedColor, completedMaterial, false);
            return; // Do nothing further
        }

        GameObject levelToComplete = levels[levelIndex-1];
        SetActivity(levelToComplete, completedColor, completedMaterial, false);
        currentLevel = levelIndex; // Update current level to the next one
        if (currentLevel < levels.Length)
        {
            SetActivity(levels[currentLevel], unlockedColor, unlockedMaterial, true);
        }
    }

}
