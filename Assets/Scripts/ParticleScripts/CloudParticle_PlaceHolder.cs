using UnityEngine;
using UnityEngine.SocialPlatforms;

public class CloudParticle_PlaceHolder : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CloudParticle particle = GetComponent<CloudParticle>();
        if (particle != null)
        {
            float thickness = particle.noiseGap;
            thickness = Mathf.Clamp(thickness * 10 - 1, 1, 30);
            transform.localScale *= thickness;
            transform.position += new Vector3 (0, transform.localScale.y / 2, 0);
        }
    }
}
