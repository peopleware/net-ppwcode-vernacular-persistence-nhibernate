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

using System.Linq;

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.III.Tests.IntegrationTests.Async.QueryOver.BiDirectionalNoCascading.Repositories;
using PPWCode.Vernacular.NHibernate.III.Tests.Model.BiDirectionalNoCascading;

namespace PPWCode.Vernacular.NHibernate.III.Tests.IntegrationTests.Async.QueryOver.BiDirectionalNoCascading
{
    public class BiDirectionalNoCascadingTest : BaseRepositoryTests<Author>
    {
        public AuthorRepository AuthorRepository
            => new AuthorRepository(SessionProvider);

        public BookRepository BookRepository
            => new BookRepository(SessionProvider);

        public KeywordRepository KeywordRepository
            => new KeywordRepository(SessionProvider);

        /// <summary>
        ///     Override this method for setup code that needs to run for each test separately.
        /// </summary>
        protected override void OnSetup()
        {
            base.OnSetup();
            SessionFactory.Statistics.Clear();
        }

        [Test]
        public void TestBiDirectionalManyToManyWithMerge()
        {
            RunInsideTransaction(
                () =>
                {
                    Book inDepth =
                        new Book
                        {
                            Name = "C# in Depth",
                            Author = null
                        };
                    Book mergedInDepth = BookRepository.Merge(inDepth);

                    Assert.IsTrue(inDepth.IsTransient, "Original Book object is unchanged: did not get a primary key.");
                    Assert.IsFalse(mergedInDepth.IsTransient, "Result of the merge is a new object with primary key.");
                    Assert.AreNotEqual(mergedInDepth, inDepth);

                    Keyword csharp =
                        new Keyword
                        {
                            Name = "C#"
                        };
                    csharp.AddBook(mergedInDepth);
                    Keyword mergedCsharp = KeywordRepository.Merge(csharp);

                    Assert.IsTrue(csharp.IsTransient, "Original Keyword object is unchanged: did not get a primary key.");
                    Assert.IsFalse(mergedCsharp.IsTransient, "Result of the merge is a new object with primary key.");
                    Assert.AreNotEqual(mergedCsharp, csharp);

                    Assert.AreNotEqual(mergedCsharp, mergedCsharp.Books.Single().Keywords.Single(), "Merged Keyword object does not have a bi-directional link with the Book object.");
                    Assert.IsTrue(mergedCsharp.Books.Single().Keywords.Single().IsTransient, "Book object still points to the not persisted Keyword object.");

                    Assert.AreEqual(1, mergedInDepth.Keywords.Count);
                },
                true);
        }

        [Test]
        public void TestBiDirectionalManyToManyWithSaveOrUpdate()
        {
            RunInsideTransaction(
                () =>
                {
                    Book inDepth =
                        new Book
                        {
                            Name = "C# in Depth",
                            Author = null
                        };
                    BookRepository.SaveOrUpdate(inDepth);

                    Assert.IsFalse(inDepth.IsTransient, "Original Book object is persisted and has now a primary key.");

                    Keyword csharp =
                        new Keyword
                        {
                            Name = "C#"
                        };
                    csharp.AddBook(inDepth);
                    KeywordRepository.SaveOrUpdate(csharp);

                    Assert.IsFalse(csharp.IsTransient, "Original Keyword object is persisted and has now a primary key.");
                    Assert.AreEqual(csharp, inDepth.Keywords.Single(), "There is a valid and consistent bi-directional link between the original Book and Keyword.");
                },
                true);
        }

        [Test]
        public void TestBiDirectionalOneToManyWithMerge()
        {
            RunInsideTransaction(
                () =>
                {
                    Author john =
                        new Author
                        {
                            Name = "John Skeet"
                        };
                    Author mergedJohn = AuthorRepository.Merge(john);

                    Assert.IsTrue(john.IsTransient, "Original Author object is unchanged: did not get a primary key.");
                    Assert.IsFalse(mergedJohn.IsTransient, "Result of the merge is a new object with primary key.");
                    Assert.AreNotEqual(mergedJohn, john);

                    Book inDepth =
                        new Book
                        {
                            Name = "C# in Depth",
                            Author = mergedJohn
                        };
                    Book mergedInDepth = BookRepository.Merge(inDepth);

                    Assert.IsTrue(inDepth.IsTransient, "Original Book object is unchanged: did not get a primary key.");
                    Assert.IsFalse(mergedInDepth.IsTransient, "Result of the merge is a new object with primary key.");
                    Assert.AreNotEqual(mergedInDepth, inDepth);

                    Assert.AreNotEqual(mergedInDepth, mergedInDepth.Author.Books.Single(), "Merged Book object does not have a bi-directional link with the Author object.");
                    Assert.IsTrue(mergedJohn.Books.Single().IsTransient, "Author object still points to the not persisted Book object.");

                    Assert.AreEqual(1, mergedJohn.Books.Count);
                },
                true);
        }

        [Test]
        public void TestBiDirectionalOneToManyWithSaveOrUpdate()
        {
            RunInsideTransaction(
                () =>
                {
                    Author john =
                        new Author
                        {
                            Name = "John Skeet"
                        };
                    AuthorRepository.SaveOrUpdate(john);

                    Assert.IsFalse(john.IsTransient, "Original Author object is persisted and has now a primary key.");

                    Book inDepth =
                        new Book
                        {
                            Name = "C# in Depth",
                            Author = john
                        };
                    BookRepository.SaveOrUpdate(inDepth);

                    Assert.IsFalse(inDepth.IsTransient, "Original Book object is persisted and has now a primary key.");
                    Assert.AreEqual(inDepth, inDepth.Author.Books.Single(), "There is a valid and consistent bi-directional link between the original Author and Book.");
                },
                true);
        }
    }
}
