/*
   Copyright 2014 David Bordoley
   Copyright 2014 Zumero, LLC

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace SQLitePCL.pretty
{
    [ContractClassFor(typeof(IDatabaseConnection))]
    internal abstract class IDatabaseConnectionContract : IDatabaseConnection
    {
        public abstract event EventHandler Rollback;

        public abstract event EventHandler<DatabaseTraceEventArgs> Trace;

        public abstract event EventHandler<DatabaseProfileEventArgs> Profile;

        public abstract event EventHandler<DatabaseUpdateEventArgs> Update;

        public abstract bool IsAutoCommit { get; }

        public abstract TimeSpan BusyTimeout { set; }

        public abstract int Changes { get; }

        public abstract IEnumerable<IStatement> Statements { get; }

        public abstract void Dispose();

        public string GetFileName(string database)
        {
            Contract.Requires(database != null);
            return default(string);
        }

        public IStatement PrepareStatement(string sql, out string tail)
        {
            Contract.Requires(sql != null);
            tail = default(string);
            return default(IStatement);
        }

        public void RegisterCollation(string name, Comparison<string> comparison)
        {
             Contract.Requires(name != null);
             Contract.Requires(comparison != null);
        }

        public void RegisterCommitHook(Func<bool> onCommit)
        {
             Contract.Requires(onCommit != null);
        }

        public void RegisterAggregateFunc<T>(string name, int nArg, T seed, Func<T, IReadOnlyList<ISQLiteValue>, T> func, Func<T, ISQLiteValue> resultSelector)
        {
            Contract.Requires(name != null);
            Contract.Requires(func != null);
            Contract.Requires(resultSelector != null);
            Contract.Requires(nArg >= -1);
        }

        public void RegisterScalarFunc(string name, int nArg, Func<IReadOnlyList<ISQLiteValue>, ISQLiteValue> reduce)
        {
            Contract.Requires(name != null);
            Contract.Requires(reduce != null);
            Contract.Requires(nArg >= -1); 
        }
    }
}