// Copyright 2017 by PeopleWare n.v..
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

using System.Collections.Generic;
using System.Diagnostics.Contracts;

using PPWCode.Vernacular.Persistence.II;
using PPWCode.Vernacular.NHibernate.I.Semantics;

namespace PPWCode.Vernacular.NHibernate.I.Tests.BiDirectionalNoCascading.Models
{
    public class Keyword : PersistentObject<int>
    {
        private string m_Name;
        private readonly ISet<Book> m_Books = new HashSet<Book>();

        public Keyword()
        {
        }

        public Keyword(int id)
            : base(id)
        {
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(Books != null);
        }

        public virtual string Name
        {
            get { return m_Name; }
            set
            {
                Contract.Ensures(Name == value);

                m_Name = value;
            }
        }

        public virtual ISet<Book> Books
        {
            get { return m_Books; }
        }

        public virtual void AddBook(Book book)
        {
            Contract.Ensures(book == null || Books.Contains(book));

            if (book != null && m_Books.Add(book))
            {
                book.AddKeyword(this);
            }
        }

        public virtual void RemoveBook(Book book)
        {
            Contract.Ensures(book == null || !Books.Contains(book));

            if (book != null && m_Books.Remove(book))
            {
                book.RemoveKeyword(this);
            }
        }
    }
}