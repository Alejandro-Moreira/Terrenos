using UnityEngine;

public class SetupSceneVisible : MonoBehaviour
{
    [Header("Configuración de la Escena")]
    [SerializeField] private bool createTerrainOnStart = true;
    [SerializeField] private bool createObjectsOnStart = true;
    [SerializeField] private bool setupLightingOnStart = true;
    [SerializeField] private bool setupCameraOnStart = true;

    [Header("Configuración del Terreno")]
    [SerializeField] private int terrainWidth = 300;
    [SerializeField] private int terrainHeight = 300;
    [SerializeField] private int terrainResolution = 512;

    [Header("Elementos Naturales")]
    [SerializeField] private int numberOfTrees = 50;
    [SerializeField] private bool createMountains = true;
    [SerializeField] private bool createForest = true;

    private Terrain currentTerrain;

    void Start()
    {
        if (createTerrainOnStart)
            CreateTerrain();

        // Esperar un frame para que el terreno se inicialice completamente
        StartCoroutine(CreateObjectsAfterTerrain());
    }

    System.Collections.IEnumerator CreateObjectsAfterTerrain()
    {
        yield return new WaitForEndOfFrame();

        // Encontrar el terreno creado
        currentTerrain = FindObjectOfType<Terrain>();

        if (createObjectsOnStart)
        {
            CreateSceneObjects();
            if (createMountains)
                CreateMountains();
            if (createForest)
                CreateTrees();
        }

        if (setupLightingOnStart)
            SetupLighting();

        if (setupCameraOnStart)
            SetupCamera();
    }

    void CreateTerrain()
    {
        // Crear datos del terreno
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = terrainResolution;
        terrainData.size = new Vector3(terrainWidth, 50, terrainHeight); // Aumentamos la altura máxima

        // Generar alturas para topografía variada con montañas
        float[,] heights = new float[terrainResolution, terrainResolution];
        for (int x = 0; x < terrainResolution; x++)
        {
            for (int y = 0; y < terrainResolution; y++)
            {
                float xCoord = (float)x / terrainResolution * 4;
                float yCoord = (float)y / terrainResolution * 4;

                // Base del terreno con Perlin noise
                heights[x, y] = Mathf.PerlinNoise(xCoord, yCoord) * 0.15f;

                // Crear cadena montañosa en el fondo (área norte/este)
                float mountainX = (float)x / terrainResolution;
                float mountainY = (float)y / terrainResolution;

                // Montañas principales en la parte superior del mapa
                if (mountainY > 0.7f)
                {
                    float mountainNoise = Mathf.PerlinNoise(mountainX * 3, mountainY * 3) * 0.6f;
                    float ridgeNoise = Mathf.PerlinNoise(mountainX * 8, mountainY * 8) * 0.2f;
                    heights[x, y] += mountainNoise + ridgeNoise;
                }

                // Colinas medianas en el área central
                if (mountainY > 0.4f && mountainY < 0.7f)
                {
                    float hillNoise = Mathf.PerlinNoise(mountainX * 5, mountainY * 5) * 0.3f;
                    heights[x, y] += hillNoise;
                }

                // Mantener área plana para la ciudad (parte inferior/sur)
                if (mountainY < 0.4f)
                {
                    heights[x, y] = Mathf.Lerp(heights[x, y], 0.05f, 0.7f); // Área más plana para ciudad
                }

                // Añadir algunas colinas específicas cerca de la ciudad
                Vector2 hill1Center = new Vector2(terrainResolution * 0.2f, terrainResolution * 0.3f);
                Vector2 hill2Center = new Vector2(terrainResolution * 0.6f, terrainResolution * 0.25f);
                Vector2 currentPos = new Vector2(x, y);

                float hill1Distance = Vector2.Distance(currentPos, hill1Center);
                if (hill1Distance < terrainResolution * 0.1f)
                {
                    float hill1Height = (1f - hill1Distance / (terrainResolution * 0.1f)) * 0.2f;
                    heights[x, y] += hill1Height;
                }

                float hill2Distance = Vector2.Distance(currentPos, hill2Center);
                if (hill2Distance < terrainResolution * 0.08f)
                {
                    float hill2Height = (1f - hill2Distance / (terrainResolution * 0.08f)) * 0.15f;
                    heights[x, y] += hill2Height;
                }
            }
        }
        terrainData.SetHeights(0, 0, heights);

        // Crear GameObject del terreno
        GameObject terrainGO = Terrain.CreateTerrainGameObject(terrainData);
        terrainGO.name = "Mountain City Terrain";

        // Configurar materiales y texturas del terreno
        Terrain terrain = terrainGO.GetComponent<Terrain>();
        if (terrain != null)
        {
            // Crear material con gradiente de colores según altura
            Material terrainMaterial = new Material(Shader.Find("Standard"));
            terrainMaterial.color = new Color(0.3f, 0.6f, 0.2f); // Verde base
            terrain.materialTemplate = terrainMaterial;
        }

        Debug.Log("Terreno montañoso creado con área urbana");

        // Guardar referencia al terreno
        currentTerrain = terrain;
    }

    // Función para obtener la altura del terreno en una posición específica
    float GetTerrainHeightAtPosition(Vector3 worldPosition)
    {
        if (currentTerrain == null) return 0f;

        Vector3 terrainPosition = worldPosition - currentTerrain.transform.position;
        Vector3 normalizedPosition = new Vector3(
            terrainPosition.x / currentTerrain.terrainData.size.x,
            0,
            terrainPosition.z / currentTerrain.terrainData.size.z
        );

        return currentTerrain.terrainData.GetInterpolatedHeight(normalizedPosition.x, normalizedPosition.z);
    }

    void CreateSceneObjects()
    {
        // ===== ÁREA URBANA (Ciudad en la parte sur/baja del mapa) =====

        // Edificios principales del centro de la ciudad
        CreateCubeOnTerrain("Rascacielos_Central", new Vector3(50, 0, 30), new Vector3(8, 50, 8), new Color(0.6f, 0.6f, 0.7f), "Edificio");
        CreateCubeOnTerrain("Torre_Oficinas_1", new Vector3(40, 0, 40), new Vector3(6, 40, 6), Color.gray, "Edificio");
        CreateCubeOnTerrain("Torre_Oficinas_2", new Vector3(65, 0, 25), new Vector3(5, 36, 5), new Color(0.5f, 0.5f, 0.6f), "Edificio");
        CreateCubeOnTerrain("Edificio_Comercial", new Vector3(30, 0, 35), new Vector3(12, 24, 8), new Color(0.7f, 0.7f, 0.8f), "Edificio");

        // Edificios residenciales medianos
        CreateCubeOnTerrain("Apartamentos_1", new Vector3(20, 0, 20), new Vector3(6, 16, 6), new Color(0.8f, 0.6f, 0.4f), "Residencial");
        CreateCubeOnTerrain("Apartamentos_2", new Vector3(75, 0, 35), new Vector3(6, 16, 6), new Color(0.7f, 0.5f, 0.3f), "Residencial");
        CreateCubeOnTerrain("Apartamentos_3", new Vector3(55, 0, 50), new Vector3(5, 12, 5), new Color(0.9f, 0.7f, 0.5f), "Residencial");

        // Casas suburbanas en las afueras
        CreateCubeOnTerrain("Casa_Suburbana_1", new Vector3(10, 0, 15), new Vector3(4, 6, 4), new Color(0.8f, 0.6f, 0.4f), "Casa");
        CreateCubeOnTerrain("Casa_Suburbana_2", new Vector3(85, 0, 20), new Vector3(4, 6, 4), new Color(0.7f, 0.5f, 0.3f), "Casa");
        CreateCubeOnTerrain("Casa_Suburbana_3", new Vector3(15, 0, 55), new Vector3(4, 6, 4), new Color(0.9f, 0.7f, 0.5f), "Casa");
        CreateCubeOnTerrain("Casa_Suburbana_4", new Vector3(80, 0, 55), new Vector3(4, 6, 4), new Color(0.8f, 0.6f, 0.4f), "Casa");

        // Infraestructura urbana
        CreateCubeOnTerrain("Almacen_Industrial", new Vector3(25, 0, 60), new Vector3(15, 8, 10), new Color(0.4f, 0.4f, 0.4f), "Industrial");
        CreateCubeOnTerrain("Centro_Comercial", new Vector3(60, 0, 60), new Vector3(20, 10, 12), new Color(0.6f, 0.6f, 0.6f), "Comercial");
        CreateCubeOnTerrain("Estacion_Servicio", new Vector3(45, 0, 15), new Vector3(8, 4, 6), Color.red, "Servicio");

        // Elementos decorativos urbanos
        CreateCubeOnTerrain("Monumento_Plaza", new Vector3(50, 0, 45), new Vector3(2, 8, 2), Color.white, "Monumento");
        CreateCubeOnTerrain("Kiosco_Parque", new Vector3(35, 0, 50), new Vector3(3, 4, 3), new Color(0.6f, 0.3f, 0.1f), "Decoracion");

        Debug.Log("Ciudad creada con más de 15 edificios y estructuras");
    }

    void CreateMountains()
    {
        // ===== CADENA MONTAÑOSA (Área norte/alta del mapa) =====

        // Picos principales de montañas usando cubos escalados - estos SÍ pueden estar elevados
        CreateCubeOnTerrain("Pico_Principal", new Vector3(150, 0, 220), new Vector3(20, 40, 20), new Color(0.4f, 0.4f, 0.5f), "Montana");
        CreateCubeOnTerrain("Pico_Secundario_1", new Vector3(120, 0, 200), new Vector3(15, 35, 15), new Color(0.45f, 0.45f, 0.5f), "Montana");
        CreateCubeOnTerrain("Pico_Secundario_2", new Vector3(180, 0, 240), new Vector3(18, 38, 18), new Color(0.4f, 0.4f, 0.48f), "Montana");
        CreateCubeOnTerrain("Pico_Menor_1", new Vector3(100, 0, 180), new Vector3(12, 25, 12), new Color(0.5f, 0.5f, 0.55f), "Montana");
        CreateCubeOnTerrain("Pico_Menor_2", new Vector3(200, 0, 210), new Vector3(14, 30, 14), new Color(0.48f, 0.48f, 0.52f), "Montana");
        CreateCubeOnTerrain("Pico_Menor_3", new Vector3(170, 0, 180), new Vector3(10, 28, 10), new Color(0.46f, 0.46f, 0.51f), "Montana");

        // Colinas de transición entre ciudad y montañas
        CreateCubeOnTerrain("Colina_1", new Vector3(80, 0, 120), new Vector3(25, 8, 25), new Color(0.3f, 0.6f, 0.2f), "Colina");
        CreateCubeOnTerrain("Colina_2", new Vector3(130, 0, 140), new Vector3(30, 10, 30), new Color(0.35f, 0.65f, 0.25f), "Colina");
        CreateCubeOnTerrain("Colina_3", new Vector3(190, 0, 120), new Vector3(28, 9, 28), new Color(0.32f, 0.62f, 0.22f), "Colina");

        // Formaciones rocosas
        CreateCubeOnTerrain("Formacion_Rocosa_1", new Vector3(110, 0, 160), new Vector3(8, 18, 6), new Color(0.3f, 0.3f, 0.4f), "Roca");
        CreateCubeOnTerrain("Formacion_Rocosa_2", new Vector3(160, 0, 190), new Vector3(6, 20, 8), new Color(0.32f, 0.32f, 0.42f), "Roca");
        CreateCubeOnTerrain("Formacion_Rocosa_3", new Vector3(140, 0, 220), new Vector3(7, 16, 7), new Color(0.28f, 0.28f, 0.38f), "Roca");

        Debug.Log("Cadena montañosa creada con múltiples picos y colinas");
    }

    void CreateTrees()
    {
        // ===== BOSQUE Y ÁRBOLES DISPERSOS =====

        // Crear árboles usando cilindros (tronco) y esferas (copa)
        for (int i = 0; i < numberOfTrees; i++)
        {
            Vector3 treePosition;

            // Distribuir árboles en diferentes zonas
            if (i < numberOfTrees * 0.6f) // 60% en zona boscosa (área media)
            {
                treePosition = new Vector3(
                    Random.Range(20f, 280f),
                    0f, // Se ajustará con GetTerrainHeightAtPosition
                    Random.Range(80f, 160f)
                );
            }
            else if (i < numberOfTrees * 0.8f) // 20% en las faldas de montañas
            {
                treePosition = new Vector3(
                    Random.Range(60f, 240f),
                    0f,
                    Random.Range(160f, 200f)
                );
            }
            else // 20% dispersos cerca de la ciudad
            {
                treePosition = new Vector3(
                    Random.Range(10f, 90f),
                    0f,
                    Random.Range(10f, 80f)
                );
            }

            CreateTreeOnTerrain($"Arbol_{i + 1}", treePosition);
        }

        // Crear algunos árboles especiales más grandes
        CreateLargeTreeOnTerrain("Roble_Gigante_1", new Vector3(45, 0, 90));
        CreateLargeTreeOnTerrain("Roble_Gigante_2", new Vector3(120, 0, 130));
        CreateLargeTreeOnTerrain("Pino_Ancestral", new Vector3(200, 0, 170));

        Debug.Log($"Bosque creado con {numberOfTrees + 3} árboles");
    }

    void CreateCubeOnTerrain(string name, Vector3 position, Vector3 scale, Color color, string tag = "")
    {
        // Ajustar posición Y basándose en la altura del terreno
        float terrainHeight = GetTerrainHeightAtPosition(position);
        position.y = terrainHeight + (scale.y * 0.5f); // Centrar el cubo sobre el terreno

        CreateCube(name, position, scale, color, tag);
    }

    void CreateTreeOnTerrain(string name, Vector3 position)
    {
        // Ajustar posición Y basándose en la altura del terreno
        float terrainHeight = GetTerrainHeightAtPosition(position);
        position.y = terrainHeight;

        CreateTree(name, position);
    }

    void CreateLargeTreeOnTerrain(string name, Vector3 position)
    {
        // Ajustar posición Y basándose en la altura del terreno
        float terrainHeight = GetTerrainHeightAtPosition(position);
        position.y = terrainHeight;

        CreateLargeTree(name, position);
    }

    void CreateTree(string name, Vector3 position)
    {
        // Crear objeto padre para el árbol
        GameObject tree = new GameObject(name);
        tree.transform.position = position;
        tree.tag = "Arbol";

        // Crear tronco (cilindro)
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = name + "_Tronco";
        trunk.transform.SetParent(tree.transform);
        trunk.transform.localPosition = Vector3.zero;
        trunk.transform.localScale = new Vector3(0.5f, 3f, 0.5f);

        Material trunkMaterial = new Material(Shader.Find("Standard"));
        trunkMaterial.color = new Color(0.4f, 0.2f, 0.1f); // Marrón
        trunk.GetComponent<Renderer>().material = trunkMaterial;

        // Crear copa (esfera)
        GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        foliage.name = name + "_Copa";
        foliage.transform.SetParent(tree.transform);
        foliage.transform.localPosition = new Vector3(0, 4f, 0);

        float foliageSize = Random.Range(2f, 4f);
        foliage.transform.localScale = Vector3.one * foliageSize;

        Material foliageMaterial = new Material(Shader.Find("Standard"));
        Color[] greenVariations = {
            new Color(0.2f, 0.6f, 0.2f),
            new Color(0.1f, 0.7f, 0.1f),
            new Color(0.3f, 0.5f, 0.2f),
            new Color(0.15f, 0.65f, 0.15f)
        };
        foliageMaterial.color = greenVariations[Random.Range(0, greenVariations.Length)];
        foliage.GetComponent<Renderer>().material = foliageMaterial;
    }

    void CreateLargeTree(string name, Vector3 position)
    {
        GameObject tree = new GameObject(name);
        tree.transform.position = position;
        tree.tag = "ArbolGigante";

        // Tronco más grande
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = name + "_Tronco";
        trunk.transform.SetParent(tree.transform);
        trunk.transform.localPosition = Vector3.zero;
        trunk.transform.localScale = new Vector3(1.5f, 8f, 1.5f);

        Material trunkMaterial = new Material(Shader.Find("Standard"));
        trunkMaterial.color = new Color(0.3f, 0.15f, 0.05f);
        trunk.GetComponent<Renderer>().material = trunkMaterial;

        // Copa más grande y frondosa
        GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        foliage.name = name + "_Copa";
        foliage.transform.SetParent(tree.transform);
        foliage.transform.localPosition = new Vector3(0, 10f, 0);
        foliage.transform.localScale = new Vector3(8f, 6f, 8f);

        Material foliageMaterial = new Material(Shader.Find("Standard"));
        foliageMaterial.color = new Color(0.1f, 0.5f, 0.1f);
        foliage.GetComponent<Renderer>().material = foliageMaterial;

        // Agregar algunas ramas adicionales
        for (int i = 0; i < 3; i++)
        {
            GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            branch.name = name + $"_Rama_{i}";
            branch.transform.SetParent(tree.transform);
            branch.transform.localPosition = new Vector3(
                Random.Range(-3f, 3f),
                Random.Range(8f, 12f),
                Random.Range(-3f, 3f)
            );
            branch.transform.localScale = Vector3.one * Random.Range(3f, 5f);
            branch.GetComponent<Renderer>().material = foliageMaterial;
        }
    }

    void CreateCube(string name, Vector3 position, Vector3 scale, Color color, string tag = "")
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = scale;

        // Crear material personalizado
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;

        // Añadir algo de brillo metálico para variedad
        if (color == Color.gray || color == Color.white)
        {
            mat.SetFloat("_Metallic", 0.3f);
            mat.SetFloat("_Smoothness", 0.6f);
        }
        else
        {
            mat.SetFloat("_Metallic", 0.1f);
            mat.SetFloat("_Smoothness", 0.3f);
        }

        cube.GetComponent<Renderer>().material = mat;

        // Asignar tag si se proporciona
        if (!string.IsNullOrEmpty(tag))
        {
            try
            {
                cube.tag = tag;
            }
            catch
            {
                Debug.LogWarning($"Tag '{tag}' no existe. Usando tag por defecto.");
            }
        }

        // Añadir collider para interacción
        if (cube.GetComponent<Collider>() == null)
        {
            cube.AddComponent<BoxCollider>();
        }
    }

    void SetupLighting()
    {
        // Crear luz direccional principal (sol)
        GameObject sunLight = new GameObject("Sun Light");
        Light sunLightComp = sunLight.AddComponent<Light>();
        sunLightComp.type = LightType.Directional;
        sunLightComp.intensity = 1.2f;
        sunLightComp.color = new Color(1f, 0.95f, 0.8f); // Luz cálida
        sunLight.transform.rotation = Quaternion.Euler(45, -30, 0);

        // Crear luz ambiental suave
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.5f, 0.7f, 1f);
        RenderSettings.ambientEquatorColor = new Color(0.4f, 0.4f, 0.6f);
        RenderSettings.ambientGroundColor = new Color(0.2f, 0.3f, 0.3f);

        // Crear algunas luces puntuales para ambiente nocturno
        CreatePointLight("Luz_Edificio_1", new Vector3(20, 15, 20), Color.yellow, 8f, 15f);
        CreatePointLight("Luz_Edificio_2", new Vector3(40, 12, 25), Color.cyan, 6f, 12f);
        CreatePointLight("Luz_Torre", new Vector3(90, 20, 90), Color.red, 10f, 25f);

        Debug.Log("Sistema de iluminación configurado");
    }

    void CreatePointLight(string name, Vector3 position, Color color, float intensity, float range)
    {
        GameObject lightGO = new GameObject(name);
        Light lightComp = lightGO.AddComponent<Light>();
        lightComp.type = LightType.Point;
        lightComp.color = color;
        lightComp.intensity = intensity;
        lightComp.range = range;
        lightGO.transform.position = position;
    }

    void SetupCamera()
    {
        // Verificar si ya existe una cámara principal
        Camera existingCamera = Camera.main;
        if (existingCamera != null)
        {
            // Posicionar la cámara para vista panorámica de ciudad y montañas
            existingCamera.transform.position = new Vector3(150, 60, 0);
            existingCamera.transform.LookAt(new Vector3(150, 20, 150));
        }
        else
        {
            // Crear nueva cámara si no existe
            GameObject cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";
            Camera cam = cameraGO.AddComponent<Camera>();
            cam.transform.position = new Vector3(150, 60, 0);
            cam.transform.LookAt(new Vector3(150, 20, 150));

            // Configuraciones adicionales de la cámara
            cam.fieldOfView = 75f; // Campo de visión más amplio para panorámica
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 2000f; // Aumentamos para ver las montañas lejanas
        }

        Debug.Log("Cámara configurada para vista panorámica ciudad-montaña");
    }

    // Método público para regenerar la escena desde el inspector
    [ContextMenu("Regenerar Escena")]
    public void RegenerateScene()
    {
        // Limpiar objetos existentes
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Edificio") || obj.name.Contains("Casa") ||
                obj.name.Contains("Apartamentos") || obj.name.Contains("Pico") ||
                obj.name.Contains("Arbol") || obj.name.Contains("Colina") ||
                obj.name.Contains("Mountain City Terrain") || obj.name.Contains("Generated Terrain"))
            {
                DestroyImmediate(obj);
            }
        }

        // Recrear la escena
        Start();
    }
}