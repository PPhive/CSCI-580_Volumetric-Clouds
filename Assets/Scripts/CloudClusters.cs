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

    Texture2D noiseHigh;
    Texture2D noiseLow;
    Texture2D noiseHeight;

    void Start()
    {
        //Sets what particle will be used
        currentParticle = cloud_default;

        noiseHigh = PerlinNoiseGenerator.instance.texture2D;
        noiseLow = PerlinNoiseGenerator.instance.Generate2DPerlinNoiseTexture(512, 5);//If high is above low, then draw a particle there, bigger gap means bigger particle.
        noiseHeight = PerlinNoiseGenerator.instance.Generate2DPerlinNoiseTexture(512, 3); //The height of particles are also randomized but connected so we need another layer of unrelated perlinoise.

        SpawnClouds(currentParticle); //spawnsa a layer of particle.
    }

    void SpawnClouds(GameObject cloudParticle) 
    {
        float center = noiseHigh.width / 2;
        for (int i = 0; i < 512; i += 4)
        {
            for (int j = 0; j < 512; j += 4) //Goes through every 16th pixel (every is too much) 
            {
                if (Random.Range(0.0f, 10f) < 3f) //Every read pixel only draws if random chance met
                {
                    float pixelHigh = noiseHigh.GetPixel(i, j).a * 3 + 0.2f;
                    float pixelLow = noiseLow.GetPixel(i, j).r; 
                    if (pixelHigh > pixelLow) // If noise hi is higher than low.
                    {
                        //move spawnnode and spawn particle there
                        float height = noiseHeight.GetPixel(i, j).r * 100 - 50;
                        spawnNode.transform.localPosition = new Vector3(i - center, height * 0.3f, j - center);
                        GameObject spawnedParticle = Instantiate(cloudParticle, spawnNode.transform.position, Random.rotation);


                        //Pass data to the particle
                        spawnedParticle.GetComponent<CloudParticle>().noiseGap = pixelHigh; //passes noise gap to particle

                        //spawnedParticle.transform.parent = transform;
                    }
                }
            }
        }
    }
}
