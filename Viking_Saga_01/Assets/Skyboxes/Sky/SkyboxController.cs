using UnityEngine;

public class SkyboxRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 0.5f;

    private Material skyboxMaterial;
    private float currentRotation;

    private void Start()
    {
        skyboxMaterial = RenderSettings.skybox;

        if (skyboxMaterial != null)
        {
            currentRotation =
                skyboxMaterial.GetFloat("_Rotation");
        }
    }

    private void Update()
    {
        if (skyboxMaterial == null)
            return;

        currentRotation +=
            rotationSpeed *
            Time.deltaTime;

        skyboxMaterial.SetFloat(
            "_Rotation",
            currentRotation
        );
    }
}