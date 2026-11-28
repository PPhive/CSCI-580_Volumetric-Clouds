using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;


[RequireComponent(typeof(Renderer))]
public class CloudParticle_Fancy : MonoBehaviour
{
    // Cache the shader property ID
    static readonly int DensityTexID = Shader.PropertyToID("_DensityTexture");

    Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();

        WorleyNoiseGenerator gen = WorleyNoiseGenerator.instance;
        if (gen == null || gen.texture3D == null)
        {
            Debug.LogWarning("CloudParticle_Fancy: no WorleyNoiseGenerator");
            return;
        }

        // Because the shader samples far outside 0-1 so the texture must repeat instead of clamping at its edges
        gen.texture3D.wrapMode = TextureWrapMode.Repeat;

        // rend.material makes a private copy of the material for this object so the original shared RaymarchCloud.mat asset on disk would not be modify

        // use one big container for Fancy cuz original pocket method would result in blocky effect
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        rend.GetPropertyBlock(block);
        block.SetTexture(DensityTexID, gen.texture3D);
        rend.SetPropertyBlock(block);
    }
}
