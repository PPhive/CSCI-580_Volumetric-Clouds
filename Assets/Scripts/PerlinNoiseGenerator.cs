using UnityEngine;

public class PerlinNoiseGenerator : MonoBehaviour
{
    public static PerlinNoiseGenerator instance; 

    [SerializeField]
    int resolution = 256;

    [SerializeField]
    int perlinPowerA = 4;
    [SerializeField, Range(0f, 1f)]
    private float AlphaA = 0.5f;

    [SerializeField]
    int perlinPowerB = 4;
    [SerializeField, Range(0f, 1f)]
    private float AlphaB = 0.5f;

    [SerializeField]
    int perlinPowerC = 4;
    [SerializeField, Range(0f, 1f)]
    private float AlphaC = 0.5f;


    public Texture2D texture2D;
    public Texture2D texture2Da;
    public Texture2D texture2Db;
    public Texture2D texture2Dc;

    private void Awake()
    {
        instance = this;

        texture2D = new Texture2D(resolution, resolution);
        texture2Da = Generate2DPerlinNoiseTexture(texture2D.width, perlinPowerA);
        texture2Db = Generate2DPerlinNoiseTexture(texture2D.width, perlinPowerB);
        texture2Dc = Generate2DPerlinNoiseTexture(texture2D.width, perlinPowerC);

        for (int i = 0; i < texture2D.width; i++)
        {
            for (int j = 0; j < texture2D.height; j++)
            {
                Color colorA = texture2Da.GetPixel(i, j);
                Color colorB = texture2Db.GetPixel(i, j);
                Color colorC = texture2Dc.GetPixel(i, j);
                float combinedBrightness = (colorA.r * AlphaA + colorB.r * AlphaB + colorC.r * AlphaC); //Average the brightness of the three textures
                combinedBrightness = Mathf.Clamp01((combinedBrightness - 0.5f) * 5f);//Clamp and multiply to make certain gray areas show while clipper lower ones
                texture2D.SetPixel(i, j, new Color(1, 1, 1, combinedBrightness));
            }
        }


        //texture2D = texture2Da;
        texture2D.Apply(); //Uploades to gpu memory
        if (GetComponent<Renderer>() != null)
        {
            GetComponent<Renderer>().material.SetTexture("_BaseMap", texture2D); //Sets the quad's texture to the generated texture
        }
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public Texture2D Generate2DPerlinNoiseTexture(int resolution, int perlinPower)
    {
        Texture2D texture = new Texture2D(resolution, resolution);

        //Inital checks for perlinPower, if it's too low or too high, return the texture without generating noise
        if (perlinPower < 1)
        {
            Debug.Log("Perlin power must be at least 1");
            return texture;
        }
        else if (perlinPower > 10) 
        {
            Debug.Log("Perlin power too big, I don't think ur pc can handle above 10");
            return texture;
        }
        int VectorDimention = Mathf.FloorToInt(Mathf.Pow(2, perlinPower));
        Vector2[,] vectors2D = new Vector2[VectorDimention + 1, VectorDimention + 1]; //Number of vectors in one dimension, +1 so it wraps around



        //Resize the texture if it's too small or not divisible by 2^perlinPower
        if (texture.width < VectorDimention) 
        {
            Debug.Log("Texture too small, expanding to meet perlinPower");
            texture = new Texture2D(VectorDimention, VectorDimention);
        }
        if (texture.width % VectorDimention > 0)
        {
            Debug.Log("Width not divisible by 2^perlin power, resizing");
            int newSize = Mathf.CeilToInt(Mathf.Max(texture.width, VectorDimention * 2) / (float)VectorDimention) * VectorDimention;
            texture = new Texture2D(newSize, newSize);
        }

        //Generate the vector grid
        for (int i = 0; i < vectors2D.GetLength(0); i++)
        {
            for (int j = 0; j < vectors2D.GetLength(1); j++)
            {
                //Generate a random unit vector
                float theta = Random.Range(0, Mathf.PI * 2f); //Select random theta
                Vector2 vector = new Vector2();
                vector = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)); //Convert to unit vector
                vectors2D[i, j] = vector; //point vector in that direction

                //Wrapping if this is last row or column, make them the same vector as the other side
                if (i == vectors2D.GetLength(0) - 1)
                {
                    vectors2D[i, j] = vectors2D[0, j]; //Wrap around to the first row
                }
                if (j == vectors2D.GetLength(1) - 1)
                {
                    vectors2D[i, j] = vectors2D[i, 0]; //Wrap around to the first column
                }
            }
        }

        //Generate the texture according to vector field
        int pixelPerVector = texture.width / (vectors2D.GetLength(0) - 1);

        for (int i = 0; i < texture.width; i++)
        {
            for (int j = 0; j < texture.height; j++)
            {
                int gridX = Mathf.FloorToInt(i / pixelPerVector); //Index of the vector grid cell that this pixel belongs to
                int gridY = Mathf.FloorToInt(j / pixelPerVector); //Index of the vector grid cell that this pixel belongs to

                float dot0 = Vector2.Dot(vectors2D[gridX, gridY], (new Vector2(i, j) - new Vector2(gridX, gridY) * pixelPerVector) / pixelPerVector);
                float dot1 = Vector2.Dot(vectors2D[gridX + 1, gridY], (new Vector2(i, j) - new Vector2(gridX + 1, gridY) * pixelPerVector) / pixelPerVector);
                float dot2 = Vector2.Dot(vectors2D[gridX, gridY + 1], (new Vector2(i, j) - new Vector2(gridX, gridY + 1) * pixelPerVector) / pixelPerVector);
                float dot3 = Vector2.Dot(vectors2D[gridX + 1, gridY + 1], (new Vector2(i, j) - new Vector2(gridX + 1, gridY + 1) * pixelPerVector) / pixelPerVector);

                float localX = (i - gridX * pixelPerVector) / (float)pixelPerVector;
                float localY = (j - gridY * pixelPerVector) / (float)pixelPerVector;
                float smoothXCurve = 3f * localX * localX - 2f * localX * localX * localX;
                float smoothYCurve = 3f * localY * localY - 2f * localY * localY * localY;

                float lerp01 = Mathf.Lerp(dot0, dot1, smoothXCurve);
                float lerp23 = Mathf.Lerp(dot2, dot3, smoothXCurve);
                float finalLerp = Mathf.Lerp(lerp01, lerp23, smoothYCurve);
                float brightness = ((finalLerp + 1) / 2); // Normalize to [0, 1] range

                /*
                if (i == 30 && j == 30)
                {
                    Debug.Log("pixelPerVectorX = " + pixelPerVector);
                    Debug.Log(gridX);
                }
                */

                texture.SetPixel(i, j, new Color(brightness, brightness, brightness));
            }
        }

        texture.wrapMode = TextureWrapMode.Repeat;
        return texture;
    }
}
