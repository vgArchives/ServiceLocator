using System;
using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace Fy.Services
{
    [AttributeUsage(AttributeTargets.Interface)]
    [BaseTypeRequired(typeof(IService))]
    [Preserve]
    public sealed class DynamicServiceAttribute : Attribute { }
}