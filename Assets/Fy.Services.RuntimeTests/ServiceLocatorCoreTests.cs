using System;
using System.Text.RegularExpressions;
using Fy.Services;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Fy.Services.RuntimeTests
{
    /// <summary>Verifies the core ServiceLocator API: register, resolve, replace, checked-get, and reset.</summary>
    [TestFixture]
    [TestOf(typeof(ServiceLocator))]
    public sealed class ServiceLocatorCoreTests
    {
        private interface IFooService : IService
        {
            int Value { get; set; }
        }

        private sealed class FooService : IFooService
        {
            public int Value { get; set; }
            public bool WasDisposed { get; private set; }

            public void Dispose()
            {
                WasDisposed = true;
            }
        }

        [DynamicService]
        private interface IDifficultyDummyService : IService { }

        private sealed class DifficultyDummy : IDifficultyDummyService
        {
            public bool WasDisposed { get; private set; }

            public void Dispose()
            {
                WasDisposed = true;
            }
        }

        private interface IOptionalDummyService : IService { }

        private sealed class OptionalDummy : IOptionalDummyService
        {
            public void Dispose() { }
        }

        [RequiredService]
        private interface IRequiredDummyService : IService { }

        private sealed class RequiredDummy : IRequiredDummyService
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

        /// <summary>A manually registered service resolves back as the same instance.</summary>
        [Test]
        public void SetService_ThenTryGet_ReturnsSameInstance()
        {
            var foo = new FooService();

            ServiceLocator.SetService<IFooService>(foo);
            bool resolved = ServiceLocator.TryGet(out IFooService result);

            Assert.That(resolved, Is.True);
            Assert.That(result, Is.SameAs(foo));
        }

        /// <summary>TryGet fails when nothing is registered for the type.</summary>
        [Test]
        public void TryGet_WithoutRegistration_ReturnsFalse()
        {
            bool resolved = ServiceLocator.TryGet(out IFooService result);

            Assert.That(resolved, Is.False);
            Assert.That(result, Is.Null);
        }

        /// <summary>A registered factory is reported and creates the service lazily on first request.</summary>
        [Test]
        public void SetFactory_RegistersFactory_AndResolvesLazily()
        {
            Assert.That(ServiceLocator.HasFactory<IFooService>(), Is.False);

            ServiceLocator.SetFactory<IFooService>(DefaultServiceFactory<FooService>.Instance);

            Assert.That(ServiceLocator.HasFactory<IFooService>(), Is.True);

            bool resolved = ServiceLocator.TryGet(out IFooService result);

            Assert.That(resolved, Is.True);
            Assert.That(result, Is.Not.Null);
        }

        /// <summary>Re-registering a dynamic service swaps it in and disposes of the previous one.</summary>
        [Test]
        public void SetService_OnDynamicService_ReplacesAndDisposesPrevious()
        {
            var first = new DifficultyDummy();
            var second = new DifficultyDummy();

            ServiceLocator.SetService<IDifficultyDummyService>(first);
            ServiceLocator.SetService<IDifficultyDummyService>(second);
            ServiceLocator.TryGet(out IDifficultyDummyService result);

            Assert.That(first.WasDisposed, Is.True);
            Assert.That(second.WasDisposed, Is.False);
            Assert.That(result, Is.SameAs(second));
        }

        /// <summary>Re-registering a non-dynamic service is rejected and the original is kept.</summary>
        [Test]
        public void SetService_DuplicateOnNonDynamicService_KeepsOriginal()
        {
            var first = new FooService();
            var second = new FooService();

            ServiceLocator.SetService<IFooService>(first);

            LogAssert.Expect(LogType.Error, new Regex(".*"));
            ServiceLocator.SetService<IFooService>(second);

            ServiceLocator.TryGet(out IFooService result);

            Assert.That(result, Is.SameAs(first));
        }

        /// <summary>GetChecked returns a registered required service without warning.</summary>
        [Test]
        public void GetChecked_OnRequiredService_ReturnsInstance()
        {
            var required = new RequiredDummy();

            ServiceLocator.SetService<IRequiredDummyService>(required);
            IRequiredDummyService result = ServiceLocator.GetChecked<IRequiredDummyService>();

            Assert.That(result, Is.SameAs(required));
        }

        /// <summary>GetChecked resolves a required service lazily through its registered factory.</summary>
        [Test]
        public void GetChecked_ResolvesRequiredServiceThroughFactory()
        {
            ServiceLocator.SetFactory<IRequiredDummyService>(DefaultServiceFactory<RequiredDummy>.Instance);

            IRequiredDummyService result = ServiceLocator.GetChecked<IRequiredDummyService>();

            Assert.That(result, Is.Not.Null);
        }

        /// <summary>GetChecked warns for a non-required type but still returns the instance.</summary>
        [Test]
        public void GetChecked_OnNonRequiredService_LogsWarningAndReturnsInstance()
        {
            var dummy = new OptionalDummy();

            ServiceLocator.SetService<IOptionalDummyService>(dummy);

            LogAssert.Expect(LogType.Warning, new Regex(".*"));
            IOptionalDummyService result = ServiceLocator.GetChecked<IOptionalDummyService>();

            Assert.That(result, Is.SameAs(dummy));
        }

        /// <summary>GetChecked throws when the requested service is missing.</summary>
        [Test]
        public void GetChecked_OnMissingRequiredService_Throws()
        {
            Assert.That(ServiceLocator.GetChecked<IRequiredDummyService>,
                Throws.InstanceOf<InvalidOperationException>());
        }

        /// <summary>Reset disposes of registered services and clears all registrations.</summary>
        [Test]
        public void Reset_DisposesAndClearsServices()
        {
            var foo = new FooService();

            ServiceLocator.SetService<IFooService>(foo);
            ServiceLocator.Reset();

            Assert.That(foo.WasDisposed, Is.True);
            Assert.That(ServiceLocator.TryGet(out IFooService _), Is.False);
        }
    }
}
