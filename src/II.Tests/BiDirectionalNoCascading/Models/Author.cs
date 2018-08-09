﻿// Copyright 2017-2018 by PeopleWare n.v..
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
using System.Collections.Generic;
using System.Runtime.Serialization;

using NHibernate.Mapping.ByCode;

using PPWCode.Vernacular.NHibernate.I.MappingByCode;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Tests.BiDirectionalNoCascading.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Author : PersistentObject<int>
    {
        private readonly ISet<Book> m_Books = new HashSet<Book>();
        private string m_Name;

        public Author()
        {
        }

        public Author(int id)
            : base(id)
        {
        }

        public virtual string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public virtual ISet<Book> Books
            => m_Books;

        public virtual void AddBook(Book book)
        {
            if ((book != null) && m_Books.Add(book))
            {
                book.Author = this;
            }
        }

        public virtual void RemoveBook(Book book)
        {
            if ((book != null) && m_Books.Remove(book))
            {
                book.Author = null;
            }
        }
    }

    public class AuthorMapper : PersistentObjectMapper<Author, int>
    {
        public AuthorMapper()
        {
            Property(a => a.Name);

            Set(
                c => c.Books,
                m =>
                {
                    m.Inverse(true);
                    m.Cascade(Cascade.None);
                },
                r => r.OneToMany(m => m.Class(typeof(Book))));
        }
    }
}