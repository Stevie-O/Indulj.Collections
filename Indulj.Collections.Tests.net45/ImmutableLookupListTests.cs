using Indulj.Collections.Immutable;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Indulj.Collections.Tests.net45
{
    [TestFixture]
    public class ImmutableLookupListTests
    {
        [Test]
        public void NoMatchReturnsEmpty()
        {
            var lookup = ImmutableLookupList<int, int>.Empty;
            Assert.That(lookup[42], Is.Not.Null.And.Empty);
        }

        // Helps verify that the tests are being run correctly
        //[Test]
        public void FailingTest()
        {
            Assert.Fail();
        }

        [Test]
        public void MatchReturnsList()
        {
            var lookup = ImmutableLookupList<int, int>.Empty;
            lookup = lookup.Add(42, 42).Add(42, 69);
            Assert.That(lookup[42], Is.Not.Null.And.EquivalentTo(new int[] { 42, 69 }));
        }

        [Test]
        public void RemoveAll1()
        {
            var lookup = ImmutableLookupList<string, int>.Empty;
            lookup = lookup.Add("a", 1).Add("a", 2).Add("a", 3).Add("b", 1);
            Assert.That(lookup.Count, Is.EqualTo(2));
            var lookup2 = lookup.RemoveAll("a");
            Assert.That(lookup2.Count, Is.EqualTo(1));
            Assert.That(lookup2.Select(l => l.Key), Is.EquivalentTo(new[] { "b" }));
        }

        [Test]
        public void RemoveAll2()
        {
            var lookup = ImmutableLookupList<string, int>.Empty;
            lookup = lookup.Add("a", 1).Add("a", 2).Add("a", 2).Add("a", 3).Add("b", 1);
            Assert.That(lookup.Count, Is.EqualTo(2));
            var lookup2 = lookup.RemoveAll("a");
            Assert.That(lookup2.Count, Is.EqualTo(1));
            Assert.That(lookup2.Select(l => l.Key), Is.EquivalentTo(new[] { "b" }));
        }

        [Test]
        public void Remove_NoopReturnsThis()
        {
            var lookup = ImmutableLookupList<string, int>.Empty;
            lookup = lookup.Add("a", 1).Add("a", 2).Add("a", 3).Add("b", 1);
            var lookup2 = lookup.RemoveAll("c");
            Assert.That(lookup2, Is.SameAs(lookup));
            var lookup3 = lookup.RemoveAll("b", 2);
            Assert.That(lookup3, Is.SameAs(lookup));
            var lookup4 = lookup.Remove("b", 2);
            Assert.That(lookup4, Is.SameAs(lookup));
        }
    }
}
