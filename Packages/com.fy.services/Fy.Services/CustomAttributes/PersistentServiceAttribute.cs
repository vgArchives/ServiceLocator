using System;
using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace Fy.Services
{
    /// <summary>
    /// Put this on a MonoBehaviour service class to keep it alive across scene loads.
    /// </summary>
    /// <remarks>
    /// The default factory calls <c>DontDestroyOnLoad</c> on the created instance. It only works for MonoBehaviour
    /// services placed at the scene root; plain services already survive scene loads on their own.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    [BaseTypeRequired(typeof(IService))]
    [Preserve]
    public sealed class PersistentServiceAttribute : Attribute { }
}
