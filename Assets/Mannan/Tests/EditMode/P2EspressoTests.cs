using System.Collections.Generic;
using Mannan.Coffee;
using Mannan.Core.Validation;
using Mannan.Interaction;
using NUnit.Framework;
using UnityEngine;

namespace Mannan.Tests.EditMode
{
    public class P2EspressoTests
    {
        [Test]
        public void DrinkDefinition_Validation_ChecksIdAndDuration()
        {
            var drink = ScriptableObject.CreateInstance<DrinkDefinition>();
            var issues = new List<ValidationIssue>();

            // Default instance has valid defaults
            drink.Validate(issues);
            Assert.AreEqual(0, issues.Count, "Default DrinkDefinition should be valid.");

            // Setting empty ID and 0 duration generates errors
            var idField = typeof(DrinkDefinition).GetField("drinkId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var durationField = typeof(DrinkDefinition).GetField("extractionDuration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            idField?.SetValue(drink, "");
            durationField?.SetValue(drink, 0f);

            issues.Clear();
            drink.Validate(issues);

            Assert.IsTrue(issues.Exists(i => i.Severity == ValidationSeverity.Error && i.Message.Contains("drinkId")));
            Assert.IsTrue(issues.Exists(i => i.Severity == ValidationSeverity.Error && i.Message.Contains("extractionDuration")));

            Object.DestroyImmediate(drink);
        }

        [Test]
        public void CoffeeCup_FillClamping_AndDrinkAssignment()
        {
            var go = new GameObject("TestCup");
            var cup = go.AddComponent<CoffeeCup>();

            var drink = ScriptableObject.CreateInstance<DrinkDefinition>();
            cup.SetDrink(drink);
            Assert.AreSame(drink, cup.CurrentDrink);

            // Test clamping
            cup.SetFillAmount(-0.5f);
            Assert.AreEqual(0f, cup.NormalizedFill, 0.001f);
            Assert.IsFalse(cup.IsFilled);

            cup.SetFillAmount(0.5f);
            Assert.AreEqual(0.5f, cup.NormalizedFill, 0.001f);
            Assert.IsFalse(cup.IsFilled);

            cup.SetFillAmount(1.5f);
            Assert.AreEqual(1.0f, cup.NormalizedFill, 0.001f);
            Assert.IsTrue(cup.IsFilled);

            // Reset
            cup.ResetCup();
            Assert.AreEqual(0f, cup.NormalizedFill, 0.001f);
            Assert.IsNull(cup.CurrentDrink);
            Assert.IsFalse(cup.IsFilled);

            Object.DestroyImmediate(drink);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void CupLiquidVisual_SetFill_ClampsAndHandlesZero()
        {
            var go = new GameObject("TestLiquid");
            var mr = go.AddComponent<MeshRenderer>();
            var visual = go.AddComponent<CupLiquidVisual>();

            var mrField = typeof(CupLiquidVisual).GetField("liquidRenderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var transField = typeof(CupLiquidVisual).GetField("liquidTransform", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            mrField?.SetValue(visual, mr);
            transField?.SetValue(visual, go.transform);

            // Zero fill disables renderer
            visual.SetFill(0f);
            Assert.IsFalse(mr.enabled);

            // Positive fill enables renderer
            visual.SetFill(0.5f);
            Assert.IsTrue(mr.enabled);

            // Clamping over 1
            visual.SetFill(2.0f);
            Assert.IsTrue(mr.enabled);

            // Reset disables renderer
            visual.ResetVisual();
            Assert.IsFalse(mr.enabled);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void EspressoWorkstation_StateTransitions_FullLifecycle()
        {
            var wsGo = new GameObject("TestWorkstation");
            wsGo.AddComponent<BoxCollider>();
            var wsInteractable = wsGo.AddComponent<WorkstationInteractable>();
            var workstation = wsGo.AddComponent<EspressoWorkstation>();

            // Setup sockets
            var brewSocket = new GameObject("BrewSocket");
            brewSocket.transform.SetParent(wsGo.transform);
            brewSocket.transform.position = new Vector3(0f, 1f, 0f);

            var stagingSocket = new GameObject("StagingSocket");
            stagingSocket.transform.SetParent(wsGo.transform);
            stagingSocket.transform.position = new Vector3(0.5f, 1f, 0f);

            var cupGo = new GameObject("Cup");
            var cup = cupGo.AddComponent<CoffeeCup>();

            var drink = ScriptableObject.CreateInstance<DrinkDefinition>();
            var durationField = typeof(DrinkDefinition).GetField("extractionDuration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            durationField?.SetValue(drink, 2.0f);

            // Wire fields via reflection
            SetField(workstation, "cupSocket", brewSocket.transform);
            SetField(workstation, "cupStagingSocket", stagingSocket.transform);
            SetField(workstation, "activeCup", cup);
            SetField(workstation, "defaultDrinkDefinition", drink);
            SetField(workstation, "cupMoveDuration", 0f); // Instant for deterministic test

            Assert.AreEqual(EspressoMachineState.Idle, workstation.CurrentState);
            Assert.IsFalse(workstation.IsCupInBrewSocket);

            // 1. Idle -> Place Cup -> CupReady
            bool placed = workstation.TryPlaceCup();
            Assert.IsTrue(placed);
            Assert.AreEqual(EspressoMachineState.CupReady, workstation.CurrentState);
            Assert.IsTrue(workstation.IsCupInBrewSocket);
            Assert.AreEqual(brewSocket.transform.position, cup.transform.position);

            // Cannot re-place when CupReady
            Assert.IsFalse(workstation.TryPlaceCup());

            // 2. CupReady -> Start Extraction -> Extracting
            bool started = workstation.TryStartExtraction();
            Assert.IsTrue(started);
            Assert.AreEqual(EspressoMachineState.Extracting, workstation.CurrentState);
            Assert.AreEqual(0f, workstation.CurrentProgress);

            // Cannot re-start while Extracting
            Assert.IsFalse(workstation.TryStartExtraction());

            // 3. Extracting -> Progress Updates
            workstation.UpdateExtraction(1.0f); // Halfway (1.0 / 2.0)
            Assert.AreEqual(0.5f, workstation.CurrentProgress, 0.001f);
            Assert.AreEqual(0.5f, cup.NormalizedFill, 0.001f);
            Assert.AreEqual(EspressoMachineState.Extracting, workstation.CurrentState);

            // 4. Progress Completes -> Completed
            workstation.UpdateExtraction(1.0f); // Finished
            Assert.AreEqual(1.0f, workstation.CurrentProgress, 0.001f);
            Assert.AreEqual(1.0f, cup.NormalizedFill, 0.001f);
            Assert.AreEqual(EspressoMachineState.Completed, workstation.CurrentState);

            // Cannot extract when Completed
            Assert.IsFalse(workstation.TryStartExtraction());

            // 5. Completed -> Reset / Take Cup -> Idle
            bool reset = workstation.TryResetOrTakeCup();
            Assert.IsTrue(reset);
            Assert.AreEqual(EspressoMachineState.Idle, workstation.CurrentState);
            Assert.IsFalse(workstation.IsCupInBrewSocket);
            Assert.AreEqual(stagingSocket.transform.position, cup.transform.position);
            Assert.AreEqual(0f, cup.NormalizedFill, 0.001f);

            Object.DestroyImmediate(drink);
            Object.DestroyImmediate(cupGo);
            Object.DestroyImmediate(wsGo);
        }

        [Test]
        public void EspressoWorkstation_CannotExtractWithoutCupOrDefinition()
        {
            var wsGo = new GameObject("TestWorkstation");
            wsGo.AddComponent<BoxCollider>();
            wsGo.AddComponent<WorkstationInteractable>();
            var workstation = wsGo.AddComponent<EspressoWorkstation>();

            // Without cup and definition in CupReady state
            SetField(workstation, "defaultDrinkDefinition", null);
            SetField(workstation, "activeCup", null);

            Assert.IsFalse(workstation.TryStartExtraction());
            Assert.IsFalse(workstation.TryPlaceCup());

            Object.DestroyImmediate(wsGo);
        }

        [Test]
        public void EspressoWorkstation_Validation_RequiresSockets()
        {
            var wsGo = new GameObject("TestWorkstationValidation");
            wsGo.AddComponent<BoxCollider>();
            wsGo.AddComponent<WorkstationInteractable>();
            var workstation = wsGo.AddComponent<EspressoWorkstation>();

            var issues = new List<ValidationIssue>();
            workstation.Validate(issues);

            Assert.IsTrue(issues.Exists(i => i.Severity == ValidationSeverity.Error && i.Message.Contains("cupSocket")));
            Assert.IsTrue(issues.Exists(i => i.Severity == ValidationSeverity.Warning && i.Message.Contains("defaultDrinkDefinition")));

            Object.DestroyImmediate(wsGo);
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
