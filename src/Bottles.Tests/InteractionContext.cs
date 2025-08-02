using NUnit.Framework;
using StructureMap.AutoMocking;

namespace Bottles.Tests
{
    public abstract class InteractionContext<T>
        where T : class
    {
        public RhinoAutoMocker<T> Services { get; private set; }

        public T ClassUnderTest
        {
            get { return Services.ClassUnderTest; }
        }

        public void Use<TService>(TService instance)
        {
            Services.Inject(instance);
        }

        public TService MockFor<TService>()
            where TService : class
        {
            return Services.Get<TService>();
        }

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            beforeAll();
        }

        [SetUp]
        public void SetUp()
        {
            Services = new RhinoAutoMocker<T>(MockMode.AAA);

            beforeEach();
        }

        [TearDown]
        public void TearDown()
        {
            afterEach();
        }

        protected virtual void beforeEach() { }

        protected virtual void afterEach() { }

        protected virtual void beforeAll() { }
    }
}