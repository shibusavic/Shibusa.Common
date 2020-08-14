using Shibusa.Data.Abstractions;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Shibusa.Data.UnitTests
{
    public class SqlWhereTests
    {
        private readonly ITestOutputHelper testOutputHelper;
        private const string DATE_FORMAT = "yyyy-MM-dd HH:mm:ss.fffffff";

        public SqlWhereTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Where_MakeFriendlyFalse_NoChange()
        {
            // makeFriendly:false leaves the right side as-is.
            SqlWhere where = new SqlWhere().Where(left: "Name", comparison: Comparison.Equal,
                right: "@name", makeFriendly: false);

            Assert.Equal("Name = @name", where.Raw);
        }

        [Fact]
        public void Where_MakeFriendlyTrue_Change()
        {
            SqlWhere where = new SqlWhere().Where(left: "Name", comparison: Comparison.Equal,
                right: "@name", makeFriendly: true);

            Assert.Equal("Name = '@name'", where.Raw);
        }

        [Fact]
        public void Where_PassClause_PassThrough()
        {
            // Any string will do to prove the transfer.
            string expected = "[Name] LIKE %PEACE%";

            SqlWhere where = new SqlWhere(expected);

            SqlWhere otherClause = new SqlWhere(where);

            Assert.Equal(where.Raw, otherClause.Raw);

            Assert.Equal(expected, where.Raw);
        }

        [Fact]
        public void ComplexQuery()
        {
            DateTime start = DateTime.Now.AddDays(-1);
            DateTime finish = DateTime.Now.AddDays(1);

            string expected = $"((((Name IS NULL AND Age > 30) OR (Name LIKE '%e%' AND Age < 30)) AND BankBalance BETWEEN 0 AND 500000) OR (TimeCreated = '{start.ToString(DATE_FORMAT)}' OR TimeCreated = '{finish.ToString(DATE_FORMAT)}'))";

            SqlWhere query = new SqlWhere().WhereEqual(left: "Name", right: default(string))
                .And(new SqlWhere().WhereGreaterThan("Age", 30))
                .Or(new SqlWhere().WhereLike("Name", "%e%").And(new SqlWhere().WhereLessThan("Age", 30)))
                .And(new SqlWhere().WhereBetween("BankBalance", 0, 500000))
                .Or(new SqlWhere().WhereEqual("TimeCreated", start)
                    .Or(new SqlWhere().WhereEqual("TimeCreated", finish)));

            Assert.Equal(expected, query.Raw);
        }

        [Fact]
        public void OrderBy()
        {
            Dictionary<string, SortOrder> cols = new Dictionary<string, SortOrder>
            {
                { "Name", SortOrder.Ascending },
                { "BankBalance", SortOrder.Descending }
            };

            string actual = SqlOrderBy.Create(cols);
            Assert.Equal("Name ASC, BankBalance DESC", actual);
        }

        [Fact]
        public void SqlFor_All()
        {
            string actual = SqlFor.Create(TemporalComparison.All);

            Assert.Equal("FOR SYSTEM_TIME ALL", actual);
        }

        [Fact]
        public void SqlFor_AsOf()
        {
            DateTime date = DateTime.Now;

            string actual = SqlFor.Create(TemporalComparison.AsOf, date);

            testOutputHelper.WriteLine(actual);

            Assert.Equal($"FOR SYSTEM_TIME AS OF '{date.ToString(DATE_FORMAT)}'", actual);
        }

        [Fact]
        public void SqlFor_Between()
        {
            DateTime start = DateTime.Now.AddDays(-1);
            DateTime finish = DateTime.Now.AddDays(1);

            string actual = SqlFor.Create(TemporalComparison.Between, start, finish);
            string expected = $"FOR SYSTEM_TIME BETWEEN '{start.ToString(DATE_FORMAT)}' AND '{finish.ToString(DATE_FORMAT)}'";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SqlFor_ContainedIn()
        {
            DateTime start = DateTime.Now.AddDays(-1);
            DateTime finish = DateTime.Now.AddDays(1);

            string actual = SqlFor.Create(TemporalComparison.ContainedIn, start,finish);
            string expected = $"FOR SYSTEM_TIME CONTAINED IN ('{start.ToString(DATE_FORMAT)}','{finish.ToString(DATE_FORMAT)}')";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SqlFor_FromTo()
        {
            DateTime start = DateTime.Now.AddDays(-1);
            DateTime finish = DateTime.Now.AddDays(1);

            string actual = SqlFor.Create(TemporalComparison.FromTo, start, finish);
            string expected = $"FOR SYSTEM_TIME FROM '{start.ToString(DATE_FORMAT)}' TO '{finish.ToString(DATE_FORMAT)}'";
            Assert.Equal(expected, actual);
        }
    }
}
