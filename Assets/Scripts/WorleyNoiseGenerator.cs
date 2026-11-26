using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Windows;

public class WorleyNoiseGenerator : MonoBehaviour
{
    public static WorleyNoiseGenerator instance;

    [SerializeField]
    int resolution = 256;

    [SerializeField]
    int worleyResolution = 10;

    public Texture2D texture2D;

    void Awake()
    {
        instance = this;

        texture2D = Generate2DWorleyNoiseTexture(resolution, worleyResolution);
        texture2D.Apply(); //Uploades to gpu memory
        if (GetComponent<Renderer>() != null)
        {
            GetComponent<Renderer>().material.SetTexture("_BaseMap", texture2D); //Sets the quad's texture to the generated texture
        }
    }

    public Texture2D Generate2DWorleyNoiseTexture(int resolution, int noiseResolution) 
    {
        float pixelPerGrid = (float)resolution / noiseResolution;

        Vector2[,] Grid = new Vector2[noiseResolution, noiseResolution];
        Texture2D output = new Texture2D(resolution, resolution);

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

                float minDist = 1;
                for (int ii = 0; ii < 3; ii++) 
                {
                    for (int jj = 0; jj < 3; jj++)
                    {
                        //float thisDist = Vector2.Distance(new Vector2(i, j), Grid[GridWrapper(GridX + (ii - 1)), GridWrapper(GridY + (jj - 1))]);
                        float thisDist = Vector2.Distance(new Vector2(i, j), GridWrappper(GridX + (ii - 1), GridY + (jj - 1))) / pixelPerGrid;
                        minDist = Mathf.Min(minDist, thisDist);
                        /*
                        if (i == 55 && j == 55) 
                        {
                            Debug.Log(thisDist + " Pixel per grid = " + pixelPerGrid);
                        }
                        */
                    }
                }
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
}
