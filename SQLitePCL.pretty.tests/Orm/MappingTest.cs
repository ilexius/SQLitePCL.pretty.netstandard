// Copyright (c) 2009-2015 Krueger Systems, Inc.
// Copyright (c) 2015 David Bordoley
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Linq;
using NUnit.Framework;

using SQLitePCL.pretty.Orm;
using SQLitePCL.pretty.Orm.Attributes;

namespace SQLitePCL.pretty.tests
{
    [TestFixture]
    public class MappingTest
    {
        [Table ("AGoodTableName")]
        class AFunnyTableName
        {
            [PrimaryKey]
            public int Id { get; set; }

            [Column("AGoodColumnName")]
            public string AFunnyColumnName { get; set; }
        }
            
        [Test]
        public void HasGoodNames()
        {
            var table = TableMapping.Create<AFunnyTableName>();
         
            Assert.AreEqual("AGoodTableName", table.TableName);
            Assert.True(table.ContainsKey("AGoodColumnName"));
            Assert.False(table.ContainsKey("AFunnyColumnName"));
        }

        [Table("foo")]
        public class Foo
        {
            [Column("baz")]
            public int Bar { get; set; }
        }

        [Test]
        public void Issue86()
        {
            var table = TableMapping.Create<Foo>();

            using (var db = SQLite3.OpenInMemory())
            {
                db.InitTable(table);
                db.Insert(table, new Foo { Bar = 42 });
                db.Insert(table, new Foo { Bar = 69 });

                var found42 = db.Query(table.CreateQuery().Where(f => f.Bar == default(int)), 42).FirstOrDefault();
                Assert.IsNotNull(found42);

                var ordered = db.Query(table.CreateQuery().OrderByDescending(f => f.Bar)).ToList();
                Assert.AreEqual(2, ordered.Count);
                Assert.AreEqual(69, ordered[0].Bar);
                Assert.AreEqual(42, ordered[1].Bar);
            }
        }
    }
}
