using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PostingLists;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Intersection1()
        {
            var p0 = PA(0, 1000000000, 1000, .101f);
            Assert.AreEqual(1000001, p0.Count());
            var p1 = PA(0, 1000000000,3000000, .021f);
            Assert.AreEqual(334, p1.Count());
            var p = p0.Intersection(p1, max: false);
            Assert.AreEqual(334, p.Count());
            foreach (var n in p.Postings())
            {
                var h = n.Document.hash;
                Assert.IsTrue(h >= 0 && h < 1000000000 && h % 3000000 == 0);
                Assert.IsTrue(n.Bm25F == .12f);
            }

            p = p1.Intersection(p0, max: true);
            Assert.AreEqual(334, p.Count());
            foreach (var n in p.Postings())
            {
                var h = n.Document.hash;
                Assert.IsTrue(h >= 0 && h < 1000000000 && h % 3000000 == 0);
                Assert.IsTrue(n.Bm25F == .10f);
            }
        }

        [TestMethod]
        public void Intersection2()
        {
            var p0 = PA(7, 22, 3, .101f);
            Assert.AreEqual(6, p0.Count());
            var p1 = PA(4, 16, 2, .021f);
            Assert.AreEqual(7, p1.Count());
            var p = p0.Intersection(p1, max: false);
            Assert.AreEqual(2, p.Count());
            foreach (var n in p.Postings())
            {
                Assert.IsTrue(n.Bm25F == .12f);
            }

            p = p1.Intersection(p0, max: true);
            Assert.AreEqual(2, p.Count());
            foreach (var n in p.Postings())
            {
                Assert.IsTrue(n.Bm25F == .1f);
            }
        }
        [TestMethod]
        public void Union2()
        {
            var p0 = PA(7, 22, 3, .101f);
            Assert.AreEqual(6, p0.Count());
            var p1 = PA(4, 16, 2, .021f);
            Assert.AreEqual(7, p1.Count());
            var p = p0.Union(p1, max: false);
            Assert.AreEqual(9, p.Count());
            foreach (var n in p.Postings())
            {
                float f1 = (n.Document.hash>=7 && n.Document.hash<=22 && (n.Document.hash - 7) % 3 == 0) ? .1f : 0;
                float f2 = (n.Document.hash >= 4 && n.Document.hash <= 16 && (n.Document.hash - 4) % 2 == 0) ? .02f : 0;
                Assert.IsTrue(Math.Abs(n.Bm25F - f1-f2)<.001);
            }
            float bm25f;
            Assert.IsTrue(p.TryGetValue(new Document(10), out bm25f));
            Assert.AreEqual(bm25f, .12f);
            Assert.IsFalse(p.TryGetValue(new Document(11), out bm25f));

            p = p1.Union(p0, max: true);
            Assert.AreEqual(9, p.Count());
            foreach (var n in p.Postings())
            {
                float f1 = (n.Document.hash >= 7 && n.Document.hash <= 22 && (n.Document.hash - 7) % 3 == 0) ? .1f : 0;
                float f2 = (n.Document.hash >= 4 && n.Document.hash <= 16 && (n.Document.hash - 4) % 2 == 0) ? .02f : 0;
                Assert.IsTrue( Math.Abs(n.Bm25F - Math.Max(f1, f2))<.0001);
            }

        }

        [TestMethod]
        public void PostingUnion()
        {
            var p0A = PA(7, 22, 3, .101f);
            var pu0A = new PostingUnion(p0A);
            var p0B = PA(107, 122, 3, .101f);
            var pu0B = new PostingUnion(p0B);
            var p1A = PA(4, 16, 2, .011f);
            var pu1A = new PostingUnion(p1A);
            var p1B = PA(104, 116, 2, .011f);
            var pu1B = new PostingUnion(p1B);

            var p0 = p0A.Union(p0B, max: true);
            var p1 = p1A.Union(p1B, max: true);
            var p = p0.Intersection(p1, max: false);

            var pu0 = pu0A.Union(pu0B, max: true);
            var pu1 = pu1A.Union(pu1B, max: true);
            var pu = pu0.Intersection(pu1, max: false);

            List<Posting> a = new List<Posting>();
            foreach (var n in p.Postings())
            {
                a.Add(n);
            }

            List<Posting> b = new List<Posting>();
            foreach (var n in pu.Postings())
            {
                b.Add(n);
            }

            Assert.AreEqual(a.Count, b.Count);
            for (int i=0; i<a.Count; i++)
            {
                Assert.AreEqual(a[i].Document, b[i].Document);
                Assert.AreEqual(a[i].Bm25F, b[i].Bm25F);
            }
        }

        public static PostingArray PA(ulong s0, ulong s1, ulong s2, float f0)
        {
            List<Posting> p = new List<Posting>();
            for (ulong s = s0; s <= s1; s += s2)
            {
                p.Add(new Posting(new Document(s), f0));
            }

            var pa = new PostingArray(p.ToArray());
            pa.Sort();
            return pa;
        }
    }
}
