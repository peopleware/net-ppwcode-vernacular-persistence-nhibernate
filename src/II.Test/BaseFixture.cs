// Copyright 2017-2018 by PeopleWare n.v..
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

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            OnFixtureSetup();
        }

        [OneTimeTearDown]
        public void FixtureTeardown()
        {
            OnFixtureTeardown();
        }

        /// <summary>
        ///     Override this method for setup code that needs to run once for the class, independently of the tests.
        /// </summary>
        protected abstract void OnFixtureSetup();

        /// <summary>
        ///     Override this method for teardown code that needs to run once for the class, independently of the tests.
        /// </summary>
        protected abstract void OnFixtureTeardown();

        /// <summary>
        ///     Override this method for setup code that needs to run for each test separately.
        /// </summary>
        protected abstract void OnSetup();

        /// <summary>
        ///     Override this method for teardown code that needs to run for each test separately.
        /// </summary>
        protected abstract void OnTeardown();
    }
}
