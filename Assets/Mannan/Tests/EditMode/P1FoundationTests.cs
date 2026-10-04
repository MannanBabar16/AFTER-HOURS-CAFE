using System.Collections.Generic;
using Mannan.Core.Validation;
using Mannan.Interaction;
using Mannan.Player;
using Mannan.UI;
using Mannan.World;
using NUnit.Framework;
using UnityEngine;

namespace Mannan.Tests.EditMode
{
    public class P1FoundationTests
    {
        [Test]
        public void PlayerController_SetMovementLocked_UpdatesCanMove()
        {
            var go = new GameObject("TestPlayer");
            go.AddComponent<CharacterController>();
            var controller = go.AddComponent<PlayerController>();

            Assert.IsTrue(controller.CanMove);

            controller.SetMovementLocked(true);
            Assert.IsFalse(controller.CanMove);

            controller.SetMovementLocked(false);
            Assert.IsTrue(controller.CanMove);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void CafeCameraRig_PlanarVectors_AreNormalizedAndPlanar()
        {
            var go = new GameObject("TestCameraRig");
            var cam = go.AddComponent<Camera>();
            var rig = go.AddComponent<CafeCameraRig>();

            go.transform.rotation = Quaternion.Euler(40f, 45f, 0f);

            Vector3 forward = rig.GetPlanarForward();
            Vector3 right = rig.GetPlanarRight();

            Assert.AreEqual(0f, forward.y, 0.001f, "Forward Y component must be zero.");
            Assert.AreEqual(0f, right.y, 0.001f, "Right Y component must be zero.");
            Assert.AreEqual(1f, forward.magnitude, 0.001f, "Forward must be unit length.");
            Assert.AreEqual(1f, right.magnitude, 0.001f, "Right must be unit length.");
            Assert.AreEqual(0f, Vector3.Dot(forward, right), 0.001f, "Forward and Right must be orthogonal.");

            Object.DestroyImmediate(go);
        }

        [Test]
        public void CafeCameraRig_CalculatesDesiredPosition_WithDioramaOffset()
        {
            var targetGo = new GameObject("Target");
            targetGo.transform.position = Vector3.zero;

            var camGo = new GameObject("CameraRig");
            camGo.AddComponent<Camera>();
            var rig = camGo.AddComponent<CafeCameraRig>();
            rig.SetFollowTarget(targetGo.transform);

            Vector3 pos = rig.CalculateDesiredGameplayPosition();

            // At pitch 40, yaw 45, distance 9.5: position should be elevated and diagonal
            Assert.Greater(pos.y, 0f, "Camera must be elevated above target.");
            Assert.Less(pos.x, 0f, "With yaw 45 and looking toward origin, camera X is negative.");
            Assert.Less(pos.z, 0f, "With yaw 45 and looking toward origin, camera Z is negative.");

            Object.DestroyImmediate(camGo);
            Object.DestroyImmediate(targetGo);
        }

        [Test]
        public void WorkstationInteractable_WithoutAnchor_GeneratesValidationError()
        {
            var go = new GameObject("TestWorkstation");
            go.AddComponent<BoxCollider>();
            var workstation = go.AddComponent<WorkstationInteractable>();

            var issues = new List<ValidationIssue>();
            workstation.Validate(issues);

            Assert.IsTrue(issues.Exists(i => i.Severity == ValidationSeverity.Error && i.Category == "Workstation"));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void InteractableBase_Validation_RequiresCollider()
        {
            var go = new GameObject("TestInteractableNoCollider");
            var interactable = go.AddComponent<InteractableBase>();

            var issues = new List<ValidationIssue>();
            interactable.Validate(issues);

            Assert.IsTrue(issues.Exists(i => i.Severity == ValidationSeverity.Warning && i.Message.Contains("Collider")));
            Object.DestroyImmediate(go);
        }

        [Test]
        public void PlayerInputReader_ConsumeTriggers_ResetsFlag()
        {
            var go = new GameObject("TestInputReader");
            var reader = go.AddComponent<PlayerInputReader>();

            Assert.IsFalse(reader.ConsumeInteractTriggered());
            Assert.IsFalse(reader.ConsumeCancelTriggered());

            // Default binding string should return readable string
            Assert.IsNotEmpty(reader.GetInteractBindingDisplayString());
            Assert.IsNotEmpty(reader.GetCancelBindingDisplayString());

            Object.DestroyImmediate(go);
        }

        [Test]
        public void InteractableBase_GetPromptWorldPosition_UsesAnchorOrFallback()
        {
            var go = new GameObject("TestInteractable");
            var col = go.AddComponent<BoxCollider>();
            col.size = new Vector3(1f, 1f, 1f);
            var interactable = go.AddComponent<InteractableBase>();

            // Without anchor, fallback should calculate above collider bounds
            Vector3 fallbackPos = interactable.GetPromptWorldPosition();
            Assert.Greater(fallbackPos.y, go.transform.position.y);

            // With anchor, should strictly match anchor position
            var anchorGo = new GameObject("Anchor");
            anchorGo.transform.SetParent(go.transform);
            anchorGo.transform.position = new Vector3(10f, 20f, 30f);

            var field = typeof(InteractableBase).GetField("interactionAnchor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(interactable, anchorGo.transform);

            Vector3 expectedPos = new Vector3(10f, 20f + interactable.PromptVerticalOffset, 30f);
            Assert.AreEqual(expectedPos, interactable.GetPromptWorldPosition());

            Object.DestroyImmediate(anchorGo);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void ObjectInteractionPrompt_Validation_ChecksReferences()
        {
            var go = new GameObject("TestObjectPrompt");
            var prompt = go.AddComponent<Mannan.UI.ObjectInteractionPrompt>();

            var issues = new List<ValidationIssue>();
            prompt.Validate(issues);

            Assert.IsTrue(issues.Exists(i => i.Severity == ValidationSeverity.Warning && i.Message.Contains("CanvasGroup")));
            Assert.IsTrue(issues.Exists(i => i.Severity == ValidationSeverity.Warning && i.Message.Contains("actionTextLabel")));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void WorkstationControlHint_Validation_ChecksReferences()
        {
            var go = new GameObject("TestWorkstationHint");
            var hint = go.AddComponent<Mannan.UI.WorkstationControlHint>();

            var issues = new List<ValidationIssue>();
            hint.Validate(issues);

            Assert.IsTrue(issues.Exists(i => i.Severity == ValidationSeverity.Warning && i.Message.Contains("CanvasGroup")));
            Assert.IsTrue(issues.Exists(i => i.Severity == ValidationSeverity.Warning && i.Message.Contains("keyBadgeLabel")));

            Object.DestroyImmediate(go);
        }
    }
}
