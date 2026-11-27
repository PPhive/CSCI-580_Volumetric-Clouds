using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;

public class CloudParticle_PlaceHolder : MonoBehaviour
{
    [SerializeField]
    GameObject particlePrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CloudParticle particle = GetComponent<CloudParticle>();
        if (particle != null)
        {
            if (particle.noiseGap < 0.07f)
            {
                Destroy(gameObject);
            }

            transform.rotation = Random.rotation;

            float thickness = particle.noiseGap;
            float scale = Mathf.Clamp(thickness * 30, 4, 25);
            transform.localScale = Vector3.one * scale;
            transform.position += new Vector3 (0, transform.localScale.y / 2, 0);

            if (thickness > 0.5f) 
            {
                GameObject spawnedParticle = Instantiate(particlePrefab, transform.position + Vector3.up * (scale * 0.48f), new Quaternion());
                //Pass data to the particle
                spawnedParticle.GetComponent<CloudParticle>().noiseGap = thickness - 0.2f; //passes noise gap to particle
                spawnedParticle.transform.SetParent(transform.parent,true);
            }


            /*
            float thickness = particle.noiseGap;
            thickness = Mathf.Clamp(thickness * 30, 2, 60);
            transform.localScale *= 10;
            transform.position += new Vector3(0, thickness, 0);
            */        
        }
    }
}
