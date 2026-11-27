using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Windows;

/*
AI assistances:
    - Assisted in debugging.
    - Taught Texture3D foramatting, explained why TextureFormat.R8 over TextureFormat.RGBA32.
    - Generated 3D texture shader for sake of debugging.
    - Optimized distance compairson calculation for worley noise, using squared distance instead of vector3.distance.
*/

public class WorleyNoiseGenerator : MonoBehaviour
{
    public static WorleyNoiseGenerator instance;

    [SerializeField]
    int resolution = 256;

    [SerializeField]
    int worleyResolution = 10;

    public Texture2D texture2D;
    public Texture3D texture3D;

    void Awake()
    {
        instance = this;

        texture2D = Generate2DWorleyNoiseTexture(resolution, worleyResolution);
        texture2D.Apply(); //Uploades to gpu memory
        if (GetComponent<Renderer>() != null)
        {
            GetComponent<Renderer>().material.SetTexture("_BaseMap", texture2D); //Sets the quad's texture to the generated texture
        }

        texture3D = Generate3DWorleyNoiseTexture(resolution, worleyResolution);
        texture3D.Apply();
        if (transform.childCount > 0 && transform.GetChild(0).GetComponent<Renderer>()) 
        {
            transform.GetChild(0).GetComponent<Renderer>().material.SetTexture("_DensityTexture", texture3D); ; //Sets the child's texture to the generated texture
        }
    }

    public Texture2D Generate2DWorleyNoiseTexture(int resolution, int noiseResolution) 
    {
        Texture2D output = new Texture2D(resolution, resolution);
        Vector2[,] Grid = new Vector2[noiseResolution, noiseResolution];
        float pixelPerGrid = (float)resolution / noiseResolution;

        //Generate reference points
        for (int i = 0; i < noiseResolution; i++)
        {
            for (int j = 0; j < noiseResolution ; j++) 
            {
                Grid[i, j] = new Vector2((i + Random.Range(0f, 1f)) * pixelPerGrid, (j + Random.Range(0f, 1f)) * pixelPerGrid);
            }
        }

        //Fill the texture;
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                //For each pixel
                int GridX = Mathf.FloorToInt(i / pixelPerGrid); //The gridX current pixel is in
                int GridY = Mathf.FloorToInt(j / pixelPerGrid); //The gridY current pixel is in

                //Compare the distance to each reference points in adjacent grid.
                float minDist = float.MaxValue;
                for (int ii = 0; ii < 3; ii++) 
                {
                    for (int jj = 0; jj < 3; jj++)
                    {
                        float thisDist = Vector2.SqrMagnitude(new Vector2(i, j) - GridWrappper(GridX + (ii - 1), GridY + (jj - 1)));
                        minDist = Mathf.Min(minDist, thisDist);
                    }
                }
                minDist = Mathf.Min(Mathf.Sqrt(minDist)/pixelPerGrid, 1);

                //Spit out the smallest distance.
                output.SetPixel(i,j, new Color(1,1,1,minDist));
            }
        }

        return output;

        Vector2 GridWrappper(int x, int y) //Since we are doing a 3x3 kernal comparesion, leftmost grid's left grid wraps around to right and vise versa. Same for verticle.
        {
            Vector2 output = new Vector2();
            float wrapfactorX = 0; //If it wraps, add the wrap factor to pretend that grid is across the texture in the right position.
            if (x < 0) { x = noiseResolution - 1; wrapfactorX = -resolution; }
            if (x > noiseResolution - 1) { x = 0; wrapfactorX = resolution; }

            float wrapfactorY = 0; //If it wraps, add the wrap factor to pretend that grid is across the texture in the right position.
            if (y < 0) { y = noiseResolution - 1; wrapfactorY = -resolution; }
            if (y > noiseResolution - 1) { y = 0; wrapfactorY = resolution; }

            output = Grid[x, y] + new Vector2(wrapfactorX, wrapfactorY);
            return output;
        }
    }

    public Texture3D Generate3DWorleyNoiseTexture(int resolution, int noiseResolution) 
    {
        //I clamped 3d resolution so it don't fry your computer
        resolution = (int)Mathf.Min((float)resolution, 64f);
        noiseResolution = (int)Mathf.Min((float)noiseResolution, 16f);


        Texture3D output = new Texture3D(resolution, resolution, resolution, TextureFormat.R8, false);
        // R8 means instead of RGB channels, each voxel stores a single 8-bit value (int 0~255) representing density from 0 to 1.
        // the false at end is means we don't need mipmap.
        Vector3[, ,] Grid = new Vector3[noiseResolution, noiseResolution, noiseResolution];
        float pixelPerGrid = (float)resolution / noiseResolution;

        //Generate reference points
        for (int i = 0; i < noiseResolution; i++)
        {
            for (int j = 0; j < noiseResolution; j++)
            {
                for (int k = 0; k < noiseResolution; k++)
                {
                    Grid[i, j, k] = new Vector3((i + Random.Range(0f, 1f)) * pixelPerGrid, (j + Random.Range(0f, 1f)) * pixelPerGrid, (k + Random.Range(0f, 1f)) * pixelPerGrid);
                }
            }
        }

        //Fill the texture;
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                for (int k = 0; k < resolution; k++)
                {
                    //For each pixel
                    int GridX = Mathf.FloorToInt(i / pixelPerGrid); //The gridX current pixel is in
                    int GridY = Mathf.FloorToInt(j / pixelPerGrid); //The gridY current pixel is in
                    int GridZ = Mathf.FloorToInt(k / pixelPerGrid); //The gridY current pixel is in

                    //Compare the distance to each reference points in adjacent grid.
                    float minDist = float.MaxValue;
                    for (int ii = 0; ii < 3; ii++)
                    {
                        for (int jj = 0; jj < 3; jj++)
                        {
                            for (int kk = 0; kk < 3; kk++)
                            {
                                float thisDist = Vector3.SqrMagnitude(new Vector3(i, j, k) - GridWrappper(GridX + (ii - 1), GridY + (jj - 1), GridZ + (kk - 1)));
                                minDist = Mathf.Min(minDist, thisDist);

                            }
                        }
                    }
                    minDist = Mathf.Min(Mathf.Sqrt(minDist) / pixelPerGrid, 1f);

                    //Spit out the smallest distance.
                    output.SetPixel(i, j, k, new Color(minDist, 0, 0, 1));
                }
            }
        }

        return output;

        Vector3 GridWrappper(int x, int y ,int z) //Since we are doing a 3x3 kernal comparesion, leftmost grid's left grid wraps around to right and vise versa. Same for verticle.
        {
            Vector3 output = new Vector3();
            float wrapfactorX = 0; //If it wraps, add the wrap factor to pretend that grid is across the texture in the right position.
            if (x < 0) { x = noiseResolution - 1; wrapfactorX = -resolution; }
            if (x > noiseResolution - 1) { x = 0; wrapfactorX = resolution; }

            float wrapfactorY = 0; //If it wraps, add the wrap factor to pretend that grid is across the texture in the right position.
            if (y < 0) { y = noiseResolution - 1; wrapfactorY = -resolution; }
            if (y > noiseResolution - 1) { y = 0; wrapfactorY = resolution; }

            float wrapfactorZ = 0; //If it wraps, add the wrap factor to pretend that grid is across the texture in the right position.
            if (z < 0) { z = noiseResolution - 1; wrapfactorZ = -resolution; }
            if (z > noiseResolution - 1) { z = 0; wrapfactorZ = resolution; }

            output = Grid[x, y ,z] + new Vector3(wrapfactorX, wrapfactorY, wrapfactorZ);
            return output;
        }
    }
}
