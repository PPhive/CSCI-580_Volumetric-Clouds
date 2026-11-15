using UnityEngine;

public class CloudClusters : MonoBehaviour
{
    [SerializeField]
    GameObject spawnNode;

    [SerializeField]
    GameObject cloud;
    GameObject currentParticle;

    Texture2D noiseHigh;
    Texture2D noiseLow;
    [SerializeField]
    Texture2D noiseHeight;

    void Start()
    {
        noiseHigh = PerlinNoiseGenerator.instance.texture2D;
        noiseLow = PerlinNoiseGenerator.instance.Generate2DPerlinNoiseTexture(512, 3);
        noiseHeight = PerlinNoiseGenerator.instance.Generate2DPerlinNoiseTexture(512, 4);
        SpawnClouds(cloud);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SpawnClouds(GameObject cloudParticle) 
    {
        float center = noiseHigh.width / 2;
        for (int i = 0; i < 512; i += 4)
        {
            for (int j = 0; j < 512; j += 4)
            {
                if (Random.Range(0.0f, 10f) < 2f)
                {
                    float pixelHigh = noiseHigh.GetPixel(i, j).a + 0.3f;
                    float pixelLow = noiseLow.GetPixel(i, j).r; 
                    if (pixelHigh > pixelLow) 
                    {
                        spawnNode.transform.localPosition = new Vector3(i - center, 0, j - center);
                        GameObject spawnedParticle = Instantiate(cloudParticle, spawnNode.transform.position, Random.rotation);
                        float thickness = pixelHigh - pixelLow;
                        thickness = Mathf.Clamp(thickness * 50, 5, 50);
                        spawnedParticle.transform.localPosition += new Vector3(0,noiseHeight.GetPixel(i,j).r * 100 - 50, 0);
                        spawnedParticle.transform.localScale *= thickness;
                    }
                }
            }
        }
    }
}
