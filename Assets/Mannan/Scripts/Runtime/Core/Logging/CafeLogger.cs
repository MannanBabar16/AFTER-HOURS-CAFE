using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Mannan.Core.Logging
{
    public static class CafeLogger
    {
        private const string Prefix = "[After Hours Café]";

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Log(string category, string message, Object context = null)
        {
            Debug.Log($"{Prefix} [{category}] {message}", context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogWarning(string category, string message, Object context = null)
        {
            Debug.LogWarning($"{Prefix} [{category}] {message}", context);
        }

        public static void LogError(string category, string message, Object context = null)
        {
            Debug.LogError($"{Prefix} [{category}] {message}", context);
        }
    }
}
