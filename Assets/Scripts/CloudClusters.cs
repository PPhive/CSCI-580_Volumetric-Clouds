using UnityEngine;

//This script generates dedicated cloud prefabs with two perlin noises are sandwitched togther.
public class CloudClusters : MonoBehaviour
{
    [SerializeField]
    GameObject spawnNode;

    [SerializeField]
    GameObject cloud_default;
    GameObject currentParticle;

    Texture2D noiseHigh;
    Texture2D noiseLow;
    [SerializeField]
    Texture2D noiseHeight;

    void Start()
    {
        //Sets what particle will be used
        currentParticle = cloud_default;

        noiseHigh = PerlinNoiseGenerator.instance.texture2D;
        noiseLow = PerlinNoiseGenerator.instance.Generate2DPerlinNoiseTexture(512, 3);//If high is above low, then draw a particle there, bigger gap means bigger particle.
        noiseHeight = PerlinNoiseGenerator.instance.Generate2DPerlinNoiseTexture(512, 4); //The height of particles are also randomized but connected so we need another layer of unrelated perlinoise.

        SpawnClouds(currentParticle); //spawnsa a layer of particle.
    }

    void SpawnClouds(GameObject cloudParticle) 
    {
        float center = noiseHigh.width / 2;
        for (int i = 0; i < 512; i += 4)
        {
            for (int j = 0; j < 512; j += 4) //Goes through every 16th pixel (every is too much) 
            {
                if (Random.Range(0.0f, 10f) < 2f) //Every read pixel only draws if random chance met
                {
                    float pixelHigh = noiseHigh.GetPixel(i, j).a + 0.3f;
                    float pixelLow = noiseLow.GetPixel(i, j).r; 
                    if (pixelHigh > pixelLow) // If noise hi is higher than low.
                    {
                        //move spawnnode and spawn particle there
                        spawnNode.transform.localPosition = new Vector3(i - center, noiseHeight.GetPixel(i, j).r * 100 - 50, j - center);
                        GameObject spawnedParticle = Instantiate(cloudParticle, spawnNode.transform.position, Random.rotation);

                        //detemine how big the particle should be, bigger gap, bigger cloud.
                        float thickness = pixelHigh - pixelLow;
                        thickness = Mathf.Clamp(thickness * 50, 5, 50);
                        spawnedParticle.transform.localScale *= thickness;
                    }
                }
            }
        }
    }
}
