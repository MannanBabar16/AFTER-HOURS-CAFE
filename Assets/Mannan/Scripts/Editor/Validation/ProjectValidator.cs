using System;
using System.Collections.Generic;
using Mannan.Core.Data;
using Mannan.Core.Validation;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mannan.Editor.Validation
{
    /// <summary>
    /// Core project validation runner.
    /// Scans Assets/Mannan definitions and scene objects for integrity issues.
    /// </summary>
    public static class ProjectValidator
    {
        private const string ProjectRoot = "Assets/Mannan";

        [MenuItem("Mannan/After Hours Café/Run Project Validation", priority = 20)]
        public static void RunValidationMenu()
        {
            List<ValidationIssue> issues = ValidateAll();
            int errors = 0;
            int warnings = 0;
            int infos = 0;

            foreach (ValidationIssue issue in issues)
            {
                switch (issue.Severity)
                {
                    case ValidationSeverity.Error:
                        errors++;
                        Debug.LogError(issue.ToString(), issue.ContextObject);
                        break;
                    case ValidationSeverity.Warning:
                        warnings++;
                        Debug.LogWarning(issue.ToString(), issue.ContextObject);
                        break;
                    case ValidationSeverity.Info:
                        infos++;
                        Debug.Log(issue.ToString(), issue.ContextObject);
                        break;
                }
            }

            Debug.Log($"[After Hours Café] Validation complete: {errors} error(s), {warnings} warning(s), {infos} info note(s).");
        }

        public static List<ValidationIssue> ValidateAll()
        {
            var issues = new List<ValidationIssue>();

            ValidateDefinitions(issues);
            ValidateSceneObjects(issues);

            return issues;
        }

        public static void ValidateDefinitions(List<ValidationIssue> issues)
        {
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { ProjectRoot });
            var seenIdsByType = new Dictionary<Type, HashSet<string>>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (so == null)
                {
                    continue;
                }

                if (so is DefinitionBase def)
                {
                    Type type = def.GetType();
                    if (!seenIdsByType.TryGetValue(type, out HashSet<string> idSet))
                    {
                        idSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        seenIdsByType[type] = idSet;
                    }

                    if (!string.IsNullOrWhiteSpace(def.Id))
                    {
                        if (!idSet.Add(def.Id))
                        {
                            issues.Add(new ValidationIssue(
                                ValidationSeverity.Error,
                                "Duplicate ID",
                                $"Duplicate ID '{def.Id}' detected on definition '{def.name}' of type {type.Name}.",
                                def));
                        }
                    }
                }

                if (so is IValidatable validatable)
                {
                    try
                    {
                        validatable.Validate(issues);
                    }
                    catch (Exception ex)
                    {
                        issues.Add(new ValidationIssue(
                            ValidationSeverity.Error,
                            "Validation Exception",
                            $"Exception while validating {so.name}: {ex.Message}",
                            so));
                    }
                }
            }
        }

        public static void ValidateSceneObjects(List<ValidationIssue> issues)
        {
            MonoBehaviour[] sceneComponents = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            if (sceneComponents == null)
            {
                return;
            }

            foreach (MonoBehaviour component in sceneComponents)
            {
                if (component is IValidatable validatable)
                {
                    try
                    {
                        validatable.Validate(issues);
                    }
                    catch (Exception ex)
                    {
                        issues.Add(new ValidationIssue(
                            ValidationSeverity.Error,
                            "Scene Validation",
                            $"Exception during scene component validation: {ex.Message}",
                            component));
                    }
                }
            }
        }
    }
}
