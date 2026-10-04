using System;
using System.Collections.Generic;
using Mannan.Core.Logging;
using Mannan.Core.Validation;
using UnityEngine;
using UnityEngine.Events;

namespace Mannan.Interaction
{
    /// <summary>
    /// Core interaction contract for 3/4 diorama perspective.
    /// Can be implemented by coffee machines, cups, trays, tables, doors, robots, or workstations.
    /// </summary>
    public interface IInteractable
    {
        string PromptText { get; }
        string ActionName { get; }
        Transform Transform { get; }

        bool CanInteract(GameObject interactor);
        void OnFocusEnter(GameObject interactor);
        void OnFocusExit(GameObject interactor);
        void Interact(GameObject interactor);
    }
}
