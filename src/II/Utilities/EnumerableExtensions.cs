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
using System.Linq;

namespace PPWCode.Vernacular.NHibernate.II
{
    internal static class EnumerableExtensions
    {
        private static IEnumerable<IGrouping<int, T>> SegmentIterator<T>(
            IEnumerable<T> source,
            int segments)
        {
            int count = source.Count();
            int perSegment = (int)Math.Ceiling(count / (decimal)segments);
            Grouping<int, T>[] groups = new Grouping<int, T>[segments];
            for (int index1 = 0; index1 < segments; ++index1)
            {
                Grouping<int, T> grouping =
                    new Grouping<int, T>(perSegment)
                    {
                        Key = index1 + 1
                    };
                groups[index1] = grouping;
            }

            int index = 0;
            int segment = 1;
            Grouping<int, T> group = groups[0];
            foreach (T element in source)
            {
                group.Add(element);
                ++index;
                if ((segment < segments) && (index == perSegment))
                {
                    yield return group;
                    index = 0;
                    ++segment;
                    group = groups[segment - 1];
                }
            }

            for (; segment <= segments; ++segment)
            {
                yield return groups[segment - 1];
            }
        }

        public static IEnumerable<IGrouping<int, T>> Segment<T>(
            this IEnumerable<T> source,
            int segments)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (segments <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(segments));
            }

            return SegmentIterator(source, segments);
        }
    }
}
