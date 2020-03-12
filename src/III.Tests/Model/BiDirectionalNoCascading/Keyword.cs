// Copyright 2020 by PeopleWare n.v..
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using JetBrains.Annotations;

using NHibernate.Mapping.ByCode;

using PPWCode.Vernacular.NHibernate.III.MappingByCode;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.Tests.Model.BiDirectionalNoCascading
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Keyword : PersistentObject<int>
    {
        [DataMember]
        private readonly ISet<Book> _books = new HashSet<Book>();

        public Keyword()
        {
        }

        public Keyword(int id)
            : base(id)
        {
        }

        [DataMember]
        public virtual string Name { get; set; }

        [AuditLogPropertyIgnore]
        public virtual ISet<Book> Books
            => _books;

        public virtual void AddBook(Book book)
        {
            if ((book != null) && Books.Add(book))
            {
                book.AddKeyword(this);
            }
        }

        public virtual void RemoveBook(Book book)
        {
            if ((book != null) && Books.Remove(book))
            {
                book.RemoveKeyword(this);
            }
        }
    }

    [UsedImplicitly]
    public class KeywordMapper : PersistentObjectMapper<Keyword, int>
    {
        public KeywordMapper()
        {
            Property(k => k.Name);

            Set(
                k => k.Books,
                m => m.Cascade(Cascade.None),
                r => r.ManyToMany());
        }
    }
}
