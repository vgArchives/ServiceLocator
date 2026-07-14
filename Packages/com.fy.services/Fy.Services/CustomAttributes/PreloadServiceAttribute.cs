using System;
using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace Fy.Services
{
    /// <summary>
    /// Put this on a service class to build it upfront instead of on first request.
    /// </summary>
    /// <remarks>
    /// The <see cref="ServiceAutoLoader"/> resolves these before the first scene loads, so they are ready the moment
    /// gameplay starts.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    [BaseTypeRequired(typeof(IService))]
    [Preserve]
    public sealed class PreloadServiceAttribute : Attribute { }
}