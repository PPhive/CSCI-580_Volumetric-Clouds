using TreeEditor;
using UnityEngine;

//This script generates dedicated cloud prefabs with two perlin noises are sandwitched togther.
public class CloudClusters : MonoBehaviour
{
    [SerializeField]
    GameObject spawnNode;

    [SerializeField]
    GameObject cloud_default;
    [SerializeField]
    GameObject currentParticle;
    [SerializeField]
    float perlinMultiplier = 10;
    [SerializeField]
    float worleyMultiplier = 10;
    [SerializeField]
    float noiseHighShift = -5;
    [SerializeField]
    float gapMultiplier = 1;
    [SerializeField]
    float densityMultiplier = 0.4f;

    Texture2D noiseHigh;
    Texture2D noiseLow;
    Texture2D noiseHeight;

    void Start()
    {
        //Sets what particle will be used
        currentParticle = cloud_default;

        //noiseHigh = PerlinNoiseGenerator.instance.texture2D;

        Texture2D perlinDefault = PerlinNoiseGenerator.instance.texture2D;
        Texture2D worleyDefault = WorleyNoiseGenerator.instance.texture2D;

        noiseHigh = new Texture2D(perlinDefault.width, perlinDefault.height);
        for (int i = 0; i < noiseHigh.width; i++) 
        {
            for (int j = 0; j < noiseHigh.height; j++)
            {
                //float output = perlinDefault.GetPixel(i, j).a * perlinMultiplier + (1 - worleyDefault.GetPixel(i, j).a) * worleyMultiplier + noiseHighShift;
                float output = Mathf.Lerp(perlinDefault.GetPixel(i, j).a, 1f, (1 - worleyDefault.GetPixel(i, j).a)) * perlinMultiplier + noiseHighShift;
                //float output = perlinDefault.GetPixel(i, j).a * 2.0f + 10 * (1 - worleyDefault.GetPixel(i, j).a) - 6f;
                noiseHigh.SetPixel(i, j, new Color(1,1,1,output));
            }
        }

        noiseLow = PerlinNoiseGenerator.instance.Generate2DPerlinNoiseTexture(256, 4);//If high is above low, then draw a particle there, bigger gap means bigger particle.
        noiseHeight = PerlinNoiseGenerator.instance.Generate2DPerlinNoiseTexture(256, 3); //The height of particles are also randomized but connected so we need another layer of unrelated perlinoise.

        SpawnClouds(currentParticle); //spawnsa a layer of particle.
    }

    void SpawnClouds(GameObject cloudParticle) 
    {
        float center = noiseHigh.width / 2;
        for (int i = 0; i < 256; i += 4)
        {
            for (int j = 0; j < 256; j += 4) //Goes through every 16th pixel (every is too much) 
            {
                if (Random.Range(0.0f, 1f) < densityMultiplier) //Every read pixel only draws if random chance met
                {
                    float pixelHigh = noiseHigh.GetPixel(i, j).a;
                    float pixelLow = noiseLow.GetPixel(i, j).r * perlinMultiplier / 1.5f; 
                    if (pixelHigh > pixelLow) // If noise hi is higher than low.
                    {
                        //move spawnnode and spawn particle there
                        float gap = pixelHigh - pixelLow;
                        float height = noiseHeight.GetPixel(i, j).r;
                        spawnNode.transform.localPosition = new Vector3(i - center, height * gap * 20, j - center);
                        GameObject spawnedParticle = Instantiate(cloudParticle, spawnNode.transform.position, transform.rotation);

                        //Pass data to the particle
                        spawnedParticle.GetComponent<CloudParticle>().noiseGap = gap * gapMultiplier; //passes noise gap to particle

                        spawnedParticle.transform.parent = transform;
                    }
                }
            }
        }
    }
}
