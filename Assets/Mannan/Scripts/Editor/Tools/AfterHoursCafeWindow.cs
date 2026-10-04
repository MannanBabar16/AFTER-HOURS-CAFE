using System.Collections.Generic;
using System.IO;
using Mannan.Core.Validation;
using Mannan.Editor.Validation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mannan.Editor.Tools
{
    public sealed class AfterHoursCafeWindow : EditorWindow
    {
        private VisualElement _validationContainer;
        private Label _statusSummaryLabel;
        private readonly List<ValidationIssue> _cachedIssues = new List<ValidationIssue>();

        [MenuItem("Mannan/After Hours Café/Project Hub", priority = 1)]
        public static void Open()
        {
            var window = GetWindow<AfterHoursCafeWindow>("After Hours Café");
            window.minSize = new Vector2(480, 520);
            window.Show();
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.backgroundColor = new Color(0.14f, 0.13f, 0.12f, 1f); // warm dark coffee tint
            root.style.paddingLeft = 14;
            root.style.paddingRight = 14;
            root.style.paddingTop = 14;
            root.style.paddingBottom = 14;

            // Scroll container
            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.flexGrow = 1;
            root.Add(scrollView);

            // Header
            var headerContainer = new VisualElement();
            headerContainer.style.marginBottom = 14;
            headerContainer.style.paddingBottom = 10;
            headerContainer.style.borderBottomWidth = 1;
            headerContainer.style.borderBottomColor = new Color(0.35f, 0.28f, 0.22f, 0.6f);

            var titleLabel = new Label("AFTER HOURS CAFÉ");
            titleLabel.style.fontSize = 20;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.color = new Color(0.96f, 0.88f, 0.78f); // warm crema cream
            headerContainer.Add(titleLabel);

            var subtitleLabel = new Label("Developer Hub & System Integrity Center");
            subtitleLabel.style.fontSize = 12;
            subtitleLabel.style.color = new Color(0.72f, 0.65f, 0.58f);
            headerContainer.Add(subtitleLabel);

            scrollView.Add(headerContainer);

            // Quick Nav Section
            var navSection = CreateCardSection("Project Shortcuts");
            var navButtonsRow = new VisualElement();
            navButtonsRow.style.flexDirection = FlexDirection.Row;
            navButtonsRow.style.flexWrap = Wrap.Wrap;

            navButtonsRow.Add(CreateQuickNavButton("Data", "Assets/Mannan/Data"));
            navButtonsRow.Add(CreateQuickNavButton("Prefabs", "Assets/Mannan/Prefabs"));
            navButtonsRow.Add(CreateQuickNavButton("Scenes", "Assets/Mannan/Scenes"));
            navButtonsRow.Add(CreateQuickNavButton("Scripts", "Assets/Mannan/Scripts"));
            navButtonsRow.Add(CreateDocButton("Docs/ARCHITECTURE.md"));

            navSection.Add(navButtonsRow);
            scrollView.Add(navSection);

            // P1 Test & Camera Controls Section
            var p1Section = CreateCardSection("P1: Camera & Interaction Prototype");
            var p1ButtonsRow = new VisualElement();
            p1ButtonsRow.style.flexDirection = FlexDirection.Row;
            p1ButtonsRow.style.flexWrap = Wrap.Wrap;
            p1ButtonsRow.style.marginBottom = 6;

            var openSceneBtn = new Button(() =>
            {
                const string p1ScenePath = "Assets/Mannan/Scenes/P1_CameraInteractionTest.unity";
                if (File.Exists(p1ScenePath))
                {
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(p1ScenePath);
                }
                else
                {
                    P1SceneGenerator.GenerateP1Scene();
                }
            })
            {
                text = "▶ Open P1 Test Scene"
            };
            openSceneBtn.style.height = 26;
            openSceneBtn.style.paddingLeft = 12;
            openSceneBtn.style.paddingRight = 12;
            openSceneBtn.style.marginRight = 6;
            openSceneBtn.style.marginBottom = 6;
            openSceneBtn.style.backgroundColor = new Color(0.28f, 0.42f, 0.28f);
            openSceneBtn.style.color = Color.white;
            p1ButtonsRow.Add(openSceneBtn);

            var regenSceneBtn = new Button(P1SceneGenerator.GenerateP1Scene)
            {
                text = "Regenerate P1 Test Scene"
            };
            regenSceneBtn.style.height = 26;
            regenSceneBtn.style.paddingLeft = 10;
            regenSceneBtn.style.paddingRight = 10;
            regenSceneBtn.style.marginBottom = 6;
            p1ButtonsRow.Add(regenSceneBtn);

            p1Section.Add(p1ButtonsRow);

            var p1Grid = new VisualElement();
            p1Grid.Add(CreateKeyValRow("Camera Mode", "3/4 Isometric Perspective (Pitch: 40°, Yaw: 45°, FOV: 38°)"));
            p1Grid.Add(CreateKeyValRow("Movement", "Camera-Relative WASD (CharacterController)"));
            p1Grid.Add(CreateKeyValRow("Interaction", "Proximity & Facing Angle Sensor (E to Interact)"));
            p1Grid.Add(CreateKeyValRow("Workstation", "Authored Close-Up Blend & Movement Lock (Q to Exit)"));
            p1Section.Add(p1Grid);

            scrollView.Add(p1Section);

            // Environment Info Section
            var envSection = CreateCardSection("Environment Baseline");
            var envGrid = new VisualElement();
            envGrid.Add(CreateKeyValRow("Root Namespace", "Mannan.*"));
            envGrid.Add(CreateKeyValRow("Project Owned Root", "Assets/Mannan"));
            envGrid.Add(CreateKeyValRow("Render Pipeline", "URP 17.x (Universal RP)"));
            envGrid.Add(CreateKeyValRow("Input System", "com.unity.inputsystem"));
            envGrid.Add(CreateKeyValRow("Assemblies", "Mannan.Runtime, Mannan.Editor, Mannan.Tests"));
            envSection.Add(envGrid);
            scrollView.Add(envSection);

            // Validation Section
            var valSection = CreateCardSection("Project Integrity & Validation");

            var validateBtnRow = new VisualElement();
            validateBtnRow.style.flexDirection = FlexDirection.Row;
            validateBtnRow.style.alignItems = Align.Center;
            validateBtnRow.style.marginBottom = 10;

            var validateBtn = new Button(RunValidation) { text = "Run Project Validation" };
            validateBtn.style.height = 30;
            validateBtn.style.paddingLeft = 16;
            validateBtn.style.paddingRight = 16;
            validateBtn.style.backgroundColor = new Color(0.38f, 0.26f, 0.18f); // roasted coffee brown
            validateBtn.style.color = Color.white;
            validateBtn.style.borderTopLeftRadius = 4;
            validateBtn.style.borderTopRightRadius = 4;
            validateBtn.style.borderBottomLeftRadius = 4;
            validateBtn.style.borderBottomRightRadius = 4;
            validateBtnRow.Add(validateBtn);

            _statusSummaryLabel = new Label("Click 'Run Project Validation' to scan definitions and scene.");
            _statusSummaryLabel.style.marginLeft = 12;
            _statusSummaryLabel.style.color = new Color(0.8f, 0.76f, 0.7f);
            validateBtnRow.Add(_statusSummaryLabel);

            valSection.Add(validateBtnRow);

            _validationContainer = new VisualElement();
            _validationContainer.style.marginTop = 6;
            valSection.Add(_validationContainer);

            scrollView.Add(valSection);

            // Run initial lightweight validation scan on open
            RunValidation();
        }

        private VisualElement CreateCardSection(string title)
        {
            var card = new VisualElement();
            card.style.backgroundColor = new Color(0.18f, 0.17f, 0.16f);
            card.style.borderTopLeftRadius = 6;
            card.style.borderTopRightRadius = 6;
            card.style.borderBottomLeftRadius = 6;
            card.style.borderBottomRightRadius = 6;
            card.style.borderTopWidth = 1;
            card.style.borderBottomWidth = 1;
            card.style.borderLeftWidth = 1;
            card.style.borderRightWidth = 1;
            card.style.borderTopColor = new Color(0.28f, 0.25f, 0.22f);
            card.style.borderBottomColor = new Color(0.28f, 0.25f, 0.22f);
            card.style.borderLeftColor = new Color(0.28f, 0.25f, 0.22f);
            card.style.borderRightColor = new Color(0.28f, 0.25f, 0.22f);
            card.style.paddingLeft = 12;
            card.style.paddingRight = 12;
            card.style.paddingTop = 10;
            card.style.paddingBottom = 10;
            card.style.marginBottom = 12;

            var titleLabel = new Label(title.ToUpperInvariant());
            titleLabel.style.fontSize = 11;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.color = new Color(0.85f, 0.65f, 0.45f); // caramel accent
            titleLabel.style.marginBottom = 8;
            card.Add(titleLabel);

            return card;
        }

        private VisualElement CreateKeyValRow(string label, string val)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.SpaceBetween;
            row.style.marginBottom = 3;

            var keyLbl = new Label(label);
            keyLbl.style.color = new Color(0.68f, 0.65f, 0.62f);
            row.Add(keyLbl);

            var valLbl = new Label(val);
            valLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            valLbl.style.color = new Color(0.9f, 0.88f, 0.85f);
            row.Add(valLbl);

            return row;
        }

        private Button CreateQuickNavButton(string label, string path)
        {
            var btn = new Button(() =>
            {
                var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (obj != null)
                {
                    Selection.activeObject = obj;
                    EditorGUIUtility.PingObject(obj);
                }
                else
                {
                    Debug.LogWarning($"[After Hours Café] Path does not exist or has no asset: {path}");
                }
            })
            {
                text = label
            };
            btn.style.marginRight = 6;
            btn.style.marginBottom = 6;
            btn.style.paddingLeft = 10;
            btn.style.paddingRight = 10;
            btn.style.height = 24;
            return btn;
        }

        private Button CreateDocButton(string relativePath)
        {
            var btn = new Button(() =>
            {
                string fullPath = Path.GetFullPath(relativePath);
                if (File.Exists(fullPath))
                {
                    EditorUtility.OpenWithDefaultApp(fullPath);
                }
                else
                {
                    Debug.LogWarning($"[After Hours Café] File not found: {fullPath}");
                }
            })
            {
                text = "Architecture Doc"
            };
            btn.style.marginRight = 6;
            btn.style.marginBottom = 6;
            btn.style.paddingLeft = 10;
            btn.style.paddingRight = 10;
            btn.style.height = 24;
            return btn;
        }

        private void RunValidation()
        {
            _cachedIssues.Clear();
            _cachedIssues.AddRange(ProjectValidator.ValidateAll());

            _validationContainer.Clear();

            int errors = 0;
            int warnings = 0;
            int infos = 0;

            foreach (ValidationIssue issue in _cachedIssues)
            {
                switch (issue.Severity)
                {
                    case ValidationSeverity.Error:
                        errors++;
                        break;
                    case ValidationSeverity.Warning:
                        warnings++;
                        break;
                    case ValidationSeverity.Info:
                        infos++;
                        break;
                }
            }

            if (_cachedIssues.Count == 0)
            {
                _statusSummaryLabel.text = "✓ All validation checks passed (0 issues found).";
                _statusSummaryLabel.style.color = new Color(0.45f, 0.85f, 0.5f);

                var emptyLabel = new Label("No validation issues found. System state is clean.");
                emptyLabel.style.color = new Color(0.6f, 0.6f, 0.6f);
                emptyLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                emptyLabel.style.marginTop = 6;
                _validationContainer.Add(emptyLabel);
                return;
            }

            _statusSummaryLabel.text = $"Issues: {errors} Error(s), {warnings} Warning(s), {infos} Info(s)";
            _statusSummaryLabel.style.color = errors > 0 ? new Color(0.95f, 0.45f, 0.45f) : (warnings > 0 ? new Color(0.95f, 0.8f, 0.4f) : new Color(0.5f, 0.8f, 0.95f));

            foreach (ValidationIssue issue in _cachedIssues)
            {
                _validationContainer.Add(CreateIssueRow(issue));
            }
        }

        private VisualElement CreateIssueRow(ValidationIssue issue)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
            row.style.borderTopWidth = 1;
            row.style.borderBottomWidth = 1;
            row.style.borderLeftWidth = 1;
            row.style.borderRightWidth = 1;
            row.style.borderTopColor = new Color(0.24f, 0.22f, 0.2f);
            row.style.borderBottomColor = new Color(0.24f, 0.22f, 0.2f);
            row.style.borderLeftColor = new Color(0.24f, 0.22f, 0.2f);
            row.style.borderRightColor = new Color(0.24f, 0.22f, 0.2f);
            row.style.borderTopLeftRadius = 4;
            row.style.borderTopRightRadius = 4;
            row.style.borderBottomLeftRadius = 4;
            row.style.borderBottomRightRadius = 4;
            row.style.paddingLeft = 8;
            row.style.paddingRight = 8;
            row.style.paddingTop = 6;
            row.style.paddingBottom = 6;
            row.style.marginBottom = 4;

            // Severity Badge
            var badge = new Label(issue.Severity.ToString().ToUpperInvariant());
            badge.style.fontSize = 9;
            badge.style.unityFontStyleAndWeight = FontStyle.Bold;
            badge.style.paddingLeft = 5;
            badge.style.paddingRight = 5;
            badge.style.paddingTop = 2;
            badge.style.paddingBottom = 2;
            badge.style.marginRight = 6;
            badge.style.borderTopLeftRadius = 3;
            badge.style.borderTopRightRadius = 3;
            badge.style.borderBottomLeftRadius = 3;
            badge.style.borderBottomRightRadius = 3;

            switch (issue.Severity)
            {
                case ValidationSeverity.Error:
                    badge.style.backgroundColor = new Color(0.6f, 0.15f, 0.15f);
                    badge.style.color = Color.white;
                    break;
                case ValidationSeverity.Warning:
                    badge.style.backgroundColor = new Color(0.6f, 0.45f, 0.1f);
                    badge.style.color = Color.white;
                    break;
                case ValidationSeverity.Info:
                    badge.style.backgroundColor = new Color(0.2f, 0.4f, 0.6f);
                    badge.style.color = Color.white;
                    break;
            }
            row.Add(badge);

            // Category Label
            var catLabel = new Label($"[{issue.Category}]");
            catLabel.style.fontSize = 11;
            catLabel.style.color = new Color(0.7f, 0.65f, 0.6f);
            catLabel.style.marginRight = 6;
            row.Add(catLabel);

            // Message
            var msgLabel = new Label(issue.Message);
            msgLabel.style.flexGrow = 1;
            msgLabel.style.whiteSpace = WhiteSpace.Normal;
            msgLabel.style.color = new Color(0.88f, 0.85f, 0.82f);
            row.Add(msgLabel);

            // Ping Button if context exists
            if (issue.ContextObject != null)
            {
                var pingBtn = new Button(() =>
                {
                    Selection.activeObject = issue.ContextObject;
                    EditorGUIUtility.PingObject(issue.ContextObject);
                })
                {
                    text = "Select"
                };
                pingBtn.style.height = 20;
                pingBtn.style.fontSize = 10;
                pingBtn.style.marginLeft = 6;
                row.Add(pingBtn);
            }

            return row;
        }
    }
}
