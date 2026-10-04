using System.IO;
using Mannan.Coffee;
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
        private const string ScenePathP1 = "Assets/Mannan/Scenes/P1_CameraInteractionTest.unity";
        private const string ScenePathP2 = "Assets/Mannan/Scenes/P2_EspressoTest.unity";
        private const string PrefabPath = "Assets/Mannan/Prefabs/Player_Prototype.prefab";
        private const string ArtDir = "Assets/Mannan/Art";

        [InitializeOnLoadMethod]
        private static void AutoEnsureScenes()
        {
            EditorApplication.delayCall += () =>
            {
                if (!File.Exists(ScenePathP2))
                {
                    GenerateScene(ScenePathP2, "P2 Espresso Prototype Scene", false);
                }
            };
        }

        [MenuItem("Mannan/After Hours Café/Setup P1 Test Scene", priority = 10)]
        public static void GenerateP1Scene()
        {
            GenerateScene(ScenePathP1, "P1 Camera & Interaction Test Scene", true);
        }

        [MenuItem("Mannan/After Hours Café/Setup P2 Espresso Scene", priority = 11)]
        public static void GenerateP2Scene()
        {
            GenerateScene(ScenePathP2, "P2 Espresso Prototype Scene", true);
        }

        public static void GenerateScene(string targetScenePath, string title, bool showDialog = false)
        {
            CafeLogger.Log("P1Generator", $"Starting generation of {title}...");

            // Ensure directories exist
            EnsureFolder("Assets/Mannan", "Scenes");
            EnsureFolder("Assets/Mannan", "Prefabs");
            EnsureFolder("Assets/Mannan/Prefabs", "Coffee");
            EnsureFolder("Assets/Mannan", "Art");
            EnsureFolder("Assets/Mannan", "Data");
            EnsureFolder("Assets/Mannan/Data", "Drinks");
            EnsureFolder("Assets/Mannan", "Derived");
            EnsureFolder("Assets/Mannan/Derived", "PolygonParticles");
            EnsureFolder("Assets/Mannan/Derived/PolygonParticles", "VFX");
            EnsureFolder("Assets/Mannan/Derived", "EpicToonFX");
            EnsureFolder("Assets/Mannan/Derived/EpicToonFX", "VFX");

            // Create prototype materials
            Material floorMat = GetOrCreateMaterial("Floor_Prototype_Mat", new Color(0.24f, 0.20f, 0.17f));
            Material counterMat = GetOrCreateMaterial("Counter_Prototype_Mat", new Color(0.38f, 0.28f, 0.20f));
            Material playerMat = GetOrCreateMaterial("Player_Prototype_Mat", new Color(0.85f, 0.65f, 0.40f));
            Material visorMat = GetOrCreateMaterial("Visor_Prototype_Mat", new Color(0.12f, 0.12f, 0.15f));
            Material machineMat = GetOrCreateMaterial("Machine_Prototype_Mat", new Color(0.70f, 0.25f, 0.22f));
            Material grinderMat = GetOrCreateMaterial("Grinder_Prototype_Mat", new Color(0.45f, 0.45f, 0.48f));
            Material liquidMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Mannan/Art/Coffee_Liquid_Mat.mat");
            Material cartoonMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Mnostva Art/FREE_Interiors_2/Materials/Cartoon_Mat.mat");

            // Load 3D meshes from Mnostva Art
            Mesh cupMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Mnostva Art/FREE_Interiors_2/Meshes/Cafe_Coffee_Cup_1.fbx");
            Mesh machineMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Mnostva Art/FREE_Interiors_2/Meshes/Cafe_Coffee_Machine_1.fbx");
            Mesh portafilterMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Mnostva Art/FREE_Interiors_2/Meshes/Cafe_Portafilter_1.fbx");

            // Load / Create DrinkDefinition
            DrinkDefinition espressoDef = AssetDatabase.LoadAssetAtPath<DrinkDefinition>("Assets/Mannan/Data/Drinks/Drink_ClassicEspresso.asset");
            if (espressoDef == null)
            {
                espressoDef = ScriptableObject.CreateInstance<DrinkDefinition>();
                SetField(espressoDef, "drinkId", "espresso");
                SetField(espressoDef, "displayName", "Classic Espresso");
                SetField(espressoDef, "description", "Rich, bold single shot of espresso topped with a velvety golden crema.");
                SetField(espressoDef, "extractionDuration", 3.5f);
                SetField(espressoDef, "liquidColor", new Color(0.18f, 0.10f, 0.05f, 1.0f));
                SetField(espressoDef, "cremaColor", new Color(0.78f, 0.58f, 0.32f, 1.0f));
                AssetDatabase.CreateAsset(espressoDef, "Assets/Mannan/Data/Drinks/Drink_ClassicEspresso.asset");
            }

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

            // 5. P2 Complete Espresso Workstation
            var workstationGo = BuildEspressoWorkstation(
                new Vector3(1.8f, 0f, 2.5f),
                counterMat,
                machineMat,
                cartoonMat,
                liquidMat,
                machineMesh,
                portafilterMesh,
                cupMesh,
                espressoDef
            );

            // 6. Player Prefab and Scene Instance
            var playerGo = new GameObject("Player");
            playerGo.tag = "Player";
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
            SetField(sensor, "interactionRadius", 2.2f);
            SetField(sensor, "forwardAngleWeight", 0.6f);

            // Save Player Prefab
            PrefabUtility.SaveAsPrefabAsset(playerGo, PrefabPath);
            CafeLogger.Log("P1Generator", $"Saved Player prototype prefab to '{PrefabPath}'.");

            // 7. Diorama Perspective Camera Rig
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var camera = camGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 38f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 100f;
            camGo.AddComponent<AudioListener>();

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
            promptRect.pivot = new Vector2(0.5f, 0f);

            var promptVisualGo = new GameObject("VisualRoot");
            promptVisualGo.transform.SetParent(promptGo.transform, false);
            var visualRect = promptVisualGo.AddComponent<RectTransform>();
            visualRect.sizeDelta = new Vector2(250f, 42f);

            var promptCanvasGroup = promptVisualGo.AddComponent<CanvasGroup>();
            promptCanvasGroup.alpha = 0f;

            var bgImage = promptVisualGo.AddComponent<Image>();
            bgImage.color = new Color(0.11f, 0.09f, 0.08f, 0.94f);

            var keyGo = new GameObject("KeyBadge");
            keyGo.transform.SetParent(promptVisualGo.transform, false);
            var keyRect = keyGo.AddComponent<RectTransform>();
            keyRect.anchorMin = new Vector2(0f, 0f);
            keyRect.anchorMax = new Vector2(0.24f, 1f);
            keyRect.offsetMin = new Vector2(6f, 5f);
            keyRect.offsetMax = new Vector2(-2f, -5f);

            var keyBg = keyGo.AddComponent<Image>();
            keyBg.color = new Color(0.24f, 0.19f, 0.15f, 0.95f);

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
            keyTmp.color = new Color(0.96f, 0.82f, 0.58f);
            keyTmp.fontStyle = FontStyles.Bold;

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

            // 8c. P2 Espresso Workstation HUD
            var espressoWorkstation = workstationGo.GetComponent<EspressoWorkstation>();
            BuildEspressoWorkstationUI(canvasGo, espressoWorkstation);

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
            EditorSceneManager.SaveScene(scene, targetScenePath);

            // Also save duplicate to P1 scene path if generating P2 or vice-versa for convenience
            if (targetScenePath == ScenePathP1)
            {
                EditorSceneManager.SaveScene(scene, ScenePathP2);
            }
            else
            {
                EditorSceneManager.SaveScene(scene, ScenePathP1);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            CafeLogger.Log("P1Generator", $"Successfully generated and saved {title} to '{targetScenePath}'.");
            if (showDialog)
            {
                EditorUtility.DisplayDialog("After Hours Café", $"{title} successfully generated with full Espresso Vertical Slice!", "OK");
            }
        }

        private static GameObject BuildEspressoWorkstation(
            Vector3 worldPos,
            Material counterMat,
            Material machineMat,
            Material cartoonMat,
            Material liquidMat,
            Mesh machineMesh,
            Mesh portafilterMesh,
            Mesh cupMesh,
            DrinkDefinition espressoDef)
        {
            var wsGo = new GameObject("Workstation_EspressoBar");
            wsGo.transform.position = worldPos;

            // Box collider around entire workstation
            var boxCol = wsGo.AddComponent<BoxCollider>();
            boxCol.center = new Vector3(0f, 0.75f, 0f);
            boxCol.size = new Vector3(1.6f, 1.5f, 1.1f);

            // 1. Counter table base
            var counterBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counterBase.name = "CounterBase";
            counterBase.transform.SetParent(wsGo.transform, false);
            counterBase.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            counterBase.transform.localScale = new Vector3(1.5f, 0.9f, 0.9f);
            Object.DestroyImmediate(counterBase.GetComponent<Collider>());
            if (counterMat != null) counterBase.GetComponent<Renderer>().sharedMaterial = counterMat;

            // 2. Machine Visual Root
            var machineRoot = new GameObject("MachineModel");
            machineRoot.transform.SetParent(wsGo.transform, false);
            machineRoot.transform.localPosition = new Vector3(0f, 0.90f, 0.05f);

            if (machineMesh != null)
            {
                var mf = machineRoot.AddComponent<MeshFilter>();
                var mr = machineRoot.AddComponent<MeshRenderer>();
                mf.sharedMesh = machineMesh;
                mr.sharedMaterial = cartoonMat != null ? cartoonMat : machineMat;
            }
            else
            {
                // Fallback geometry
                var machineBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
                machineBox.transform.SetParent(machineRoot.transform, false);
                machineBox.transform.localPosition = new Vector3(0f, 0.35f, 0f);
                machineBox.transform.localScale = new Vector3(0.65f, 0.70f, 0.55f);
                Object.DestroyImmediate(machineBox.GetComponent<Collider>());
                if (machineMat != null) machineBox.GetComponent<Renderer>().sharedMaterial = machineMat;
            }

            // Portafilter
            var portafilterGo = new GameObject("Portafilter");
            portafilterGo.transform.SetParent(wsGo.transform, false);
            portafilterGo.transform.localPosition = new Vector3(0f, 1.12f, -0.06f);
            if (portafilterMesh != null)
            {
                var mfPf = portafilterGo.AddComponent<MeshFilter>();
                var mrPf = portafilterGo.AddComponent<MeshRenderer>();
                mfPf.sharedMesh = portafilterMesh;
                mrPf.sharedMaterial = cartoonMat != null ? cartoonMat : machineMat;
            }

            // 3. Authored Sockets
            var cupSocket = new GameObject("CupSocket");
            cupSocket.transform.SetParent(wsGo.transform, false);
            cupSocket.transform.localPosition = new Vector3(0f, 0.94f, -0.15f);

            var cupStagingSocket = new GameObject("CupStagingSocket");
            cupStagingSocket.transform.SetParent(wsGo.transform, false);
            cupStagingSocket.transform.localPosition = new Vector3(0.48f, 0.92f, -0.15f);

            var cameraAnchor = new GameObject("CameraAnchor_Espresso");
            cameraAnchor.transform.SetParent(wsGo.transform, false);
            cameraAnchor.transform.localPosition = new Vector3(0f, 1.25f, -0.75f);
            cameraAnchor.transform.localRotation = Quaternion.Euler(16f, 0f, 0f);

            var interactionAnchor = new GameObject("InteractionAnchor");
            interactionAnchor.transform.SetParent(wsGo.transform, false);
            interactionAnchor.transform.localPosition = new Vector3(0f, 1.15f, 0f);

            // 4. Stream & Steam VFX
            var spoutAnchor = new GameObject("PourSpoutAnchor");
            spoutAnchor.transform.SetParent(wsGo.transform, false);
            spoutAnchor.transform.localPosition = new Vector3(0f, 1.10f, -0.15f);
            var streamPs = CreatePourStreamVFX(spoutAnchor, liquidMat);

            var steamAnchor = new GameObject("SteamAnchor");
            steamAnchor.transform.SetParent(wsGo.transform, false);
            steamAnchor.transform.localPosition = new Vector3(0f, 1.02f, -0.15f);
            var steamPs = CreateSteamVFX(steamAnchor);

            // 5. Brew button for tactile mechanical feedback
            var brewButton = new GameObject("BrewButton");
            brewButton.transform.SetParent(machineRoot.transform, false);
            brewButton.transform.localPosition = new Vector3(0.22f, 0.50f, -0.22f);
            var btnVis = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            btnVis.transform.SetParent(brewButton.transform, false);
            btnVis.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            btnVis.transform.localScale = new Vector3(0.04f, 0.015f, 0.04f);
            Object.DestroyImmediate(btnVis.GetComponent<Collider>());
            if (counterMat != null) btnVis.GetComponent<Renderer>().sharedMaterial = counterMat;

            // 6. AudioSource
            var audioSrc = wsGo.AddComponent<AudioSource>();
            audioSrc.playOnAwake = false;
            audioSrc.spatialBlend = 0.8f;
            audioSrc.minDistance = 1.0f;
            audioSrc.maxDistance = 8.0f;

            // 7. Cup Instance
            var cupInstance = BuildCoffeeCup(cupMesh, cartoonMat, liquidMat);
            cupInstance.transform.SetParent(wsGo.transform, false);
            cupInstance.transform.position = cupStagingSocket.transform.position;
            cupInstance.transform.rotation = cupStagingSocket.transform.rotation;

            // 8. Components
            var wsInteractable = wsGo.AddComponent<WorkstationInteractable>();
            SetField(wsInteractable, "promptText", "Espresso Machine");
            SetField(wsInteractable, "actionName", "Use");
            SetField(wsInteractable, "cameraAnchor", cameraAnchor.transform);
            SetField(wsInteractable, "interactionAnchor", interactionAnchor.transform);
            SetField(wsInteractable, "promptVerticalOffset", 0.45f);
            SetField(wsInteractable, "transitionDuration", 0.65f);
            wsGo.AddComponent<InteractableFocusHighlighter>();

            var espressoWs = wsGo.AddComponent<EspressoWorkstation>();
            SetField(espressoWs, "defaultDrinkDefinition", espressoDef);
            SetField(espressoWs, "cupSocket", cupSocket.transform);
            SetField(espressoWs, "cupStagingSocket", cupStagingSocket.transform);
            SetField(espressoWs, "activeCup", cupInstance);
            SetField(espressoWs, "cupMoveDuration", 0.42f);
            SetField(espressoWs, "cupMoveArcHeight", 0.07f);

            var presentation = wsGo.AddComponent<EspressoMachinePresentation>();
            SetField(presentation, "pourStreamVfx", streamPs);
            SetField(presentation, "steamVfx", steamPs);
            SetField(presentation, "brewButtonTransform", brewButton.transform);
            SetField(presentation, "machineVisualRoot", machineRoot.transform);
            SetField(presentation, "audioSource", audioSrc);

            // Save Workstation Prefab
            const string wsPrefabPath = "Assets/Mannan/Prefabs/Coffee/EspressoWorkstation.prefab";
            PrefabUtility.SaveAsPrefabAsset(wsGo, wsPrefabPath);
            CafeLogger.Log("P1Generator", $"Saved Espresso Workstation prefab to '{wsPrefabPath}'.");

            return wsGo;
        }

        private static CoffeeCup BuildCoffeeCup(Mesh cupMesh, Material cartoonMat, Material liquidMat)
        {
            var cupGo = new GameObject("CoffeeCup_Espresso");
            var cup = cupGo.AddComponent<CoffeeCup>();

            var visualRoot = new GameObject("VisualRoot");
            visualRoot.transform.SetParent(cupGo.transform, false);

            var cupModelGo = new GameObject("CupMesh");
            cupModelGo.transform.SetParent(visualRoot.transform, false);

            if (cupMesh != null)
            {
                var mf = cupModelGo.AddComponent<MeshFilter>();
                var mr = cupModelGo.AddComponent<MeshRenderer>();
                mf.sharedMesh = cupMesh;
                mr.sharedMaterial = cartoonMat;
            }
            else
            {
                var cupCyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cupCyl.transform.SetParent(cupModelGo.transform, false);
                cupCyl.transform.localScale = new Vector3(0.08f, 0.05f, 0.08f);
                cupCyl.transform.localPosition = new Vector3(0f, 0.025f, 0f);
                Object.DestroyImmediate(cupCyl.GetComponent<Collider>());
                if (cartoonMat != null) cupCyl.GetComponent<Renderer>().sharedMaterial = cartoonMat;
            }

            // Liquid Surface
            var liquidGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            liquidGo.name = "LiquidSurface";
            liquidGo.transform.SetParent(visualRoot.transform, false);
            liquidGo.transform.localScale = new Vector3(0.055f, 0.001f, 0.055f);
            liquidGo.transform.localPosition = new Vector3(0f, 0.015f, 0f);
            Object.DestroyImmediate(liquidGo.GetComponent<Collider>());

            var liquidMr = liquidGo.GetComponent<MeshRenderer>();
            if (liquidMat != null) liquidMr.sharedMaterial = liquidMat;

            var liquidVisual = liquidGo.AddComponent<CupLiquidVisual>();
            SetField(liquidVisual, "liquidRenderer", liquidMr);
            SetField(liquidVisual, "liquidTransform", liquidGo.transform);
            SetField(liquidVisual, "emptyLocalPosition", new Vector3(0f, 0.015f, 0f));
            SetField(liquidVisual, "fullLocalPosition", new Vector3(0f, 0.065f, 0f));
            SetField(liquidVisual, "emptyLocalScale", new Vector3(0.045f, 0.001f, 0.045f));
            SetField(liquidVisual, "fullLocalScale", new Vector3(0.065f, 0.002f, 0.065f));

            SetField(cup, "liquidVisual", liquidVisual);
            SetField(cup, "visualRoot", visualRoot.transform);

            // Save cup prefab
            const string cupPrefabPath = "Assets/Mannan/Prefabs/Coffee/CoffeeCup_Espresso.prefab";
            PrefabUtility.SaveAsPrefabAsset(cupGo, cupPrefabPath);
            CafeLogger.Log("P1Generator", $"Saved Coffee Cup prefab to '{cupPrefabPath}'.");

            return cup;
        }

        private static ParticleSystem CreatePourStreamVFX(GameObject parent, Material liquidMat)
        {
            var vfxGo = new GameObject("VFX_EspressoStream");
            vfxGo.transform.SetParent(parent.transform, false);

            var ps = vfxGo.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = true;
            main.startLifetime = 0.20f;
            main.startSpeed = 1.35f;
            main.startSize = 0.016f;
            main.gravityModifier = 1.4f;
            main.startColor = new Color(0.24f, 0.12f, 0.05f, 0.95f);

            var emission = ps.emission;
            emission.rateOverTime = 60f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.010f;

            var psRenderer = vfxGo.GetComponent<ParticleSystemRenderer>();
            if (liquidMat != null) psRenderer.sharedMaterial = liquidMat;

            // Save project-owned derived VFX prefab
            const string vfxPrefabPath = "Assets/Mannan/Derived/EpicToonFX/VFX/FX_EspressoStream.prefab";
            PrefabUtility.SaveAsPrefabAsset(vfxGo, vfxPrefabPath);

            return ps;
        }

        private static ParticleSystem CreateSteamVFX(GameObject parent)
        {
            var vfxGo = new GameObject("VFX_EspressoSteam");
            vfxGo.transform.SetParent(parent.transform, false);

            var ps = vfxGo.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = true;
            main.startLifetime = 1.4f;
            main.startSpeed = 0.18f;
            main.startSize = 0.035f;
            main.gravityModifier = -0.04f;
            main.startColor = new Color(1.0f, 0.96f, 0.90f, 0.22f);

            var emission = ps.emission;
            emission.rateOverTime = 10f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.030f;

            var sizeModule = ps.sizeOverLifetime;
            sizeModule.enabled = true;
            var curve = new AnimationCurve();
            curve.AddKey(0f, 0.5f);
            curve.AddKey(1f, 1.8f);
            sizeModule.size = new ParticleSystem.MinMaxCurve(1f, curve);

            var colorModule = ps.colorOverLifetime;
            colorModule.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.96f, 0.90f), 0f), new GradientColorKey(new Color(1f, 0.94f, 0.88f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.22f, 0.25f), new GradientAlphaKey(0f, 1f) }
            );
            colorModule.color = new ParticleSystem.MinMaxGradient(grad);

            // Save project-owned derived VFX prefab
            const string steamPrefabPath = "Assets/Mannan/Derived/PolygonParticles/VFX/FX_EspressoSteam.prefab";
            PrefabUtility.SaveAsPrefabAsset(vfxGo, steamPrefabPath);

            return ps;
        }

        private static void BuildEspressoWorkstationUI(GameObject canvasGo, EspressoWorkstation workstation)
        {
            var hudGo = new GameObject("EspressoWorkstationUI");
            hudGo.transform.SetParent(canvasGo.transform, false);
            var hudRect = hudGo.AddComponent<RectTransform>();
            hudRect.anchorMin = new Vector2(0.5f, 0f);
            hudRect.anchorMax = new Vector2(0.5f, 0f);
            hudRect.pivot = new Vector2(0.5f, 0f);
            hudRect.anchoredPosition = new Vector2(0f, 60f);
            hudRect.sizeDelta = new Vector2(360f, 120f);

            var hudCanvasGroup = hudGo.AddComponent<CanvasGroup>();
            hudCanvasGroup.alpha = 0f;

            // 1. Action prompt badge container (e.g. "[ E ] Place Cup")
            var promptContainer = new GameObject("ActionPromptBadge");
            promptContainer.transform.SetParent(hudGo.transform, false);
            var promptRect = promptContainer.AddComponent<RectTransform>();
            promptRect.anchorMin = new Vector2(0.5f, 0.5f);
            promptRect.anchorMax = new Vector2(0.5f, 0.5f);
            promptRect.pivot = new Vector2(0.5f, 0.5f);
            promptRect.anchoredPosition = new Vector2(0f, 10f);
            promptRect.sizeDelta = new Vector2(250f, 44f);

            var promptBg = promptContainer.AddComponent<Image>();
            promptBg.color = new Color(0.11f, 0.09f, 0.08f, 0.95f);

            var keyBadgeGo = new GameObject("KeyBadge");
            keyBadgeGo.transform.SetParent(promptContainer.transform, false);
            var keyBadgeRect = keyBadgeGo.AddComponent<RectTransform>();
            keyBadgeRect.anchorMin = new Vector2(0f, 0f);
            keyBadgeRect.anchorMax = new Vector2(0.25f, 1f);
            keyBadgeRect.offsetMin = new Vector2(6f, 5f);
            keyBadgeRect.offsetMax = new Vector2(-2f, -5f);

            var keyBadgeBg = keyBadgeGo.AddComponent<Image>();
            keyBadgeBg.color = new Color(0.24f, 0.19f, 0.15f, 0.95f);

            var keyBadgeTextGo = new GameObject("Label");
            keyBadgeTextGo.transform.SetParent(keyBadgeGo.transform, false);
            var keyBadgeTextRect = keyBadgeTextGo.AddComponent<RectTransform>();
            keyBadgeTextRect.anchorMin = Vector2.zero;
            keyBadgeTextRect.anchorMax = Vector2.one;
            keyBadgeTextRect.offsetMin = Vector2.zero;
            keyBadgeTextRect.offsetMax = Vector2.zero;

            var keyBadgeTmp = keyBadgeTextGo.AddComponent<TextMeshProUGUI>();
            keyBadgeTmp.text = "[ E ]";
            keyBadgeTmp.fontSize = 17;
            keyBadgeTmp.alignment = TextAlignmentOptions.Center;
            keyBadgeTmp.color = new Color(0.96f, 0.82f, 0.58f);
            keyBadgeTmp.fontStyle = FontStyles.Bold;

            var actionTextGo = new GameObject("ActionText");
            actionTextGo.transform.SetParent(promptContainer.transform, false);
            var actionTextRect = actionTextGo.AddComponent<RectTransform>();
            actionTextRect.anchorMin = new Vector2(0.25f, 0f);
            actionTextRect.anchorMax = new Vector2(1f, 1f);
            actionTextRect.offsetMin = new Vector2(8f, 4f);
            actionTextRect.offsetMax = new Vector2(-8f, -4f);

            var actionTextTmp = actionTextGo.AddComponent<TextMeshProUGUI>();
            actionTextTmp.text = "Place Cup";
            actionTextTmp.fontSize = 16;
            actionTextTmp.alignment = TextAlignmentOptions.MidlineLeft;
            actionTextTmp.color = Color.white;

            // 2. Extraction Progress Bar container
            var progressContainer = new GameObject("ProgressContainer");
            progressContainer.transform.SetParent(hudGo.transform, false);
            var progRect = progressContainer.AddComponent<RectTransform>();
            progRect.anchorMin = new Vector2(0.5f, 0.5f);
            progRect.anchorMax = new Vector2(0.5f, 0.5f);
            progRect.pivot = new Vector2(0.5f, 0.5f);
            progRect.anchoredPosition = new Vector2(0f, 10f);
            progRect.sizeDelta = new Vector2(280f, 46f);

            var progCanvasGroup = progressContainer.AddComponent<CanvasGroup>();
            progCanvasGroup.alpha = 0f;

            var progBg = progressContainer.AddComponent<Image>();
            progBg.color = new Color(0.11f, 0.09f, 0.08f, 0.95f);

            // Progress bar track
            var trackGo = new GameObject("BarTrack");
            trackGo.transform.SetParent(progressContainer.transform, false);
            var trackRect = trackGo.AddComponent<RectTransform>();
            trackRect.anchorMin = new Vector2(0.05f, 0.16f);
            trackRect.anchorMax = new Vector2(0.95f, 0.40f);
            trackRect.offsetMin = Vector2.zero;
            trackRect.offsetMax = Vector2.zero;
            var trackImg = trackGo.AddComponent<Image>();
            trackImg.color = new Color(0.22f, 0.17f, 0.14f, 1f);

            // Progress bar fill
            var fillGo = new GameObject("BarFill");
            fillGo.transform.SetParent(trackGo.transform, false);
            var fillRect = fillGo.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImg = fillGo.AddComponent<Image>();
            fillImg.color = new Color(0.92f, 0.68f, 0.32f, 1f);
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = 0;
            fillImg.fillAmount = 0f;

            // Status label
            var statusLabelGo = new GameObject("StatusLabel");
            statusLabelGo.transform.SetParent(progressContainer.transform, false);
            var statusLabelRect = statusLabelGo.AddComponent<RectTransform>();
            statusLabelRect.anchorMin = new Vector2(0.05f, 0.48f);
            statusLabelRect.anchorMax = new Vector2(0.95f, 0.92f);
            statusLabelRect.offsetMin = Vector2.zero;
            statusLabelRect.offsetMax = Vector2.zero;

            var statusLabelTmp = statusLabelGo.AddComponent<TextMeshProUGUI>();
            statusLabelTmp.text = "Extracting Espresso... 0%";
            statusLabelTmp.fontSize = 14;
            statusLabelTmp.alignment = TextAlignmentOptions.Center;
            statusLabelTmp.color = new Color(0.96f, 0.88f, 0.78f);

            // 3. Completion celebration banner
            var completionContainer = new GameObject("CompletionBanner");
            completionContainer.transform.SetParent(hudGo.transform, false);
            var compRect = completionContainer.AddComponent<RectTransform>();
            compRect.anchorMin = new Vector2(0.5f, 1f);
            compRect.anchorMax = new Vector2(0.5f, 1f);
            compRect.pivot = new Vector2(0.5f, 1f);
            compRect.anchoredPosition = new Vector2(0f, 15f);
            compRect.sizeDelta = new Vector2(220f, 32f);

            var compCanvasGroup = completionContainer.AddComponent<CanvasGroup>();
            compCanvasGroup.alpha = 0f;

            var compBg = completionContainer.AddComponent<Image>();
            compBg.color = new Color(0.24f, 0.18f, 0.10f, 0.92f);

            var compLabelGo = new GameObject("CompletionLabel");
            compLabelGo.transform.SetParent(completionContainer.transform, false);
            var compLabelRect = compLabelGo.AddComponent<RectTransform>();
            compLabelRect.anchorMin = Vector2.zero;
            compLabelRect.anchorMax = Vector2.one;
            compLabelRect.offsetMin = Vector2.zero;
            compLabelRect.offsetMax = Vector2.zero;

            var compLabelTmp = compLabelGo.AddComponent<TextMeshProUGUI>();
            compLabelTmp.text = "Espresso Ready!";
            compLabelTmp.fontSize = 16;
            compLabelTmp.alignment = TextAlignmentOptions.Center;
            compLabelTmp.color = new Color(0.98f, 0.84f, 0.52f);
            compLabelTmp.fontStyle = FontStyles.Bold;

            // Wire EspressoWorkstationUI component
            var wsUI = hudGo.AddComponent<EspressoWorkstationUI>();
            SetField(wsUI, "workstation", workstation);
            SetField(wsUI, "rootCanvasGroup", hudCanvasGroup);
            SetField(wsUI, "keyBadgeLabel", keyBadgeTmp);
            SetField(wsUI, "actionTextLabel", actionTextTmp);
            SetField(wsUI, "progressCanvasGroup", progCanvasGroup);
            SetField(wsUI, "progressFillBar", fillImg);
            SetField(wsUI, "progressStatusLabel", statusLabelTmp);
            SetField(wsUI, "completionCanvasGroup", compCanvasGroup);
            SetField(wsUI, "completionLabel", compLabelTmp);
        }

        private static void EnsureFolder(string parent, string child)
        {
            string full = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(full))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
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
