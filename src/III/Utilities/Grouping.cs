// Copyright  by PeopleWare n.v..
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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace PPWCode.Vernacular.NHibernate.III
{
    internal class Grouping<TKey, TElement>
        : IGrouping<TKey, TElement>,
          IList<TElement>
    {
        private int _count;
        private TElement[] _elements;

        public Grouping(int count)
        {
            _elements = new TElement[count];
        }

        internal TKey Key { get; set; }

        public IEnumerator<TElement> GetEnumerator()
        {
            for (int i = 0; i < _count; ++i)
            {
                yield return _elements[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        TKey IGrouping<TKey, TElement>.Key
            => Key;

        int ICollection<TElement>.Count
            => _count;

        bool ICollection<TElement>.IsReadOnly
            => true;

        void ICollection<TElement>.Add(TElement item)
        {
            throw new ReadOnlyException();
        }

        void ICollection<TElement>.Clear()
        {
            throw new ReadOnlyException();
        }

        bool ICollection<TElement>.Contains(TElement item)
            => Array.IndexOf(_elements, item, 0, _count) >= 0;

        void ICollection<TElement>.CopyTo(TElement[] array, int arrayIndex)
            => Array.Copy(_elements, 0, array, arrayIndex, _count);

        bool ICollection<TElement>.Remove(TElement item)
            => throw new ReadOnlyException();

        int IList<TElement>.IndexOf(TElement item)
            => Array.IndexOf(_elements, item, 0, _count);

        void IList<TElement>.Insert(int index, TElement item)
            => throw new ReadOnlyException();

        void IList<TElement>.RemoveAt(int index)
            => throw new NotSupportedException();

        TElement IList<TElement>.this[int index]
        {
            get
            {
                if ((index < 0) || (index >= _count))
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return _elements[index];
            }
            set => throw new ReadOnlyException();
        }

        internal void Add(TElement element)
        {
            if (_elements.Length == _count)
            {
                Array.Resize(ref _elements, _count * 2);
            }

            _elements[_count++] = element;
        }
    }
}
