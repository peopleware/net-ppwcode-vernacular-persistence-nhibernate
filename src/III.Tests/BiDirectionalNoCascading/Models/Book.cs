// Copyright 2017 by PeopleWare n.v..
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

namespace PPWCode.Vernacular.NHibernate.III.Tests.BiDirectionalNoCascading.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Book : PersistentObject<int>
    {
        [DataMember]
        private readonly ISet<Keyword> _keywords = new HashSet<Keyword>();

        [DataMember]
        private Author _author;

        public Book()
        {
        }

        public Book(int id)
            : base(id)
        {
        }

        [DataMember]
        public virtual string Name { get; set; }

        public virtual Author Author
        {
            get => _author;
            set
            {
                if (_author != value)
                {
                    if (_author != null)
                    {
                        Author previousAuthor = _author;
                        _author = null;
                        previousAuthor.RemoveBook(this);
                    }

                    _author = value;
                    _author?.AddBook(this);
                }
            }
        }

        [AuditLogPropertyIgnore]
        public virtual ISet<Keyword> Keywords
            => _keywords;

        public virtual void AddKeyword(Keyword keyword)
        {
            if ((keyword != null) && Keywords.Add(keyword))
            {
                keyword.AddBook(this);
            }
        }

        public virtual void RemoveKeyword(Keyword keyword)
        {
            if ((keyword != null) && Keywords.Remove(keyword))
            {
                keyword.RemoveBook(this);
            }
        }
    }

    [UsedImplicitly]
    public class BookMapper : PersistentObjectMapper<Book, int>
    {
        public BookMapper()
        {
            Property(b => b.Name);

            ManyToOne(b => b.Author);

            Set(
                b => b.Keywords,
                m =>
                {
                    m.Inverse(true);
                    m.Cascade(Cascade.None);
                },
                r => r.ManyToMany());
        }
    }
}
