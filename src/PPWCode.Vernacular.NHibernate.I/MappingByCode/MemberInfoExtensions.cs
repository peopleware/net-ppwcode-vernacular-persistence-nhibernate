// Copyright 2015 by PeopleWare n.v..
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
using System.Reflection;

namespace PPWCode.Vernacular.NHibernate.I.MappingByCode
{
    public static class MemberInfoExtensions
    {
        public static Type MemberType(this MemberInfo memberInfo)
        {
            Type memberType;
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    memberType = ((FieldInfo)memberInfo).FieldType;
                    break;
                case MemberTypes.Property:
                    memberType = ((PropertyInfo)memberInfo).PropertyType;
                    break;
                default:
                    memberType = null;
                    break;
            }

            return memberType;
        }
    }
}