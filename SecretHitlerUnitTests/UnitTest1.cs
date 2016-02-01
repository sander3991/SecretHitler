using System;
using System.Drawing;
using SecretHitler.Networking;
using SecretHitler.Objects;
using SecretHitler.Logic;
using SecretHitler.Views;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SecretHitlerUnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestExtensions()
        {
            // Check Points in Rectangle
            var rect = new Rectangle(0, 0, 200, 200);
            Assert.IsTrue(rect.IsPointIn(new Point(100, 100)));
            Assert.IsTrue(rect.IsPointIn(new Point(200, 200)));
            Assert.IsFalse(rect.IsPointIn(new Point(201, 201)));
            Assert.IsTrue(rect.IsPointIn(new Point(0, 0)));
            Assert.IsFalse(rect.IsPointIn(new Point(-1, -1)));

            //Check Point Relative To Point
            var point1 = new Point(100, 100);
            var point2 = new Point(200, 200);
            Assert.AreEqual(point2.RelativeTo(point1), point1);
            Assert.AreNotEqual(point1.RelativeTo(point2), point1);
            Assert.AreEqual(point1.RelativeTo(point2), new Point(-100, -100));
            Assert.AreEqual(point1.RelativeTo(new Point(50, 50)), new Point(50, 50));
        }
    }
}
