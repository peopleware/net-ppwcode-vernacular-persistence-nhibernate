// Copyright 2014 by PeopleWare n.v..
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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using NHibernate;
using NHibernate.Linq;

namespace PPWCode.Vernacular.NHibernate.I.Implementations
{
    public static class LinqExtensionsForNHibernate
    {
        private static NhQueryable<T> QueryableFrom<T>(IQueryable<T> queryable)
        {
            NhQueryable<T> nhQueryable = queryable as NhQueryable<T>;
            if (nhQueryable == null)
            {
                throw new InvalidOperationException("Underlying type behind this IQueryable is not NhQueryable!");
            }

            return nhQueryable;
        }

        public static IFutureValue<TResult> ToFutureValue<T, TResult>(this IQueryable<T> source, Expression<Func<IQueryable<T>, TResult>> selector)
            where TResult : struct
            where T : class
        {
            Contract.Requires(source != null);

            NhQueryable<T> qry = QueryableFrom(source);
            INhQueryProvider provider = (INhQueryProvider)qry.Provider;
            MethodInfo method = ((MethodCallExpression)selector.Body).Method;
            MethodCallExpression expression = Expression.Call(null, method, source.Expression);
            return (IFutureValue<TResult>)provider.ExecuteFuture(expression);
        }
    }
}