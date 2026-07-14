using System;
using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace Fy.Services
{
    /// <summary>
    /// Put this on a service interface to allow its instance to be replaced at runtime.
    /// </summary>
    /// <remarks>
    /// By default <see cref="ServiceLocator.SetService{T}"/> refuses to overwrite an existing service. With this
    /// attribute it disposes the old instance and swaps in the new one. Useful for per-scene services.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Interface)]
    [BaseTypeRequired(typeof(IService))]
    [Preserve]
    public sealed class DynamicServiceAttribute : Attribute { }
}