using UnityEngine;
using UnityEngine.Events;
using Meta.XR;
using Meta.XR.MRUtilityKit;
using System.Collections.Generic;

// Folder+File naming and saving for drawing capture 
public class SaveDrawingNameHelper : MonoBehaviour
{
    [SerializeField]
    public string folderPrefix;
    [SerializeField]
    public string[] nameList;

    private int curNameIdx = 0;
    private Dictionary<string, int> numSaved = new Dictionary<string, int>();

    public SaveDrawingToFile saveDrawingToFile;
    public DrawWithTrigger drawer;

    private string baseName; // Original name of the attached GO (SaveDrawingToFile)

    void Start()
    {
        // Save the name of the attached GO to change it later
        baseName = gameObject.name;

        if (nameList.Length != 0)
        {   
            // Set up dict
            foreach (string name in nameList)
            {
                numSaved[name] = 0;
            }

            UpdateGOName();
            Debug.Log($"SaveDrawingNameHelper: switched selected name to '{nameList[curNameIdx]}' ({curNameIdx + 1} / {nameList.Length})");
        }
        else
        {
            Debug.Log($"SaveDrawingNameHelper: nameList is empty, add names to nameList in the inspector");
        }
    }

    void Update()
    {
        // Cycle the current name
        if (OVRInput.GetDown(OVRInput.RawButton.Y))
        {
            if (nameList.Length != 0)
            {
                curNameIdx = (curNameIdx + 1) % nameList.Length;
                UpdateGOName();
                Debug.Log($"SaveDrawingNameHelper: switched selected name to '{nameList[curNameIdx]}'");
            }
        }
        // Save the current drawing
        else if (OVRInput.GetDown(OVRInput.RawButton.LThumbstick))
        {
            if (nameList.Length != 0)
            {
                saveDrawingToFile.SaveDrawing(nameList[curNameIdx], drawer.points, folderPrefix);

                numSaved[nameList[curNameIdx]]++;
                UpdateGOName();
            }
        }
    }

    private void UpdateGOName()
    {
        gameObject.name = $"{baseName} (selected: '{nameList[curNameIdx]}' {curNameIdx + 1} / {nameList.Length} numSaved: {numSaved[nameList[curNameIdx]]})";
    }
}

