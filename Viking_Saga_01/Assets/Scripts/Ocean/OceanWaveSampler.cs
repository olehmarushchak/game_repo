using UnityEngine;

public class OceanWaveSampler : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private Renderer waterRenderer;
    [SerializeField] private Material waterMaterial;


    // =========================================================
    // WAVE 1 - VALUES READ FROM MATERIAL
    // =========================================================

    private float wave1Length;
    private float wave1Height;
    private float wave1Speed;
    private Vector3 wave1Direction;
    private float wave1Sharpness;


    // =========================================================
    // WAVE 2 - VALUES READ FROM MATERIAL
    // =========================================================

    private float wave2Length;
    private float wave2Height;
    private float wave2Speed;
    private Vector3 wave2Direction;
    private float wave2Sharpness;


    // =========================================================
    // SHADER PROPERTY IDS
    // =========================================================

    private int wave1LengthID;
    private int wave1HeightID;
    private int wave1SpeedID;
    private int wave1DirectionID;
    private int wave1SharpnessID;

    private int wave2LengthID;
    private int wave2HeightID;
    private int wave2SpeedID;
    private int wave2DirectionID;
    private int wave2SharpnessID;


    private bool propertiesFound;


    // Same constants used by the Gerstner subgraph.
    private const float GRAVITY = 9.8f;
    private const float TAU = Mathf.PI * 2f;


    // =========================================================
    // PUBLIC WAVE SAMPLE
    // =========================================================

    public struct WaveSample
    {
        // World-space Y position of the water surface.
        public float height;

        // Combined horizontal direction of the waves.
        public Vector3 direction;

        // 0 = trough
        // 0.5 = middle
        // 1 = crest
        public float crest;

        // Approximate strength of the combined waves
        // at this position.
        public float intensity;
    }


    // Internal information about one Gerstner wave.
    private struct WaveData
    {
        public float height;
        public float crest;
        public float intensity;
        public Vector3 direction;
    }


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (waterRenderer == null)
        {
            waterRenderer = GetComponent<Renderer>();
        }

        if (waterMaterial == null && waterRenderer != null)
        {
            // sharedMaterial is intentional:
            // we want to read the actual material used by the ocean,
            // without generating a runtime copy.
            waterMaterial = waterRenderer.sharedMaterial;
        }

        ResolveShaderProperties();
        ReadMaterialValues();
    }


    private void Update()
    {
        /*
         * We read the material every frame so changing:
         *
         * Wave Height
         * Wave Length
         * Wave Speed
         * Wave Direction
         * Wave Sharpness
         *
         * in the Material Inspector automatically changes
         * all gameplay systems using this sampler.
         */
        ReadMaterialValues();
    }


    // =========================================================
    // SHADER PROPERTY LOOKUP
    // =========================================================

    private void ResolveShaderProperties()
    {
        if (waterMaterial == null)
        {
            Debug.LogError(
                "OceanWaveSampler: Water material was not found."
            );

            return;
        }

        Shader shader = waterMaterial.shader;


        // -------------------------
        // Wave 1
        // -------------------------

        wave1LengthID =
            FindProperty(
                shader,
                "1st_Wave_Length"
            );

        wave1HeightID =
            FindProperty(
                shader,
                "1st_Wave_Height"
            );

        wave1SpeedID =
            FindProperty(
                shader,
                "1st_Wave_Speed"
            );

        wave1DirectionID =
            FindProperty(
                shader,
                "1st_Wave_Direction"
            );

        wave1SharpnessID =
            FindProperty(
                shader,
                "1st_Wave_Sharpness"
            );


        // -------------------------
        // Wave 2
        // -------------------------

        wave2LengthID =
            FindProperty(
                shader,
                "2nd_Wave_Length"
            );

        wave2HeightID =
            FindProperty(
                shader,
                "2nd_Wave_Height"
            );

        wave2SpeedID =
            FindProperty(
                shader,
                "2nd_Wave_Speed"
            );

        wave2DirectionID =
            FindProperty(
                shader,
                "2nd_Wave_Direction"
            );

        wave2SharpnessID =
            FindProperty(
                shader,
                "2nd_Wave_Sharpness"
            );


        propertiesFound =
            wave1LengthID != -1 &&
            wave1HeightID != -1 &&
            wave1SpeedID != -1 &&
            wave1DirectionID != -1 &&
            wave1SharpnessID != -1 &&

            wave2LengthID != -1 &&
            wave2HeightID != -1 &&
            wave2SpeedID != -1 &&
            wave2DirectionID != -1 &&
            wave2SharpnessID != -1;


        if (!propertiesFound)
        {
            Debug.LogError(
                "OceanWaveSampler: One or more wave properties were not found."
            );
        }
    }


    private int FindProperty(
        Shader shader,
        string displayName
    )
    {
        int propertyCount =
            shader.GetPropertyCount();


        for (int i = 0; i < propertyCount; i++)
        {
            string propertyDescription =
                shader.GetPropertyDescription(i);


            if (propertyDescription != displayName)
            {
                continue;
            }


            string propertyName =
                shader.GetPropertyName(i);


            Debug.Log(
                displayName +
                " -> shader property: " +
                propertyName
            );


            return Shader.PropertyToID(
                propertyName
            );
        }


        Debug.LogWarning(
            "OceanWaveSampler: Property not found: " +
            displayName
        );


        return -1;
    }


    // =========================================================
    // READ MATERIAL
    // =========================================================

    private void ReadMaterialValues()
    {
        if (!propertiesFound || waterMaterial == null)
        {
            return;
        }


        // -------------------------
        // Wave 1
        // -------------------------

        wave1Length =
            waterMaterial.GetFloat(
                wave1LengthID
            );

        wave1Height =
            waterMaterial.GetFloat(
                wave1HeightID
            );

        wave1Speed =
            waterMaterial.GetFloat(
                wave1SpeedID
            );

        wave1Direction =
            waterMaterial.GetVector(
                wave1DirectionID
            );

        wave1Sharpness =
            waterMaterial.GetFloat(
                wave1SharpnessID
            );


        // -------------------------
        // Wave 2
        // -------------------------

        wave2Length =
            waterMaterial.GetFloat(
                wave2LengthID
            );

        wave2Height =
            waterMaterial.GetFloat(
                wave2HeightID
            );

        wave2Speed =
            waterMaterial.GetFloat(
                wave2SpeedID
            );

        wave2Direction =
            waterMaterial.GetVector(
                wave2DirectionID
            );

        wave2Sharpness =
            waterMaterial.GetFloat(
                wave2SharpnessID
            );
    }


    // =========================================================
    // PUBLIC API
    // =========================================================

    public WaveSample SampleWave(
        Vector3 worldPosition
    )
    {
        if (!propertiesFound)
        {
            return new WaveSample
            {
                height = transform.position.y,
                direction = Vector3.zero,
                crest = 0f,
                intensity = 0f
            };
        }


        WaveData wave1 =
            CalculateWaveData(
                worldPosition,
                wave1Length,
                wave1Height,
                wave1Speed,
                wave1Direction,
                wave1Sharpness
            );


        WaveData wave2 =
            CalculateWaveData(
                worldPosition,
                wave2Length,
                wave2Height,
                wave2Speed,
                wave2Direction,
                wave2Sharpness
            );


        // -----------------------------------------------------
        // HEIGHT
        // -----------------------------------------------------

        float combinedHeight =
            wave1.height +
            wave2.height;


        // -----------------------------------------------------
        // DIRECTION
        //
        // Stronger wave contributes more to the final direction.
        // -----------------------------------------------------

        Vector3 combinedDirection =
            wave1.direction *
            wave1.intensity
            +
            wave2.direction *
            wave2.intensity;


        if (combinedDirection.sqrMagnitude > 0.0001f)
        {
            combinedDirection.Normalize();
        }


        // -----------------------------------------------------
        // CREST
        //
        // Weighted by the wave amplitudes.
        // -----------------------------------------------------

        float totalWeight =
            wave1.intensity +
            wave2.intensity;


        float combinedCrest = 0f;


        if (totalWeight > 0.0001f)
        {
            combinedCrest =
                (
                    wave1.crest *
                    wave1.intensity
                    +
                    wave2.crest *
                    wave2.intensity
                )
                /
                totalWeight;
        }


        combinedCrest =
            Mathf.Clamp01(
                combinedCrest
            );


        // -----------------------------------------------------
        // INTENSITY
        // -----------------------------------------------------

        float combinedIntensity =
            wave1.intensity +
            wave2.intensity;


        return new WaveSample
        {
            height =
                transform.position.y +
                combinedHeight,

            direction =
                combinedDirection,

            crest =
                combinedCrest,

            intensity =
                combinedIntensity
        };
    }


    public float GetWaveHeight(
        Vector3 worldPosition
    )
    {
        return SampleWave(
            worldPosition
        ).height;
    }


    // =========================================================
    // GERSTNER SAMPLE
    // =========================================================

    private WaveData CalculateWaveData(
        Vector3 worldPosition,
        float wavelength,
        float height,
        float speed,
        Vector3 direction,
        float sharpness
    )
    {
        if (
            wavelength <= 0.001f ||
            Mathf.Abs(height) <= 0.00001f
        )
        {
            return new WaveData
            {
                height = 0f,
                crest = 0f,
                intensity = 0f,
                direction = Vector3.zero
            };
        }


        // -----------------------------------------------------
        // k = TAU / wavelength
        // Same as the Gerstner subgraph.
        // -----------------------------------------------------

        float k =
            TAU / wavelength;


        // -----------------------------------------------------
        // Direction
        //
        // Shader operates in world space.
        // We only need X/Z for the ocean surface.
        // -----------------------------------------------------

        Vector2 dir2D =
            new Vector2(
                direction.x,
                direction.z
            );


        if (dir2D.sqrMagnitude <= 0.0001f)
        {
            dir2D =
                Vector2.right;
        }


        dir2D.Normalize();


        Vector3 worldDirection =
            new Vector3(
                dir2D.x,
                0f,
                dir2D.y
            );


        // -----------------------------------------------------
        // Spatial phase:
        //
        // dot(worldPosition, direction * k)
        // -----------------------------------------------------

        float spatialPhase =
            worldPosition.x *
            dir2D.x *
            k
            +
            worldPosition.z *
            dir2D.y *
            k;


        // -----------------------------------------------------
        // Gerstner angular speed:
        //
        // sqrt(g * k)
        // -----------------------------------------------------

        float angularSpeed =
            Mathf.Sqrt(
                GRAVITY * k
            );


        // -----------------------------------------------------
        // Time phase
        // -----------------------------------------------------

        float timePhase =
            angularSpeed *
            Time.time *
            speed;


        // Same subtraction seen in the shader graph.
        float phase =
            spatialPhase -
            timePhase;


        float sinValue =
            Mathf.Sin(
                phase
            );


        // -----------------------------------------------------
        // Vertical displacement
        // -----------------------------------------------------

        float verticalDisplacement =
            sinValue *
            height;


        // -----------------------------------------------------
        // CREST
        //
        // -1 -> 0
        //  0 -> 0.5
        // +1 -> 1
        // -----------------------------------------------------

        float crest =
            sinValue *
            0.5f +
            0.5f;


        // -----------------------------------------------------
        // INTENSITY
        //
        // We use amplitude as the main gameplay measure.
        //
        // Sharpness is included slightly because it determines
        // how aggressive the Gerstner shape becomes.
        //
        // This is a GAMEPLAY value, not a full fluid simulation.
        // -----------------------------------------------------

        float sharpnessContribution =
            Mathf.Lerp(
                1f,
                1.5f,
                Mathf.Clamp01(
                    Mathf.Abs(sharpness)
                )
            );


        float intensity =
            Mathf.Abs(height) *
            sharpnessContribution;


        return new WaveData
        {
            height =
                verticalDisplacement,

            crest =
                crest,

            intensity =
                intensity,

            direction =
                worldDirection
        };
    }


    // =========================================================
    // OPTIONAL DEBUG ACCESSORS
    // =========================================================

    public bool HasValidMaterial()
    {
        return
            propertiesFound &&
            waterMaterial != null;
    }


    public Material GetWaterMaterial()
    {
        return waterMaterial;
    }
}