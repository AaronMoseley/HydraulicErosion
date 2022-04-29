using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterNode
{
    public WaterNode(int x, int y, float water, float altitude)
    {
        this.x = x;
        this.y = y;
        this.water = water;
        this.altitude = altitude;
        sediment = 0;
    }

    public Vector2Int getPos()
    {
        return new Vector2Int(x, y);
    }

    public float getWater()
    {
        return water;
    }

    public float getAlt()
    {
        return altitude;
    }

    public float getSediment()
    {
        return sediment;
    }

    public void setWater(float water)
    {
        this.water = water;
    }

    public void setAlt(float altitude)
    {
        this.altitude = altitude;
    }

    public void setSediment(float sediment)
    {
        this.sediment = sediment;
    }

    private int x;
    private int y;
    private float water;
    private float altitude;
    private float sediment;
}
