// Copyright 2015 by PeopleWare n.v..
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

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

using NHibernate;

namespace PPWCode.Vernacular.NHibernate.I.Interfaces
{
    [ContractClass(typeof(INHibernateSessionFactoryContract))]
    public interface INHibernateSessionFactory
    {
        ISessionFactory SessionFactory { get; }
    }

    // ReSharper disable once InconsistentNaming
    [ExcludeFromCodeCoverage, ContractClassFor(typeof(INHibernateSessionFactory))]
    internal abstract class INHibernateSessionFactoryContract : INHibernateSessionFactory
    {
        public ISessionFactory SessionFactory
        {
            get
            {
                Contract.Ensures(Contract.Result<ISessionFactory>() != null);

                return default(ISessionFactory);
            }
        }
    }
}