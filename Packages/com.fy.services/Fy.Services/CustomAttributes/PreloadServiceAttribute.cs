using System;
using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace Fy.Services
{
    [AttributeUsage(AttributeTargets.Class)]
    [BaseTypeRequired(typeof(IService))]
    [Preserve]
    public sealed class PreloadServiceAttribute : Attribute { }
}