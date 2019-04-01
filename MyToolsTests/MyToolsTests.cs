/**
 * Script : Jeux de tests pour la bibliothèque de classes MyTools, en particulier en ce qui concerne les opérations sur les dates.
 * Author : Alice BORD
 * Email : alice.bord1@gmail.com
 * Date : 31/03/2019
 */

using System;
using Xunit;
using MyTools;

namespace MyToolsTests
{
    public class DateManagementTests
    {
        [Theory]
        [InlineData(1, "12")]
        [InlineData(10, "09")]
        [InlineData(9, "08")]
        [InlineData(12, "11")]
        public void GetPreviousMonthTests(int mois, string expectedResult)
        {
            // Act.
            var actualResult = MyTools.DateManagement.GetPreviousMonth(new DateTime(2019, mois, 8));
            // Assert.
            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData(12, "01")]
        [InlineData(8, "09")]
        [InlineData(11, "12")]
        [InlineData(9, "10")]
        public void GetNexMonth(int mois, string expectedResult)
        {
            // Act.
            var actualResult = MyTools.DateManagement.GetNextMonth(new DateTime(2019, mois, 8));
            // Assert.
            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData(10, 8, 12, false)]
        [InlineData(10, 8, 9, true)]
        [InlineData(10, 8, 5, false)]
        [InlineData(15, 30, 22, true)]
        [InlineData(1, 10, 1, true)]
        [InlineData(1, 10, 10, true)]
        public void Between(int day1, int day2, int day, bool expectedResult)
        {
            // Act. 
            var actualResult = MyTools.DateManagement.Between(day1, day2, new DateTime(2019, 3, day));
            // Assert.
            Assert.Equal(expectedResult, actualResult);
        }
    }
}
