using System;
using UnityEngine;

namespace Mannan.Core.Validation
{
    [Serializable]
    public sealed class ValidationIssue
    {
        public ValidationSeverity Severity { get; }
        public string Category { get; }
        public string Message { get; }
        public UnityEngine.Object ContextObject { get; }

        public ValidationIssue(ValidationSeverity severity, string category, string message, UnityEngine.Object contextObject = null)
        {
            Severity = severity;
            Category = string.IsNullOrWhiteSpace(category) ? "General" : category;
            Message = message ?? string.Empty;
            ContextObject = contextObject;
        }

        public override string ToString()
        {
            string contextName = ContextObject != null ? $" [{ContextObject.name}]" : string.Empty;
            return $"[{Severity}] ({Category}){contextName}: {Message}";
        }
    }
}
