using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public enum DrawMode { Texture, Terrain };
    public DrawMode mode;
    public bool autoUpdate;
    public GameObject infoPanel;
    bool showingInfo = false;

    public Button rainButton;

    [Space]
    [Header("Generation")]
    public int textureSize = 255;
    public int layers;
    public int seed;
    public float startingFrequency;
    public float layerChangeFactor;
    public float heightFactor;
    public AnimationCurve curve;
    [System.NonSerialized]
    public float[,] terrainHeights;

    [Space]
    [Header("Texture")]
    public ColorObj[] colors;
    public GameObject texturePlane;

    [Space]
    [Header("Erosion")]
    public float volumeMultiplier;
    public float altitudeMultiplier;
    public float sedimentCapacity;
    public float deposition;
    public float softness;
    public float evaporation;
    public int iterations;
    [System.NonSerialized]
    public WaterNode[,] erosionNodes;
    [System.NonSerialized]
    public float erosionStartTime;

    [Space]
    [Header("Rain")]
    public int rainIterations;
    public int erosionIterations;
    public float heaviness;
    public float minWaterContent;
    [System.NonSerialized]
    public int rainCounter = 0;
    [System.NonSerialized]
    public bool stillEroding = true;
    [System.NonSerialized]
    public WaterNode[,] rainNodes;
    [System.NonSerialized]
    public bool isRaining = false;
    [System.NonSerialized]
    public float rainStartTime;

    void Start()
    {

    }

    private void Update()
    {
        if(showingInfo && Input.GetKeyDown(KeyCode.Mouse0))
        {
            infoPanel.SetActive(false);
            showingInfo = false;
        }

        if (isRaining)
        {
            if (rainCounter > 0 && stillEroding)
            {
                float maxWater = 0;
                for (int i = 0; i < textureSize; i++)
                {
                    for (int j = 0; j < textureSize; j++)
                    {
                        maxWater = (maxWater > rainNodes[i, j].getWater()) ? maxWater : rainNodes[i, j].getWater();
                    }
                }

                float[,] heights;
                if (maxWater >= minWaterContent || rainCounter < rainIterations)
                {
                    for (int i = 0; i < erosionIterations; i++)
                    {
                        Erode(ref rainNodes);
                    }

                    if (rainCounter >= rainIterations)
                    {
                        for (int i = 0; i < textureSize; i++)
                        {
                            for (int j = 0; j < textureSize; j++)
                            {
                                rainNodes[i, j].setWater(rainNodes[i, j].getWater() * (1 - evaporation));
                            }
                        }
                    }

                    heights = EndErosion(rainNodes, false, false, -1f);
                } else
                {
                    Debug.Log("Done Raining");
                    heights = EndErosion(rainNodes, true, true, rainStartTime);
                    rainButton.interactable = true;
                    stillEroding = false;
                    isRaining = false;
                }

                GenerateMesh(heights, "RainTerrain", new Vector2((textureSize * 2) + 20, 0), false);
            }

            if (rainCounter < rainIterations && isRaining)
            {
                for (int i = 0; i < textureSize; i++)
                {
                    for (int j = 0; j < textureSize; j++)
                    {
                        rainNodes[i, j].setWater(rainNodes[i, j].getWater() + (heaviness * volumeMultiplier));
                    }
                }

                rainCounter++;
            }
        }
    }

    public void DrawTexture()
    {
        float[,] heights = NoiseGeneration.PerlinNoise(textureSize, textureSize, layers, seed, startingFrequency, layerChangeFactor);
        SetTexture(heights, texturePlane, true);
    }

    void SetTexture(float[,] color, GameObject obj, bool useCurve)
    {
        Material mat = new Material(Shader.Find("Specular"));

        Texture2D texture = new Texture2D(color.GetLength(0), color.GetLength(1));
        Color[] colorMap = new Color[textureSize * textureSize];

        for(int i = 0; i < color.GetLength(0); i++)
        {
            for(int j = 0; j < color.GetLength(1); j++)
            {
                float usableHeight = (useCurve) ? curve.Evaluate(Mathf.Clamp(color[i, j], 0f, 1f)) : Mathf.Clamp(color[i, j], 0f, 1f);

                ColorObj firstColor = colors[0];
                ColorObj secondColor = colors[0];

                for(int k = 0; k < colors.Length - 1; k++)
                {
                    if(colors[k + 1].height >= usableHeight && colors[k].height <= usableHeight)
                    {
                        firstColor = colors[k];
                        secondColor = colors[k + 1];
                    }
                }

                float totalDist = Mathf.Abs(secondColor.height - firstColor.height);
                float coeff1 = Mathf.Abs(usableHeight - secondColor.height) / totalDist;
                float coeff2 = Mathf.Abs(usableHeight - firstColor.height) / totalDist;

                texture.SetPixel(i, j, (firstColor.color * coeff1) + (secondColor.color * coeff2));
            }
        }

        texture.Apply();

        texture.filterMode = FilterMode.Trilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        mat.mainTexture = texture;
        obj.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    public void GenerateTerrain(bool replaceHeights)
    {
        if(replaceHeights)
            terrainHeights = NoiseGeneration.PerlinNoise(textureSize, textureSize, layers, seed, startingFrequency, layerChangeFactor);

        /*string write = "";

        for(int i = 0; i < textureSize; i++)
        {
            for(int j = 0; j < textureSize; j++)
            {
                write += terrainHeights[i, j].ToString() + " ";
            }

            write += "\n";
        }

        System.IO.File.WriteAllText(Application.dataPath + "/heights.txt", write);*/

        GenerateMesh(terrainHeights, "Terrain", Vector2.zero, true);
    }

    public void GenerateMesh(float[,] heights, string tag, Vector2 offset, bool useCurve)
    {
        GameObject[] planes = GameObject.FindGameObjectsWithTag(tag);

        for (int i = 1; i < planes.Length; i++)
        {
            DestroyImmediate(planes[i]);
        }

        List<Vector3> verts = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (int i = 0; i < heights.GetLength(1); i++)
        {
            for (int j = 0; j < heights.GetLength(0); j++)
            {
                float usableHeight;
                if(useCurve)
                    usableHeight = curve.Evaluate(heights[i, j]);
                else
                    usableHeight = heights[i, j];

                verts.Add(new Vector3(i, usableHeight * heightFactor, j));

                if (i != 0 && j != 0)
                {
                    triangles.Add(textureSize * i + j - textureSize);
                    triangles.Add(textureSize * i + j);
                    triangles.Add(textureSize * i + j - 1);

                    triangles.Add(textureSize * i + j - 1);
                    triangles.Add(textureSize * i + j - 1 - textureSize);
                    triangles.Add(textureSize * i + j - textureSize);
                }
            }
        }

        for (int i = 0; i < verts.Count; i++)
        {
            uvs.Add(new Vector2(verts[i].x / (float)textureSize, verts[i].z / (float)textureSize));
        }

        GameObject plane;
        if (planes.Length == 0)
        {
            plane = new GameObject(tag);
            plane.tag = tag;
            plane.AddComponent<MeshFilter>();
            plane.AddComponent<MeshRenderer>();
        } else
        {
            plane = planes[0];
        }

        Mesh terrainMesh = new Mesh();
        terrainMesh.vertices = verts.ToArray();
        terrainMesh.triangles = triangles.ToArray();
        terrainMesh.uv = uvs.ToArray();

        terrainMesh.RecalculateNormals();
        plane.GetComponent<MeshFilter>().sharedMesh = terrainMesh;

        SetTexture(heights, plane, useCurve);

        plane.transform.position = new Vector3(offset.x, 0, offset.y);
    }

    public void SetUpErosion(ref WaterNode[,] nodes, bool useCurve)
    {
        //Initialize node array and define all starting nodes
        nodes = new WaterNode[textureSize, textureSize];
        for(int i = 0; i < textureSize; i++)
        {
            for(int j = 0; j < textureSize; j++)
            {
                nodes[i, j] = new WaterNode(i, j, volumeMultiplier, (useCurve) ? curve.Evaluate(terrainHeights[i, j]) * altitudeMultiplier : terrainHeights[i, j] * altitudeMultiplier);
            }
        }
    }

    public void Erode(ref WaterNode[,] nodes)
    {
        Random.InitState((int)Time.realtimeSinceStartup);

        List<WaterNode> listNodes = new List<WaterNode>();
        for(int i = 0; i < textureSize; i++)
        {
            for(int j = 0; j < textureSize; j++)
            {
                listNodes.Add(nodes[i, j]);
            }
        }

        for(int i = 0; i < listNodes.Count; i++)
        {
            WaterNode temp = listNodes[i];
            int newIndex = Random.Range(0, listNodes.Count);
            listNodes[i] = listNodes[newIndex];
            listNodes[newIndex] = temp;
        }

        //Loop through all nodes
        for (int index = 0; index < listNodes.Count; index++)
        {
            WaterNode currNode = listNodes[index];

            WaterNode[] neighbors = getNeighbors(currNode.getPos().x, currNode.getPos().y, nodes);
                
            for (int i = 0; i < neighbors.Length; i++)
            {
                //Evaporate stagnant water
                currNode.setWater(currNode.getWater() * (1f - evaporation));

                //Find out how much water can move to each neighbor
                float deltaW = (currNode.getWater() < ((currNode.getWater() + currNode.getAlt()) - (neighbors[i].getAlt() + neighbors[i].getWater()))) ? currNode.getWater() : ((currNode.getWater() + currNode.getAlt()) - (neighbors[i].getAlt() + neighbors[i].getWater()));

                if(deltaW <= 0)
                {
                    //Deposit sediment if the water isn't moving
                    currNode.setAlt(currNode.getAlt() + (deposition * currNode.getSediment()));
                    currNode.setSediment(currNode.getSediment() - (deposition * currNode.getSediment()));
                } else
                {
                    //Move to adjacent nodes if water moves down
                    currNode.setWater(currNode.getWater() - deltaW);
                    neighbors[i].setWater(neighbors[i].getWater() + deltaW);

                    float capacity = sedimentCapacity * deltaW;

                    if(currNode.getSediment() >= capacity)
                    {
                        neighbors[i].setSediment(neighbors[i].getSediment() + capacity);
                        currNode.setAlt(currNode.getAlt() + (deposition * (currNode.getSediment() - capacity)));
                        currNode.setSediment((1 - deposition) * (currNode.getSediment() - capacity));
                    } else
                    {
                        neighbors[i].setSediment(neighbors[i].getSediment() + currNode.getSediment() + (softness * (capacity - currNode.getSediment())));
                        currNode.setAlt(currNode.getAlt() - (softness * (capacity - currNode.getSediment())));
                        currNode.setSediment(0);
                    }
                }
            }
        }
    }

    public float[,] EndErosion(WaterNode[,] nodes, bool addSediment, bool displayData, float startTime)
    {
        //Establish heights between 0 and 1 that can be used to generate a mesh
        float[,] usableHeights = new float[textureSize, textureSize];

        float minDiff = float.MaxValue;
        float maxDiff = 0;
        float avgDiff = 0;
        float overallMax = float.MinValue;
        float prevMax = float.MinValue;
        float overallMin = float.MaxValue;
        float prevMin = float.MaxValue;

        for (int i = 0; i < textureSize; i++)
        {
            for(int j = 0; j < textureSize; j++)
            {
                usableHeights[i, j] = (((addSediment) ? nodes[i, j].getSediment() : 0) + nodes[i, j].getAlt()) / altitudeMultiplier;

                if (displayData)
                {
                    minDiff = (Mathf.Abs(usableHeights[i, j] - terrainHeights[i, j]) < Mathf.Abs(minDiff)) ? usableHeights[i, j] - terrainHeights[i, j] : minDiff;
                    maxDiff = (Mathf.Abs(usableHeights[i, j] - terrainHeights[i, j]) > Mathf.Abs(maxDiff)) ? usableHeights[i, j] - terrainHeights[i, j] : maxDiff;
                    avgDiff += usableHeights[i, j] - terrainHeights[i, j];
                    overallMax = (usableHeights[i, j] > overallMax) ? usableHeights[i, j] : overallMax;
                    prevMax = (terrainHeights[i, j] > prevMax) ? terrainHeights[i, j] : prevMax;
                    overallMin = (usableHeights[i, j] < overallMin) ? usableHeights[i, j] : overallMin;
                    prevMin = (terrainHeights[i, j] < prevMin) ? terrainHeights[i, j] : prevMin;
                }
            }
        }

        if (displayData)
        {
            avgDiff /= textureSize * textureSize;

            Text infoText = infoPanel.GetComponentInChildren<Text>();
            infoText.text = "Erosion Algorithm Summary: \n\n";
            
            infoText.text += "Running Time (seconds): " + (Time.realtimeSinceStartup - startTime) + "\n\n";

            infoText.text += "Minimum Difference: " + minDiff + "\n";
            infoText.text += "Maximum Difference: " + maxDiff + "\n";
            infoText.text += "Average Difference: " + avgDiff + "\n\n";
            
            infoText.text += "Overall Maximum: " + overallMax + "\n";
            infoText.text += "Previous Maximum: " + prevMax + "\n\n";
            
            infoText.text += "Overall Minimum: " + overallMin + "\n";
            infoText.text += "Previous Minimum: " + prevMin + "\n\n";

            infoText.text += "Click to close this window";

            infoPanel.SetActive(true);
            showingInfo = true;
        }

        return usableHeights;
    }

    public WaterNode[] getNeighbors(int x, int y, WaterNode[,] nodes)
    {
        List<WaterNode> result = new List<WaterNode>();

        //Loop through all nodes who have an offset of at most x: 1, y: 1
        for(int i = -1; i <= 1; i++)
        {
            for(int j = -1; j <= 1; j++)
            {
                //Make sure we're at a valid node that's not the current node
                if(x + i >= 0 && x + i < textureSize && y + j >= 0 && y + j < textureSize && (i != 0 || j != 0))
                {
                    if (result.Count == 0)
                    {
                        //Initialize list
                        result.Add(nodes[x + i, y + j]);
                    } else
                    {
                        //Add node list in increasing order based on altitude
                        int index = 0;
                        while (index < result.Count && result[index].getAlt() < nodes[x + i, y + j].getAlt())
                        {
                            index++;
                        }

                        result.Insert(index, nodes[x + i, y + j]);
                    }
                }
            }
        }

        return result.ToArray();
    }

    [System.Serializable]
    public struct ColorObj
    {
        public Color color;
        public float height;
    }
}
