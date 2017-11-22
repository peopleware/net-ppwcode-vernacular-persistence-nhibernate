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

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Tests.BiDirectionalNoCascading.Models
{
    [Serializable, DataContract(IsReference = true)]
    public class Book : PersistentObject<int>
    {
        private string m_Name;
        private Author m_Author;
        private readonly ISet<Keyword> m_Keywords = new HashSet<Keyword>();

        public Book()
        {
        }

        public Book(int id)
            : base(id)
        {
        }

        public virtual string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public virtual Author Author
        {
            get { return m_Author; }
            set
            {
                if (m_Author != value)
                {
                    if (m_Author != null)
                    {
                        Author previousAuthor = m_Author;
                        m_Author = null;
                        previousAuthor.RemoveBook(this);
                    }

                    m_Author = value;
                    if (m_Author != null)
                    {
                        m_Author.AddBook(this);
                    }
                }
            }
        }

        public virtual ISet<Keyword> Keywords
        {
            get { return m_Keywords; }
        }

        public virtual void AddKeyword(Keyword keyword)
        {
            if (keyword != null && m_Keywords.Add(keyword))
            {
                keyword.AddBook(this);
            }
        }

        public virtual void RemoveKeyword(Keyword keyword)
        {
            if (keyword != null && m_Keywords.Remove(keyword))
            {
                keyword.RemoveBook(this);
            }
        }
    }
}
