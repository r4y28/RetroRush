using UnityEngine;

public class bloodShader : MonoBehaviour
{
    public Material material;

    public float FillAmount = 0;

    // Update is called once per frame
    void Update()
    {
        material.SetFloat("_Fillamount", FillAmount);
    }
}
