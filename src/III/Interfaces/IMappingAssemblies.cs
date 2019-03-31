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

using System.Collections.Generic;
using System.Reflection;

using JetBrains.Annotations;

using NHibernate.Mapping.ByCode;

namespace PPWCode.Vernacular.NHibernate.III
{
    /// <summary>
    ///     Used to determine where we can find our hbm definition for our models.
    /// </summary>
    public interface IMappingAssemblies
    {
        /// <summary>
        ///     Returns all assemblies:
        ///     <list type="bullet">
        ///         <item>
        ///             <description>with embedded hbm-files</description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 when the hbmMapping is done by code, all assemblies that
        ///                 contains the necessary <see cref="IConformistHoldersProvider" /> that are used to create the hbmMapping
        ///                 by code
        ///             </description>
        ///         </item>
        ///     </list>
        /// </summary>
        /// <returns>
        ///     Sequence of <see cref="Assembly" />
        /// </returns>
        [NotNull]
        IEnumerable<Assembly> GetAssemblies();
    }
}
