using System;
using System.Linq;
using Fy.Services;
using NUnit.Framework;

namespace Fy.Services.RuntimeTests
{
    /// <summary>Verifies the internal EnumerateServices snapshot API used by the Service Locator window.</summary>
    [TestFixture]
    [TestOf(typeof(ServiceLocator))]
    public sealed class ServiceSnapshotTests
    {
        private interface ISnapshotDummyService : IService
        {
            int Value { get; set; }
        }

        private sealed class SnapshotDummy : ISnapshotDummyService
        {
            public int Value { get; set; }

            public void Dispose() { }
        }

        [DynamicService]
        private interface IDynamicDummyService : IService { }

        private sealed class DynamicDummy : IDynamicDummyService
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

        /// <summary>A manually set service is reported with its live value and no factory.</summary>
        [Test]
        public void EnumerateServices_AfterSetService_ReportsResolvedValueAndNoFactory()
        {
            var dummy = new SnapshotDummy();

            ServiceLocator.SetService<ISnapshotDummyService>(dummy);
            ServiceSnapshot snapshot = FindSnapshot(typeof(ISnapshotDummyService));

            Assert.That(snapshot.Value, Is.SameAs(dummy));
            Assert.That(snapshot.Factory, Is.Null);
        }

        /// <summary>A registered factory is reported before the service is resolved, with a null value.</summary>
        [Test]
        public void EnumerateServices_AfterSetFactory_ReportsFactoryAndNullValue()
        {
            ServiceLocator.SetFactory<ISnapshotDummyService>(DefaultServiceFactory<SnapshotDummy>.Instance);
            ServiceSnapshot snapshot = FindSnapshot(typeof(ISnapshotDummyService));

            Assert.That(snapshot.Value, Is.Null);
            Assert.That(snapshot.Factory, Is.SameAs(DefaultServiceFactory<SnapshotDummy>.Instance));
        }

        /// <summary>Once a factory resolves its service, the snapshot reports the cached value.</summary>
        [Test]
        public void EnumerateServices_AfterFactoryResolves_ReportsValue()
        {
            ServiceLocator.SetFactory<ISnapshotDummyService>(DefaultServiceFactory<SnapshotDummy>.Instance);
            ServiceLocator.TryGet(out ISnapshotDummyService _);

            ServiceSnapshot snapshot = FindSnapshot(typeof(ISnapshotDummyService));

            Assert.That(snapshot.Value, Is.Not.Null);
        }

        /// <summary>The snapshot mirrors the [DynamicService] flag from the service interface.</summary>
        [Test]
        public void EnumerateServices_OnDynamicService_SetsIsDynamic()
        {
            ServiceLocator.SetService<IDynamicDummyService>(new DynamicDummy());
            ServiceSnapshot snapshot = FindSnapshot(typeof(IDynamicDummyService));

            Assert.That(snapshot.IsDynamic, Is.True);
            Assert.That(snapshot.IsRequired, Is.False);
        }

        /// <summary>The snapshot mirrors the [RequiredService] flag from the service interface.</summary>
        [Test]
        public void EnumerateServices_OnRequiredService_SetsIsRequired()
        {
            ServiceLocator.SetService<IRequiredDummyService>(new RequiredDummy());
            ServiceSnapshot snapshot = FindSnapshot(typeof(IRequiredDummyService));

            Assert.That(snapshot.IsRequired, Is.True);
            Assert.That(snapshot.IsDynamic, Is.False);
        }

        /// <summary>Resetting the locator clears every snapshot.</summary>
        [Test]
        public void EnumerateServices_AfterReset_IsEmpty()
        {
            ServiceLocator.SetService<ISnapshotDummyService>(new SnapshotDummy());
            ServiceLocator.Reset();

            Assert.That(ServiceLocator.EnumerateServices(), Is.Empty);
        }

        private static ServiceSnapshot FindSnapshot(Type interfaceType)
        {
            return ServiceLocator.EnumerateServices()
                .First(snapshot => snapshot.InterfaceType == interfaceType);
        }
    }
}
