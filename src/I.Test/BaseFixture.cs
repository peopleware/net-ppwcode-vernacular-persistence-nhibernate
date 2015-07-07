// Copyright 2014 by PeopleWare n.v..
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics.CodeAnalysis;

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

        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Ok in test code.")]
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