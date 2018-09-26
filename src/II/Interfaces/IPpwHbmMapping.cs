// Copyright 2018 by PeopleWare n.v..
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using JetBrains.Annotations;

using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Impl;

using PPWCode.Vernacular.NHibernate.II.MappingByCode;

namespace PPWCode.Vernacular.NHibernate.II
{
    /// <summary>
    ///     This is a wrapper around the mapping by code class <see cref="HbmMapping" /> from nHibernate.
    /// </summary>
    public interface IPpwHbmMapping
    {
        /// <summary>
        ///     Get a <see cref="HbmMapping" /> instance or null, this can be used by hbmMapping by code.
        /// </summary>
        /// <returns>
        ///     A <see cref="HbmMapping" /> instance or null.
        /// </returns>
        [NotNull]
        HbmMapping HbmMapping { get; }

        /// <summary>
        ///     Get a <see cref="ModelMapper" /> instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="ModelMapper" /> instance.
        /// </returns>
        [NotNull]
        ModelMapper ModelMapper { get; }

        /// <summary>
        ///     Get an instance that implements <see cref="ICandidatePersistentMembersProvider" /> instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="ICandidatePersistentMembersProvider" /> instance.
        /// </returns>
        [NotNull]
        ICandidatePersistentMembersProvider MembersProvider { get; }

        /// <summary>
        ///     <p>If an identifier is a reserved word for a dialect, it needs to quoted.</p>
        ///     <p>
        ///         You can use this member for this purpose. The value of
        ///         <see cref="SimpleModelMapper.QuoteIdentifiers" />
        ///         is returned here.
        ///     </p>
        /// </summary>
        /// <returns>
        ///     Returns <see cref="SimpleModelMapper.QuoteIdentifiers" />
        /// </returns>
        bool QuoteIdentifiers { get; }

        bool UseCamelCaseUnderScoreForDbObjects { get; }

        [ContractAnnotation("null => null; notnull => notnull")]
        string CamelCaseToUnderscore(string camelCase);

        [ContractAnnotation("null => null; notnull => notnull")]
        string GetIdentifier(string identifier);

        [ContractAnnotation("identifier:null => null; identifier:notnull => notnull")]
        string ConditionalQuoteIdentifier(string identifier, bool? quoteIdentifier);
    }
}
