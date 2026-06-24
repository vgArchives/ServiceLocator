using Fy.Services;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Fy.Services.RuntimeTests
{
    /// <summary>Verifies the reflection-driven auto-registration, preload, and required-service validation.</summary>
    [TestFixture]
    [TestOf(typeof(ServiceAutoLoader))]
    public sealed class ServiceAutoLoaderTests
    {
        private interface IDisabledDummyService : IService { }

        [DisableDefaultFactory]
        private sealed class DisabledDummyService : IDisabledDummyService
        {
            public void Dispose() { }
        }

        private interface IPreloadDummyService : IService { }

        [PreloadService]
        private sealed class PreloadDummyService : IPreloadDummyService
        {
            public static int InstantiationCount;

            public PreloadDummyService()
            {
                InstantiationCount++;
            }

            public void Dispose() { }
        }

        private interface IRegularDummyService : IService { }

        private sealed class RegularDummy : IRegularDummyService
        {
            public void Dispose() { }
        }

        private interface IActorDummyService : IService { }

        private sealed class ActorDummy : MonoBehaviour, IActorDummyService
        {
            public void Dispose()
            {
                if (this != null)
                {
                    Object.DestroyImmediate(gameObject);
                }
            }
        }

        [AbstractService]
        private interface IAbstractBaseDummyService : IService { }

        private interface IConcreteDummyService : IAbstractBaseDummyService { }

        private sealed class ConcreteDummy : IConcreteDummyService
        {
            public void Dispose() { }
        }

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceLocator.Reset();
        }

        /// <summary>Auto-registration creates a default factory for a non-MonoBehaviour service.</summary>
        [Test]
        public void AutoRegister_RegistersFactoryForNonMonoBehaviourService()
        {
            ServiceAutoLoader.AutoRegisterAll();

            Assert.That(ServiceLocator.HasFactory<IRegularDummyService>(), Is.True);
        }

        /// <summary>Auto-registration creates an actor factory for a MonoBehaviour service.</summary>
        [Test]
        public void AutoRegister_RegistersFactoryForMonoBehaviourService()
        {
            ServiceAutoLoader.AutoRegisterAll();

            Assert.That(ServiceLocator.HasFactory<IActorDummyService>(), Is.True);
        }

        /// <summary>After auto-registration, a non-MonoBehaviour service actually resolves to a usable instance via TryGet.</summary>
        [Test]
        public void AutoRegister_NonMonoBehaviourService_ResolvesThroughTryGet()
        {
            ServiceAutoLoader.AutoRegisterAll();

            bool resolved = ServiceLocator.TryGet(out IRegularDummyService regular);

            Assert.That(resolved, Is.True);
            Assert.That(regular, Is.Not.Null);
        }

        /// <summary>After auto-registration, a MonoBehaviour service actually resolves to a live Component via TryGet.</summary>
        [Test]
        public void AutoRegister_MonoBehaviourService_ResolvesThroughTryGet()
        {
            ServiceAutoLoader.AutoRegisterAll();

            bool resolved = ServiceLocator.TryGet(out IActorDummyService actor);

            Assert.That(resolved, Is.True);
            Assert.That(actor, Is.InstanceOf<Component>());
        }

        /// <summary>Auto-registration skips types marked DisableDefaultFactory.</summary>
        [Test]
        public void AutoRegister_SkipsDisableDefaultFactoryType()
        {
            ServiceAutoLoader.AutoRegisterAll();

            Assert.That(ServiceLocator.HasFactory<IDisabledDummyService>(), Is.False);
        }

        /// <summary>Auto-registration registers the concrete interface but hides the AbstractService base interface.</summary>
        [Test]
        public void AutoRegister_DoesNotRegisterAbstractServiceBaseInterface()
        {
            ServiceAutoLoader.AutoRegisterAll();

            Assert.That(ServiceLocator.HasFactory<IConcreteDummyService>(), Is.True);
            Assert.That(ServiceLocator.HasFactory<IAbstractBaseDummyService>(), Is.False);
        }

        /// <summary>Preload eagerly instantiates services marked PreloadService.</summary>
        [Test]
        public void Preload_InstantiatesPreloadService()
        {
            ServiceAutoLoader.AutoRegisterAll();
            PreloadDummyService.InstantiationCount = 0;

            ServiceAutoLoader.PreloadAll();

            Assert.That(PreloadDummyService.InstantiationCount, Is.EqualTo(1));
        }

        /// <summary>Validation passes silently when every required service has a factory.</summary>
        [Test]
        public void ValidateRequiredServices_WithWiredRequiredService_LogsNoError()
        {
            ServiceAutoLoader.AutoRegisterAll();
            ServiceAutoLoader.ValidateRequiredServices();

            LogAssert.NoUnexpectedReceived();
        }
    }
}
