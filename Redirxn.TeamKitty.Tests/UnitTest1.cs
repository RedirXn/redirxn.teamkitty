using NUnit.Framework;
using Redirxn.TeamKitty.Models;

namespace Redirxn.TeamKitty.Tests
{
    [TestFixture]
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var k = new Kitty();
            Assert.Pass();
        }
    }
}