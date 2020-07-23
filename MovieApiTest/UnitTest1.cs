using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MovieApiTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var movieDbProvider = new TVShower.MovieDbApi.MovieDbProvider();
            var title = movieDbProvider.GetTvShow(15768);
            Assert.AreEqual("The Guild", title);
        }

        [TestMethod]
        [DataRow("The Guild", 15768, DisplayName = "Good Show")]
        [DataRow("Some stupid non existing show", -1, DisplayName = "Non-Existing Show")]

        public void TestMethod2(string tvShowName, int id)
        {
            var movieDbProvider = new TVShower.MovieDbApi.MovieDbProvider();
            var idFromDB = movieDbProvider.SearchTvShow(tvShowName);
            Assert.AreEqual(id, idFromDB);
        }
    }
}
