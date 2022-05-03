using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject fullUI;
    public GameObject helpMessages;
    public Text filePath;

    public Button generateButton;
    public InputField[] generationVariables;

    public Button erodeButton;
    public InputField[] erosionVariables;

    public Button rainButton;
    public InputField[] rainVariables;

    [System.NonSerialized]
    public string currTerrain = "";

    TerrainManager terrain;

    void Start()
    {
        filePath.text += Application.dataPath;
        terrain = gameObject.GetComponent<TerrainManager>();

        for(int i = 0; i < generationVariables.Length; i++)
        {
            switch (i)
            {
                case 0:
                    generationVariables[i].text = terrain.seed.ToString();
                    break;
                case 1:
                    generationVariables[i].text = terrain.layers.ToString();
                    break;
                case 2:
                    generationVariables[i].text = terrain.heightFactor.ToString();
                    break;
                case 3:
                    generationVariables[i].text = terrain.layerChangeFactor.ToString();
                    break;
            }
        }

        for (int i = 0; i < erosionVariables.Length; i++)
        {
            switch (i)
            {
                case 0:
                    erosionVariables[i].text = terrain.volumeMultiplier.ToString();
                    break;
                case 1:
                    erosionVariables[i].text = terrain.sedimentCapacity.ToString();
                    break;
                case 2:
                    erosionVariables[i].text = terrain.evaporation.ToString();
                    break;
                case 3:
                    erosionVariables[i].text = terrain.iterations.ToString();
                    break;
            }
        }

        for (int i = 0; i < rainVariables.Length; i++)
        {
            switch (i)
            {
                case 0:
                    rainVariables[i].text = terrain.rainIterations.ToString();
                    break;
                case 1:
                    rainVariables[i].text = terrain.erosionIterations.ToString();
                    break;
                case 2:
                    rainVariables[i].text = terrain.heaviness.ToString();
                    break;
            }
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.H))
        {
            fullUI.SetActive(!fullUI.activeSelf);
            helpMessages.SetActive(fullUI.activeSelf);
        } else if(Input.GetKeyDown(KeyCode.G))
        {
            helpMessages.SetActive(!helpMessages.activeSelf);
        }
    }

    public void GenerateTerrain()
    {
        for(int i = 0; i < generationVariables.Length; i++)
        {
            switch(i)
            {
                case 0:
                    try
                    {
                        int seed = int.Parse(generationVariables[i].text);
                        terrain.seed = seed;
                    } catch
                    {
                        generationVariables[i].text = terrain.seed.ToString();
                    }
                    break;
                case 1:
                    try
                    {
                        int layers = int.Parse(generationVariables[i].text);
                        terrain.layers = layers;
                    }
                    catch
                    {
                        generationVariables[i].text = terrain.layers.ToString();
                    }
                    break;
                case 2:
                    try
                    {
                        float heightFactor = float.Parse(generationVariables[i].text);
                        terrain.heightFactor = heightFactor;
                    } catch
                    {
                        generationVariables[i].text = terrain.heightFactor.ToString();
                    }
                    break;
                case 3:
                    try
                    {
                        float layerChangeFactor = float.Parse(generationVariables[i].text);
                        terrain.layerChangeFactor = layerChangeFactor;
                    } catch
                    {
                        generationVariables[i].text = terrain.layerChangeFactor.ToString();
                    }
                    break;
            }
        }

        terrain.isRaining = false;
        rainButton.interactable = false;

        GameObject[] terrain1 = GameObject.FindGameObjectsWithTag("ErodedTerrain");
        GameObject[] terrain2 = GameObject.FindGameObjectsWithTag("RainTerrain");

        for(int i = 0; i < terrain1.Length; i++)
        {
            Destroy(terrain1[i]);
        }

        for(int i = 0; i < terrain2.Length; i++)
        {
            Destroy(terrain2[i]);
        }

        terrain.erosionNodes = null;
        terrain.rainNodes = null;
        terrain.rainCounter = 0;
        terrain.stillEroding = true;

        if(System.IO.File.Exists(Application.dataPath + "/heights.txt"))
        {
            try
            {
                string[] fileLines = System.IO.File.ReadAllLines(Application.dataPath + "/heights.txt");
                string[][] stringHeights = new string[terrain.textureSize][];

                float[,] newHeights = new float[terrain.textureSize, terrain.textureSize];

                float max = float.MinValue;
                for(int i = 0; i < terrain.textureSize; i++)
                {
                    stringHeights[i] = fileLines[i].Split(' ');

                    for(int j = 0; j < terrain.textureSize; j++)
                    {
                        newHeights[i, j] = float.Parse(stringHeights[i][j]);
                        if(newHeights[i, j] > max)
                        {
                            max = newHeights[i, j];
                        }
                    }
                }

                terrain.terrainHeights = new float[terrain.textureSize, terrain.textureSize];

                for(int i = 0; i < terrain.textureSize; i++)
                {
                    for(int j = 0; j < terrain.textureSize; j++)
                    {
                        newHeights[i, j] /= max;

                        terrain.terrainHeights[i, j] = newHeights[i, j];
                    }
                }

                Debug.Log("Loaded File");

                generateButton.interactable = false;
                for(int i = 0; i < generationVariables.Length; i++)
                {
                    generationVariables[i].interactable = false;
                }

                terrain.GenerateTerrain(false);
            } catch
            {
                terrain.GenerateTerrain(true);
                Debug.Log("Incorrect Format");
            }
        } else
        {
            terrain.GenerateTerrain(true);
        }

        erodeButton.interactable = true;
    }

    public void ErodeTerrain()
    {
        for (int i = 0; i < erosionVariables.Length; i++)
        {
            switch (i)
            {
                case 0:
                    try
                    {
                        float volume = float.Parse(erosionVariables[i].text);
                        terrain.volumeMultiplier = volume;
                    }
                    catch
                    {
                        erosionVariables[i].text = terrain.volumeMultiplier.ToString();
                    }
                    break;
                case 1:
                    try
                    {
                        float sediment = float.Parse(erosionVariables[i].text);
                        terrain.sedimentCapacity = sediment;
                    }
                    catch
                    {
                        erosionVariables[i].text = terrain.sedimentCapacity.ToString();
                    }
                    break;
                case 2:
                    try
                    {
                        float evaporation = float.Parse(erosionVariables[i].text);
                        terrain.evaporation = evaporation;
                    }
                    catch
                    {
                        erosionVariables[i].text = terrain.evaporation.ToString();
                    }
                    break;
                case 3:
                    try
                    {
                        int iterations = int.Parse(erosionVariables[i].text);
                        terrain.iterations = iterations;
                    }
                    catch
                    {
                        erosionVariables[i].text = terrain.iterations.ToString();
                    }
                    break;
            }
        }

        terrain.erosionStartTime = Time.realtimeSinceStartup;
        terrain.SetUpErosion(ref terrain.erosionNodes, true);

        GameObject rainTerrain = GameObject.FindGameObjectWithTag("RainTerrain");
        if (rainTerrain != null)
            Destroy(rainTerrain);

        for (int i = 0; i < terrain.iterations; i++)
        {
            terrain.Erode(ref terrain.erosionNodes);
        }

        terrain.GenerateMesh(terrain.EndErosion(terrain.erosionNodes, true, true, terrain.erosionStartTime, "erosion"), "ErodedTerrain", new Vector2(terrain.textureSize + 10, 0), false);

        rainButton.interactable = true;
    }

    public void StartRain()
    {
        for (int i = 0; i < rainVariables.Length; i++)
        {
            switch (i)
            {
                case 0:
                    try
                    {
                        int rainIterations = int.Parse(rainVariables[i].text);
                        terrain.rainIterations = rainIterations;
                    }
                    catch
                    {
                        rainVariables[i].text = terrain.rainIterations.ToString();
                    }
                    break;
                case 1:
                    try
                    {
                        int erosionIterations = int.Parse(rainVariables[i].text);
                        terrain.erosionIterations = erosionIterations;
                    }
                    catch
                    {
                        rainVariables[i].text = terrain.erosionIterations.ToString();
                    }
                    break;
                case 2:
                    try
                    {
                        float heaviness = float.Parse(rainVariables[i].text);
                        terrain.heaviness = heaviness;
                    }
                    catch
                    {
                        rainVariables[i].text = terrain.heaviness.ToString();
                    }
                    break;
            }
        }

        terrain.rainStartTime = Time.realtimeSinceStartup;
        terrain.SetUpErosion(ref terrain.rainNodes, true);

        for (int i = 0; i < terrain.textureSize; i++)
        {
            for (int j = 0; j < terrain.textureSize; j++)
            {
                terrain.rainNodes[i, j].setWater(0f);
            }
        }

        rainButton.interactable = false;
        terrain.isRaining = true;
        terrain.stillEroding = true;
        terrain.rainCounter = 0;
    }

    public bool UIFocused()
    {
        bool result = false;

        for (int i = 0; i < generationVariables.Length; i++)
        {
            result = result || generationVariables[i].isFocused;
        }

        for (int i = 0; i < erosionVariables.Length; i++)
        {
            result = result || erosionVariables[i].isFocused;
        }

        for (int i = 0; i < rainVariables.Length; i++)
        {
            result = result || rainVariables[i].isFocused;
        }

        return result;
    }

    public void Output()
    {
        string path = Application.dataPath + "/output.txt";
        string write = "";

        switch(currTerrain) {
            case "erosion":
                for(int i = 0; i < terrain.textureSize; i++)
                {
                    for(int j = 0; j < terrain.textureSize; j++)
                    {
                        write += (terrain.erosionNodes[i, j].getAlt() / terrain.altitudeMultiplier).ToString() + " ";
                    }
                    write += "\n";
                }
                break;
            case "rain":
                for (int i = 0; i < terrain.textureSize; i++)
                {
                    for (int j = 0; j < terrain.textureSize; j++)
                    {
                        write += (terrain.rainNodes[i, j].getAlt() / terrain.altitudeMultiplier).ToString() + " ";
                    }
                    write += "\n";
                }
                break;
            default:
                return;
        }

        System.IO.File.WriteAllText(path, write);
        currTerrain = "";
    }
}
