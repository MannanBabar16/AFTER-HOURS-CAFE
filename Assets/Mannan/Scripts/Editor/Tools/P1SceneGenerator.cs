using System.IO;
using Mannan.Core.Logging;
using Mannan.Interaction;
using Mannan.Player;
using Mannan.UI;
using Mannan.World;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Mannan.Editor.Tools
{
    public static class P1SceneGenerator
    {
        private const string ScenePath = "Assets/Mannan/Scenes/P1_CameraInteractionTest.unity";
        private const string PrefabPath = "Assets/Mannan/Prefabs/Player_Prototype.prefab";
        private const string ArtDir = "Assets/Mannan/Art";

        [MenuItem("Mannan/After Hours Café/Setup P1 Test Scene", priority = 10)]
        public static void GenerateP1Scene()
        {
            CafeLogger.Log("P1Generator", "Starting generation of P1 Camera & Interaction Test Scene...");

            // Ensure directories exist
            if (!AssetDatabase.IsValidFolder("Assets/Mannan/Scenes")) AssetDatabase.CreateFolder("Assets/Mannan", "Scenes");
            if (!AssetDatabase.IsValidFolder("Assets/Mannan/Prefabs")) AssetDatabase.CreateFolder("Assets/Mannan", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Mannan/Art")) AssetDatabase.CreateFolder("Assets/Mannan", "Art");

            // Create prototype materials
            Material floorMat = GetOrCreateMaterial("Floor_Prototype_Mat", new Color(0.24f, 0.20f, 0.17f));
            Material counterMat = GetOrCreateMaterial("Counter_Prototype_Mat", new Color(0.38f, 0.28f, 0.20f));
            Material playerMat = GetOrCreateMaterial("Player_Prototype_Mat", new Color(0.85f, 0.65f, 0.40f)); // warm caramel
            Material visorMat = GetOrCreateMaterial("Visor_Prototype_Mat", new Color(0.12f, 0.12f, 0.15f));
            Material machineMat = GetOrCreateMaterial("Machine_Prototype_Mat", new Color(0.70f, 0.25f, 0.22f)); // retro red espresso machine
            Material grinderMat = GetOrCreateMaterial("Grinder_Prototype_Mat", new Color(0.45f, 0.45f, 0.48f));

            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Directional Light
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1.0f, 0.95f, 0.88f);
            light.intensity = 1.2f;
            lightGo.transform.rotation = Quaternion.Euler(45f, -35f, 0f);

            // 2. Floor
            var floorGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floorGo.name = "Floor_Diorama";
            floorGo.transform.position = new Vector3(0f, -0.25f, 0f);
            floorGo.transform.localScale = new Vector3(14f, 0.5f, 14f);
            if (floorMat != null) floorGo.GetComponent<Renderer>().sharedMaterial = floorMat;

            // 3. Obstacles (Counter & Pillar)
            var counterGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counterGo.name = "Obstacle_Counter";
            counterGo.transform.position = new Vector3(-2.2f, 0.5f, 1.5f);
            counterGo.transform.localScale = new Vector3(3.6f, 1.0f, 0.85f);
            if (counterMat != null) counterGo.GetComponent<Renderer>().sharedMaterial = counterMat;

            var pillarGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillarGo.name = "Obstacle_Pillar";
            pillarGo.transform.position = new Vector3(3.5f, 1.5f, -2.0f);
            pillarGo.transform.localScale = new Vector3(0.8f, 3.0f, 0.8f);
            if (counterMat != null) pillarGo.GetComponent<Renderer>().sharedMaterial = counterMat;

            // 4. Generic Interactables
            // 4a. Pastry Display on Counter
            var pastryGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pastryGo.name = "Interactable_PastryDisplay";
            pastryGo.transform.position = new Vector3(-1.4f, 1.35f, 1.5f);
            pastryGo.transform.localScale = new Vector3(0.7f, 0.6f, 0.55f);
            var pastryInteractable = pastryGo.AddComponent<InteractableBase>();
            SetField(pastryInteractable, "promptText", "Pastry Case");
            SetField(pastryInteractable, "actionName", "Inspect");
            if (floorMat != null) pastryGo.GetComponent<Renderer>().sharedMaterial = floorMat;

            // 4b. Coffee Grinder on Counter
            var grinderGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            grinderGo.name = "Interactable_ManualGrinder";
            grinderGo.transform.position = new Vector3(-3.2f, 1.35f, 1.5f);
            grinderGo.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            var grinderInteractable = grinderGo.AddComponent<InteractableBase>();
            SetField(grinderInteractable, "promptText", "Coffee Grinder");
            SetField(grinderInteractable, "actionName", "Examine");
            if (grinderMat != null) grinderGo.GetComponent<Renderer>().sharedMaterial = grinderMat;

            // 5. Workstation Interactable with Contextual Camera Anchor
            var workstationGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            workstationGo.name = "Workstation_EspressoBar";
            workstationGo.transform.position = new Vector3(1.8f, 0.5f, 2.5f);
            workstationGo.transform.localScale = new Vector3(1.4f, 1.0f, 0.9f);
            if (machineMat != null) workstationGo.GetComponent<Renderer>().sharedMaterial = machineMat;

            // Camera anchor child: positioned close-up, elevated, angled at workstation
            var anchorGo = new GameObject("CameraAnchor_Espresso");
            anchorGo.transform.SetParent(workstationGo.transform);
            anchorGo.transform.position = new Vector3(1.8f, 1.45f, 1.3f);
            anchorGo.transform.rotation = Quaternion.Euler(22f, 0f, 0f);

            var workstation = workstationGo.AddComponent<WorkstationInteractable>();
            SetField(workstation, "promptText", "Espresso Machine");
            SetField(workstation, "actionName", "Use");
            SetField(workstation, "cameraAnchor", anchorGo.transform);
            SetField(workstation, "transitionDuration", 0.75f);

            // 6. Player Prefab and Scene Instance
            var playerGo = new GameObject("Player");
            playerGo.transform.position = new Vector3(0f, 0f, 0f);

            var charController = playerGo.AddComponent<CharacterController>();
            charController.center = new Vector3(0f, 0.9f, 0f);
            charController.radius = 0.35f;
            charController.height = 1.8f;
            charController.stepOffset = 0.3f;
            charController.skinWidth = 0.06f;

            var inputReader = playerGo.AddComponent<PlayerInputReader>();

            var visualRootGo = new GameObject("VisualRoot");
            visualRootGo.transform.SetParent(playerGo.transform, false);

            var bodyGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bodyGo.name = "BodyMesh";
            bodyGo.transform.SetParent(visualRootGo.transform, false);
            bodyGo.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            bodyGo.transform.localScale = new Vector3(0.65f, 0.9f, 0.65f);
            Object.DestroyImmediate(bodyGo.GetComponent<Collider>()); // Let CharacterController handle physical collisions
            if (playerMat != null) bodyGo.GetComponent<Renderer>().sharedMaterial = playerMat;

            // Visor indicating facing direction
            var visorGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visorGo.name = "VisorIndicator";
            visorGo.transform.SetParent(visualRootGo.transform, false);
            visorGo.transform.localPosition = new Vector3(0f, 1.35f, 0.24f);
            visorGo.transform.localScale = new Vector3(0.35f, 0.14f, 0.2f);
            Object.DestroyImmediate(visorGo.GetComponent<Collider>());
            if (visorMat != null) visorGo.GetComponent<Renderer>().sharedMaterial = visorMat;

            var playerController = playerGo.AddComponent<PlayerController>();
            SetField(playerController, "characterController", charController);
            SetField(playerController, "inputReader", inputReader);
            SetField(playerController, "visualRoot", visualRootGo.transform);
            SetField(playerController, "walkSpeed", 4.0f);
            SetField(playerController, "turnSpeed", 720f);

            var sensor = playerGo.AddComponent<InteractionSensor>();
            SetField(sensor, "playerController", playerController);
            SetField(sensor, "inputReader", inputReader);
            SetField(sensor, "detectionRadius", 2.2f);

            // Save player prefab
            PrefabUtility.SaveAsPrefabAsset(playerGo, PrefabPath);
            CafeLogger.Log("P1Generator", $"Saved player prototype prefab at {PrefabPath}");

            // 7. Camera Rig
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var camera = camGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.11f, 0.10f, 0.09f);
            camera.fieldOfView = 38f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 100f;

            camGo.AddComponent<AudioListener>();

            // URP Additional Camera Data
            var urpCam = camGo.GetComponent<UniversalAdditionalCameraData>() ?? camGo.AddComponent<UniversalAdditionalCameraData>();
            if (urpCam != null)
            {
                urpCam.renderPostProcessing = true;
            }

            var cameraRig = camGo.AddComponent<CafeCameraRig>();
            SetField(cameraRig, "followTarget", playerGo.transform);
            SetField(cameraRig, "pitchAngle", 40f);
            SetField(cameraRig, "yawAngle", 45f);
            SetField(cameraRig, "distance", 9.5f);
            SetField(cameraRig, "fieldOfView", 38f);
            SetField(cameraRig, "followDamping", 0.12f);

            camGo.transform.position = cameraRig.CalculateDesiredGameplayPosition();
            camGo.transform.rotation = Quaternion.Euler(40f, 45f, 0f);

            // 8. HUD Canvas & Interaction Prompt View
            var canvasGo = new GameObject("HUD_Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            // Prompt badge container
            var badgeGo = new GameObject("PromptBadge");
            badgeGo.transform.SetParent(canvasGo.transform, false);

            var badgeRect = badgeGo.AddComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(0.5f, 0f);
            badgeRect.anchorMax = new Vector2(0.5f, 0f);
            badgeRect.pivot = new Vector2(0.5f, 0.5f);
            badgeRect.anchoredPosition = new Vector2(0f, 85f);
            badgeRect.sizeDelta = new Vector2(300f, 48f);

            var bgImage = badgeGo.AddComponent<Image>();
            bgImage.color = new Color(0.12f, 0.11f, 0.10f, 0.88f); // dark coffee backdrop

            var canvasGroup = badgeGo.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            // Key badge "[E]"
            var keyGo = new GameObject("KeyText");
            keyGo.transform.SetParent(badgeGo.transform, false);
            var keyRect = keyGo.AddComponent<RectTransform>();
            keyRect.anchorMin = new Vector2(0f, 0f);
            keyRect.anchorMax = new Vector2(0.25f, 1f);
            keyRect.offsetMin = new Vector2(8f, 4f);
            keyRect.offsetMax = new Vector2(0f, -4f);

            var keyTmp = keyGo.AddComponent<TextMeshProUGUI>();
            keyTmp.text = "[E]";
            keyTmp.fontSize = 20;
            keyTmp.alignment = TextAlignmentOptions.Center;
            keyTmp.color = new Color(0.96f, 0.78f, 0.55f); // warm caramel cream
            keyTmp.fontStyle = FontStyles.Bold;

            // Action / Description Text
            var actionGo = new GameObject("ActionText");
            actionGo.transform.SetParent(badgeGo.transform, false);
            var actionRect = actionGo.AddComponent<RectTransform>();
            actionRect.anchorMin = new Vector2(0.25f, 0f);
            actionRect.anchorMax = new Vector2(1f, 1f);
            actionRect.offsetMin = new Vector2(4f, 4f);
            actionRect.offsetMax = new Vector2(-8f, -4f);

            var actionTmp = actionGo.AddComponent<TextMeshProUGUI>();
            actionTmp.text = "Inspect Pastry Case";
            actionTmp.fontSize = 17;
            actionTmp.alignment = TextAlignmentOptions.MidlineLeft;
            actionTmp.color = Color.white;

            var promptView = badgeGo.AddComponent<InteractionPromptView>();
            SetField(promptView, "sensor", sensor);
            SetField(promptView, "canvasGroup", canvasGroup);
            SetField(promptView, "keyLabel", keyTmp);
            SetField(promptView, "actionLabel", actionTmp);

            // Save scene
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            CafeLogger.Log("P1Generator", $"Successfully generated and saved P1 test scene to '{ScenePath}'.");
            EditorUtility.DisplayDialog("After Hours Café", "P1 Camera & Interaction Test Scene successfully generated!", "OK");
        }

        private static Material GetOrCreateMaterial(string matName, Color color)
        {
            string path = $"{ArtDir}/{matName}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            mat = new Material(shader);
            mat.color = color;
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static void SetField(object obj, string fieldName, object value)
        {
            var type = obj.GetType();
            while (type != null)
            {
                var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                if (field != null)
                {
                    field.SetValue(obj, value);
                    return;
                }
                type = type.BaseType;
            }
        }
    }
}
