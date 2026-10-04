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
using UnityEngine.InputSystem;
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

            var pastryAnchor = new GameObject("InteractionAnchor");
            pastryAnchor.transform.SetParent(pastryGo.transform, false);
            pastryAnchor.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            SetField(pastryInteractable, "interactionAnchor", pastryAnchor.transform);
            SetField(pastryInteractable, "promptVerticalOffset", 0.35f);
            pastryGo.AddComponent<InteractableFocusHighlighter>();

            // 4b. Coffee Grinder on Counter
            var grinderGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            grinderGo.name = "Interactable_ManualGrinder";
            grinderGo.transform.position = new Vector3(-3.2f, 1.35f, 1.5f);
            grinderGo.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            var grinderInteractable = grinderGo.AddComponent<InteractableBase>();
            SetField(grinderInteractable, "promptText", "Coffee Grinder");
            SetField(grinderInteractable, "actionName", "Examine");
            if (grinderMat != null) grinderGo.GetComponent<Renderer>().sharedMaterial = grinderMat;

            var grinderAnchor = new GameObject("InteractionAnchor");
            grinderAnchor.transform.SetParent(grinderGo.transform, false);
            grinderAnchor.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            SetField(grinderInteractable, "interactionAnchor", grinderAnchor.transform);
            SetField(grinderInteractable, "promptVerticalOffset", 0.30f);
            grinderGo.AddComponent<InteractableFocusHighlighter>();

            // 5. Workstation Interactable with Contextual Camera Anchor
            var workstationGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            workstationGo.name = "Workstation_EspressoBar";
            workstationGo.transform.position = new Vector3(1.8f, 0.5f, 2.5f);
            workstationGo.transform.localScale = new Vector3(1.4f, 1.0f, 0.9f);
            if (machineMat != null) workstationGo.GetComponent<Renderer>().sharedMaterial = machineMat;

            // Camera anchor child: positioned close-up, elevated, angled at workstation
            var anchorGo = new GameObject("CameraAnchor_Espresso");
            anchorGo.transform.SetParent(workstationGo.transform, false);
            anchorGo.transform.localPosition = new Vector3(0f, 0.95f, -1.2f);
            anchorGo.transform.localRotation = Quaternion.Euler(22f, 0f, 0f);

            var wsPromptAnchor = new GameObject("InteractionAnchor");
            wsPromptAnchor.transform.SetParent(workstationGo.transform, false);
            wsPromptAnchor.transform.localPosition = new Vector3(0f, 0.85f, 0f);

            var workstation = workstationGo.AddComponent<WorkstationInteractable>();
            SetField(workstation, "promptText", "Espresso Machine");
            SetField(workstation, "actionName", "Use");
            SetField(workstation, "cameraAnchor", anchorGo.transform);
            SetField(workstation, "interactionAnchor", wsPromptAnchor.transform);
            SetField(workstation, "promptVerticalOffset", 0.40f);
            SetField(workstation, "transitionDuration", 0.75f);
            workstationGo.AddComponent<InteractableFocusHighlighter>();

            // 6. Player Prefab and Scene Instance
            var playerGo = new GameObject("Player");
            playerGo.transform.position = new Vector3(0f, 0f, 0f);

            var charController = playerGo.AddComponent<CharacterController>();
            charController.center = new Vector3(0f, 0.9f, 0f);
            charController.radius = 0.35f;
            charController.height = 1.8f;
            charController.stepOffset = 0.3f;
            charController.skinWidth = 0.06f;

            var schemeTracker = playerGo.AddComponent<ActiveControlSchemeTracker>();
            var inputReader = playerGo.AddComponent<PlayerInputReader>();
            SetField(inputReader, "schemeTracker", schemeTracker);
            var actionsAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/Mannan/Settings/PlayerInputActions.inputactions");
            if (actionsAsset != null)
            {
                SetField(inputReader, "actionsAsset", actionsAsset);
            }

            var visualRootGo = new GameObject("VisualRoot");
            visualRootGo.transform.SetParent(playerGo.transform, false);

            var bodyGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bodyGo.name = "BodyMesh";
            bodyGo.transform.SetParent(visualRootGo.transform, false);
            bodyGo.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            bodyGo.transform.localScale = new Vector3(0.65f, 0.9f, 0.65f);
            Object.DestroyImmediate(bodyGo.GetComponent<Collider>());
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

            // 8. Hybrid Interaction UI Presentation (Screen-Space Canvas)
            var canvasGo = new GameObject("Interaction_Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var canvasScaler = canvasGo.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var presController = canvasGo.AddComponent<InteractionPresentationController>();

            // 8a. Object-Tracking Prompt
            var promptGo = new GameObject("ObjectInteractionPrompt");
            promptGo.transform.SetParent(canvasGo.transform, false);
            var promptRect = promptGo.AddComponent<RectTransform>();
            promptRect.sizeDelta = new Vector2(250f, 42f);
            promptRect.pivot = new Vector2(0.5f, 0f); // pivot at bottom center

            var promptVisualGo = new GameObject("VisualRoot");
            promptVisualGo.transform.SetParent(promptGo.transform, false);
            var visualRect = promptVisualGo.AddComponent<RectTransform>();
            visualRect.sizeDelta = new Vector2(250f, 42f);

            var promptCanvasGroup = promptVisualGo.AddComponent<CanvasGroup>();
            promptCanvasGroup.alpha = 0f;

            // Card background
            var bgImage = promptVisualGo.AddComponent<Image>();
            bgImage.color = new Color(0.11f, 0.09f, 0.08f, 0.94f); // deep warm coffee

            // Key badge container "[ E ]"
            var keyGo = new GameObject("KeyBadge");
            keyGo.transform.SetParent(promptVisualGo.transform, false);
            var keyRect = keyGo.AddComponent<RectTransform>();
            keyRect.anchorMin = new Vector2(0f, 0f);
            keyRect.anchorMax = new Vector2(0.24f, 1f);
            keyRect.offsetMin = new Vector2(6f, 5f);
            keyRect.offsetMax = new Vector2(-2f, -5f);

            var keyBg = keyGo.AddComponent<Image>();
            keyBg.color = new Color(0.24f, 0.19f, 0.15f, 0.95f); // pill backing

            var keyTextGo = new GameObject("Label");
            keyTextGo.transform.SetParent(keyGo.transform, false);
            var keyTextRect = keyTextGo.AddComponent<RectTransform>();
            keyTextRect.anchorMin = Vector2.zero;
            keyTextRect.anchorMax = Vector2.one;
            keyTextRect.offsetMin = Vector2.zero;
            keyTextRect.offsetMax = Vector2.zero;

            var keyTmp = keyTextGo.AddComponent<TextMeshProUGUI>();
            keyTmp.text = "[ E ]";
            keyTmp.fontSize = 17;
            keyTmp.alignment = TextAlignmentOptions.Center;
            keyTmp.color = new Color(0.96f, 0.82f, 0.58f); // warm caramel cream
            keyTmp.fontStyle = FontStyles.Bold;

            // Action text
            var actionGo = new GameObject("ActionText");
            actionGo.transform.SetParent(promptVisualGo.transform, false);
            var actionRect = actionGo.AddComponent<RectTransform>();
            actionRect.anchorMin = new Vector2(0.24f, 0f);
            actionRect.anchorMax = new Vector2(1f, 1f);
            actionRect.offsetMin = new Vector2(8f, 4f);
            actionRect.offsetMax = new Vector2(-8f, -4f);

            var actionTmp = actionGo.AddComponent<TextMeshProUGUI>();
            actionTmp.text = "Interact";
            actionTmp.fontSize = 16;
            actionTmp.alignment = TextAlignmentOptions.MidlineLeft;
            actionTmp.color = Color.white;

            var objectPrompt = promptGo.AddComponent<ObjectInteractionPrompt>();
            SetField(objectPrompt, "canvasRect", canvasGo.GetComponent<RectTransform>());
            SetField(objectPrompt, "canvasGroup", promptCanvasGroup);
            SetField(objectPrompt, "visualRoot", promptVisualGo.transform);
            SetField(objectPrompt, "keyBadgeLabel", keyTmp);
            SetField(objectPrompt, "actionTextLabel", actionTmp);

            // 8b. Workstation Control Hint (Safe Area Bottom-Left)
            var hintGo = new GameObject("WorkstationControlHint");
            hintGo.transform.SetParent(canvasGo.transform, false);
            var hintRect = hintGo.AddComponent<RectTransform>();
            hintRect.anchorMin = new Vector2(0f, 0f);
            hintRect.anchorMax = new Vector2(0f, 0f);
            hintRect.pivot = new Vector2(0f, 0f);
            hintRect.anchoredPosition = new Vector2(36f, 36f);
            hintRect.sizeDelta = new Vector2(165f, 40f);

            var hintCanvasGroup = hintGo.AddComponent<CanvasGroup>();
            hintCanvasGroup.alpha = 0f;

            var hintBg = hintGo.AddComponent<Image>();
            hintBg.color = new Color(0.11f, 0.09f, 0.08f, 0.94f);

            // Key badge container "[ Q ]"
            var hintKeyGo = new GameObject("KeyBadge");
            hintKeyGo.transform.SetParent(hintGo.transform, false);
            var hintKeyRect = hintKeyGo.AddComponent<RectTransform>();
            hintKeyRect.anchorMin = new Vector2(0f, 0f);
            hintKeyRect.anchorMax = new Vector2(0.32f, 1f);
            hintKeyRect.offsetMin = new Vector2(6f, 5f);
            hintKeyRect.offsetMax = new Vector2(-2f, -5f);

            var hintKeyBg = hintKeyGo.AddComponent<Image>();
            hintKeyBg.color = new Color(0.24f, 0.19f, 0.15f, 0.95f);

            var hintKeyTextGo = new GameObject("Label");
            hintKeyTextGo.transform.SetParent(hintKeyGo.transform, false);
            var hintKeyTextRect = hintKeyTextGo.AddComponent<RectTransform>();
            hintKeyTextRect.anchorMin = Vector2.zero;
            hintKeyTextRect.anchorMax = Vector2.one;
            hintKeyTextRect.offsetMin = Vector2.zero;
            hintKeyTextRect.offsetMax = Vector2.zero;

            var hintKeyTmp = hintKeyTextGo.AddComponent<TextMeshProUGUI>();
            hintKeyTmp.text = "[ Q ]";
            hintKeyTmp.fontSize = 17;
            hintKeyTmp.alignment = TextAlignmentOptions.Center;
            hintKeyTmp.color = new Color(0.96f, 0.82f, 0.58f);
            hintKeyTmp.fontStyle = FontStyles.Bold;

            // Action label "Back"
            var hintActionGo = new GameObject("ActionText");
            hintActionGo.transform.SetParent(hintGo.transform, false);
            var hintActionRect = hintActionGo.AddComponent<RectTransform>();
            hintActionRect.anchorMin = new Vector2(0.32f, 0f);
            hintActionRect.anchorMax = new Vector2(1f, 1f);
            hintActionRect.offsetMin = new Vector2(6f, 4f);
            hintActionRect.offsetMax = new Vector2(-6f, -4f);

            var hintActionTmp = hintActionGo.AddComponent<TextMeshProUGUI>();
            hintActionTmp.text = "Back";
            hintActionTmp.fontSize = 16;
            hintActionTmp.alignment = TextAlignmentOptions.MidlineLeft;
            hintActionTmp.color = Color.white;

            var workstationHint = hintGo.AddComponent<WorkstationControlHint>();
            SetField(workstationHint, "rectTransform", hintRect);
            SetField(workstationHint, "canvasGroup", hintCanvasGroup);
            SetField(workstationHint, "keyBadgeLabel", hintKeyTmp);
            SetField(workstationHint, "actionLabel", hintActionTmp);

            // Wire presentation controller
            SetField(presController, "sensor", sensor);
            SetField(presController, "inputReader", inputReader);
            SetField(presController, "objectPrompt", objectPrompt);
            SetField(presController, "workstationHint", workstationHint);
            SetField(presController, "targetCamera", camera);

            // Save UI prefabs
            const string promptPrefabPath = "Assets/Mannan/Prefabs/ObjectInteractionPrompt.prefab";
            PrefabUtility.SaveAsPrefabAsset(promptGo, promptPrefabPath);
            const string hintPrefabPath = "Assets/Mannan/Prefabs/WorkstationControlHint.prefab";
            PrefabUtility.SaveAsPrefabAsset(hintGo, hintPrefabPath);
            const string canvasPrefabPath = "Assets/Mannan/Prefabs/InteractionCanvas.prefab";
            PrefabUtility.SaveAsPrefabAsset(canvasGo, canvasPrefabPath);
            CafeLogger.Log("P1Generator", "Saved interaction UI prefabs under Assets/Mannan/Prefabs/");

            // Save scene
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            CafeLogger.Log("P1Generator", $"Successfully generated and saved P1 test scene to '{ScenePath}'.");
            EditorUtility.DisplayDialog("After Hours Café", "P1 Camera & Interaction Test Scene successfully generated with Hybrid Interaction UI!", "OK");
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
