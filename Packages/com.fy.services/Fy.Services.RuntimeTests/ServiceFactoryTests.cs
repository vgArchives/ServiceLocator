using System.Text.RegularExpressions;
using Fy.Services;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Fy.Services.RuntimeTests
{
    /// <summary>Verifies factory resolution: caching, null/wrong-type handling, MonoBehaviour creation, scene persistence, and destroyed-object validity.</summary>
    [TestFixture]
    public sealed class ServiceFactoryTests
    {
        private interface IDummyService : IService { }

        private sealed class DummyService : IDummyService
        {
            public void Dispose() { }
        }

        private interface IOtherService : IService { }

        private sealed class OtherService : IOtherService
        {
            public bool WasDisposed { get; private set; }

            public void Dispose()
            {
                WasDisposed = true;
            }
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

        private interface IPersistentActorDummyService : IService { }

        [PersistentService]
        private sealed class PersistentActorDummy : MonoBehaviour, IPersistentActorDummyService
        {
            public void Dispose()
            {
                if (this != null)
                {
                    Object.DestroyImmediate(gameObject);
                }
            }
        }

        private sealed class TransientFactory : IServiceFactory
        {
            public bool ShouldSetService => false;

            public IService GetService()
            {
                return new DummyService();
            }
        }

        private sealed class NullFactory : IServiceFactory
        {
            public IService GetService()
            {
                return null;
            }
        }

        private sealed class WrongTypeFactory : IServiceFactory
        {
            public readonly OtherService Created = new();

            public IService GetService()
            {
                return Created;
            }
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

        /// <summary>A transient factory (ShouldSetService false) returns a fresh instance of each request.</summary>
        [Test]
        public void Factory_WithShouldSetServiceFalse_DoesNotCacheInstance()
        {
            ServiceLocator.SetFactory<IDummyService>(new TransientFactory());

            ServiceLocator.TryGet(out IDummyService first);
            ServiceLocator.TryGet(out IDummyService second);

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Not.Null);
            Assert.That(first, Is.Not.SameAs(second));
        }

        /// <summary>A caching factory (ShouldSetService true) returns the same instance on repeated requests.</summary>
        [Test]
        public void Factory_WithShouldSetServiceTrue_CachesInstance()
        {
            ServiceLocator.SetFactory<IDummyService>(DefaultServiceFactory<DummyService>.Instance);

            ServiceLocator.TryGet(out IDummyService first);
            ServiceLocator.TryGet(out IDummyService second);

            Assert.That(first, Is.SameAs(second));
        }

        /// <summary>A factory that returns null resolves to no service.</summary>
        [Test]
        public void Factory_ReturningNull_ResolvesToNull()
        {
            ServiceLocator.SetFactory<IDummyService>(new NullFactory());

            bool resolved = ServiceLocator.TryGet(out IDummyService result);

            Assert.That(resolved, Is.False);
            Assert.That(result, Is.Null);
        }

        /// <summary>A factory returning the wrong type resolves to null and disposes of the created object.</summary>
        [Test]
        public void Factory_ReturningWrongType_ResolvesToNullAndDisposesCreated()
        {
            var factory = new WrongTypeFactory();

            ServiceLocator.SetFactory<IDummyService>(factory);

            bool resolved = ServiceLocator.TryGet(out IDummyService result);

            Assert.That(resolved, Is.False);
            Assert.That(result, Is.Null);
            Assert.That(factory.Created.WasDisposed, Is.True);
        }

        /// <summary>The actor factory creates a GameObject when no instance exists in the scene.</summary>
        [Test]
        public void ActorFactory_WhenNoneExists_CreatesGameObject()
        {
            ServiceLocator.SetFactory<IActorDummyService>(DefaultServiceActorFactory<ActorDummy>.Instance);

            bool resolved = ServiceLocator.TryGet(out IActorDummyService result);

            Assert.That(resolved, Is.True);
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Component>());
        }

        /// <summary>The actor factory reuses an existing instance already present in the scene.</summary>
        [Test]
        public void ActorFactory_WhenOneExists_ReturnsExisting()
        {
            var existingObject = new GameObject("ExistingActorDummy");
            ActorDummy existing = existingObject.AddComponent<ActorDummy>();

            ServiceLocator.SetFactory<IActorDummyService>(DefaultServiceActorFactory<ActorDummy>.Instance);
            ServiceLocator.TryGet(out IActorDummyService result);

            Assert.That(result, Is.SameAs(existing));
        }

        /// <summary>When multiple instances exist in the scene, the actor factory logs an error and returns the lowest-InstanceID one deterministically.</summary>
        [Test]
        public void ActorFactory_WhenMultipleExist_LogsErrorAndReturnsDeterministicWinner()
        {
            var firstObject = new GameObject("ActorDummyA");
            ActorDummy first = firstObject.AddComponent<ActorDummy>();
            var secondObject = new GameObject("ActorDummyB");
            ActorDummy second = secondObject.AddComponent<ActorDummy>();

            ActorDummy expected = first.GetEntityId() < second.GetEntityId() ? first : second;

            ServiceLocator.SetFactory<IActorDummyService>(DefaultServiceActorFactory<ActorDummy>.Instance);

            LogAssert.Expect(LogType.Error, new Regex("Multiple .* service instances found"));
            ServiceLocator.TryGet(out IActorDummyService result);

            Assert.That(result, Is.SameAs(expected));

            Object.DestroyImmediate(firstObject);
            Object.DestroyImmediate(secondObject);
        }

        /// <summary>A [PersistentService] MonoBehaviour created by the actor factory is moved to the DontDestroyOnLoad scene.</summary>
        [Test]
        public void PersistentActorFactory_WhenCreated_MovesToDontDestroyOnLoadScene()
        {
            ServiceLocator.SetFactory<IPersistentActorDummyService>(
                DefaultServiceActorFactory<PersistentActorDummy>.Instance);

            ServiceLocator.TryGet(out IPersistentActorDummyService result);

            Assert.That(result, Is.Not.Null);
            Assert.That(((Component)result).gameObject.scene.name, Is.EqualTo("DontDestroyOnLoad"));
        }

        /// <summary>A [PersistentService] instance found as a child object logs an error and is not persisted.</summary>
        [Test]
        public void PersistentActorFactory_WhenFoundAsChild_LogsError()
        {
            var parentObject = new GameObject("PersistentParent");
            var childObject = new GameObject("PersistentChild");
            childObject.transform.SetParent(parentObject.transform);
            PersistentActorDummy child = childObject.AddComponent<PersistentActorDummy>();

            ServiceLocator.SetFactory<IPersistentActorDummyService>(
                DefaultServiceActorFactory<PersistentActorDummy>.Instance);

            LogAssert.Expect(LogType.Error, new Regex(".*"));
            ServiceLocator.TryGet(out IPersistentActorDummyService result);

            Assert.That(result, Is.SameAs(child));

            Object.DestroyImmediate(parentObject);
        }

        /// <summary>A destroyed MonoBehaviour service no longer resolves.</summary>
        [Test]
        public void DestroyedMonoBehaviourService_IsTreatedAsInvalid()
        {
            var serviceObject = new GameObject("CorpseActorDummy");
            ActorDummy actor = serviceObject.AddComponent<ActorDummy>();

            ServiceLocator.SetService<IActorDummyService>(actor);
            Assert.That(ServiceLocator.TryGet(out IActorDummyService _), Is.True);

            Object.DestroyImmediate(serviceObject);

            bool resolved = ServiceLocator.TryGet(out IActorDummyService result);

            Assert.That(resolved, Is.False);
            Assert.That(result, Is.Null);
        }

        /// <summary>A live (non-MonoBehaviour) service is reported valid.</summary>
        [Test]
        public void ObjectUtility_IsValid_OnLiveService_ReturnsTrue()
        {
            var dummy = new DummyService();

            Assert.That(dummy.IsValid(), Is.True);
        }

        /// <summary>A destroyed MonoBehaviour is reported invalid (Unity "corpse" handling).</summary>
        [Test]
        public void ObjectUtility_IsValid_OnDestroyedMonoBehaviour_ReturnsFalse()
        {
            var serviceObject = new GameObject("DeadActorDummy");
            ActorDummy actor = serviceObject.AddComponent<ActorDummy>();

            Object.DestroyImmediate(serviceObject);

            Assert.That(((IService)actor).IsValid(), Is.False);
        }
    }
}
