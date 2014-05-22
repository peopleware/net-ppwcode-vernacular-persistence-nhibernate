using System;

using log4net;
using log4net.Config;

using NUnit.Framework;

namespace PPWCode.Vernacular.NHibernate.I.Test
{
    [TestFixture]
    public abstract class BaseFixture
    {
        [SetUp]
        public void Setup()
        {
            OnSetup();
        }

        [TearDown]
        public void Teardown()
        {
            OnTeardown();
        }

        protected static ILog Log =
            new Func<ILog>(
                () =>
                {
                    XmlConfigurator.Configure();
                    return LogManager.GetLogger(typeof(BaseFixture));
                }).Invoke();

        protected virtual void OnFixtureSetup()
        {
        }

        protected virtual void OnFixtureTeardown()
        {
        }

        protected virtual void OnSetup()
        {
        }

        protected virtual void OnTeardown()
        {
        }

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            OnFixtureSetup();
        }

        [TestFixtureTearDown]
        public void FixtureTeardown()
        {
            OnFixtureTeardown();
        }
    }
}