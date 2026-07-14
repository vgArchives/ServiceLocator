using System;
using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace Fy.Services
{
    /// <summary>
    /// Put this on a service interface to hide it from auto-registration.
    /// </summary>
    /// <remarks>
    /// Use it on a shared base interface that several services extend, so the <see cref="ServiceAutoLoader"/> skips
    /// the base and registers the concrete interfaces instead.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Interface)]
    [BaseTypeRequired(typeof(IService))]
    [Preserve]
    public sealed class AbstractServiceAttribute : Attribute { }
}