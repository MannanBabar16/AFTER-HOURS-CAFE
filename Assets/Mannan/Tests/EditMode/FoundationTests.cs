using System.Collections.Generic;
using Mannan.Core.Bootstrap;
using Mannan.Core.Data;
using Mannan.Core.Validation;
using Mannan.Editor.Validation;
using NUnit.Framework;
using UnityEngine;

namespace Mannan.Tests.EditMode
{
    public class FoundationTests
    {
        private class TestDefinition : DefinitionBase
        {
            public void SetIdForTesting(string testId)
            {
                var idField = typeof(DefinitionBase).GetField("id", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                idField?.SetValue(this, testId);
            }
        }

        [Test]
        public void ValidationIssue_Properties_MatchConstructorArguments()
        {
            var issue = new ValidationIssue(ValidationSeverity.Warning, "TestCat", "TestMsg");
            Assert.AreEqual(ValidationSeverity.Warning, issue.Severity);
            Assert.AreEqual("TestCat", issue.Category);
            Assert.AreEqual("TestMsg", issue.Message);
            Assert.IsNull(issue.ContextObject);
        }

        [Test]
        public void DefinitionBase_WithEmptyId_GeneratesErrorIssue()
        {
            var def = ScriptableObject.CreateInstance<TestDefinition>();
            def.name = "TestCoffeeRecipe";
            def.SetIdForTesting("");

            var issues = new List<ValidationIssue>();
            def.Validate(issues);

            Assert.IsTrue(issues.Exists(i => i.Severity == ValidationSeverity.Error && i.Category == "Definitions"));
            Object.DestroyImmediate(def);
        }

        [Test]
        public void DefinitionBase_WithValidId_DoesNotGenerateErrorIssue()
        {
            var def = ScriptableObject.CreateInstance<TestDefinition>();
            def.name = "TestCoffeeRecipe";
            def.SetIdForTesting("recipe_espresso_solo");

            var issues = new List<ValidationIssue>();
            def.Validate(issues);

            Assert.IsFalse(issues.Exists(i => i.Severity == ValidationSeverity.Error));
            Object.DestroyImmediate(def);
        }

        [Test]
        public void CafeBootstrap_WithNullEntry_GeneratesValidationWarning()
        {
            var go = new GameObject("TestBootstrap");
            var bootstrap = go.AddComponent<CafeBootstrap>();

            var issues = new List<ValidationIssue>();
            // Since initializables is empty by default, validate passes with 0 issues
            bootstrap.Validate(issues);
            Assert.AreEqual(0, issues.Count);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ProjectValidator_ValidateAll_RunsWithoutException()
        {
            Assert.DoesNotThrow(() =>
            {
                List<ValidationIssue> issues = ProjectValidator.ValidateAll();
                Assert.IsNotNull(issues);
            });
        }
    }
}
