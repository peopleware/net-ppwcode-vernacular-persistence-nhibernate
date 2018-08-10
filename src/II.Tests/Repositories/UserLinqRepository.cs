﻿// Copyright 2018 by PeopleWare n.v..
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Linq;

using PPWCode.Vernacular.NHibernate.II.Interfaces;
using PPWCode.Vernacular.NHibernate.II.Tests.Models;

namespace PPWCode.Vernacular.NHibernate.II.Tests.Repositories
{
    public class UserLinqRepository
        : TestLinqRepository<User>,
          IUserLinqRepository
    {
        public UserLinqRepository(ISessionProvider sessionProvider)
            : base(sessionProvider)
        {
        }

        public User GetUserByName(string name)
        {
            return Get(qry => qry.Where(u => u.Name == name));
        }
    }
}
