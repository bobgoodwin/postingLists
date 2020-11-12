using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel.Design;

namespace PostingLists
{
    public class PostingArray 
    {
        Posting[] list;
        public PostingArray(byte[] bytes)
        {
            list = new Posting[bytes.Length / 8];
            for (int i = 0; i < bytes.Length / 8; i++)
            {
                list[i] = new Posting(BitConverter.ToUInt64(bytes, i * 8));
            }
        }

        public PostingArray(Posting[] list)
        {
            this.list = list;
        }

        public long Count()
        {
            return this.list.Length;
        }

        public PostingArray(string path)
        {
            if (!File.Exists(path))
            {
                list = new Posting[0];
                return;
            }

            using (System.IO.BinaryReader br = new System.IO.BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                int byteCount = br.ReadInt32();
                this.list = new Posting[byteCount / sizeof(ulong)];
                for (int i=0; i<list.Length;i++)
                {
                    var hash=br.ReadUInt64();
                    list[i] = new Posting(hash);
                }
            }
        }

        public void Write(string path)
        {
            Document last = default;
            using (System.IO.BinaryWriter bw = new BinaryWriter(File.Create(path)))
            {
                bw.Write(this.List.Length*8);
                foreach (var n in list)
                {
                    if (last.hash >= n.Document.hash) throw new Exception();
                    last = n.Document;
                    bw.Write(n.Item);
                }
            }
        }

        public Posting[] List => list;

        public IEnumerable<Posting> Postings()
        {
            foreach (var n in list)
            {
                yield return n;
            }
        }
  
        public void Sort()
        {
            Array.Sort(this.list, (x, y) => x.Document.hash.CompareTo(y.Document.hash));
        }

        public PostingArray Intersection(PostingArray other, bool max)
        {
            int min = Math.Min(this.list.Length, other.list.Length);
            if (min == 0) return new PostingArray(new Posting[0]);

            int span0 = this.list.Length / other.list.Length;
            int span1 = other.list.Length / this.list.Length;
            List<Posting> result = new List<Posting>(min / 10 + 1);
            int i0 = 0;
            int i1 = 0;
            Posting item0 = this.list[i0];
            Posting item1 = other.list[i1];
            while (i0 < this.list.Length && i1 < other.list.Length)
            {
                if (item0.Document == item1.Document)
                {
                    if (max)
                    {
                        result.Add(new Posting(item0.Document, Math.Max(item0.Bm25F, item1.Bm25F)));
                    }
                    else
                    {
                        result.Add(new Posting(item0.Document, item0.Bm25F + item1.Bm25F));
                    }
                    i0 = this.Next(i0, out item0);
                    i1 = other.Next(i1, out item1);

                }
                else if (item0.Document.hash < item1.Document.hash)
                {
                    i0 = this.SkipAhead(i0, span0, item1.Document, out item0);
                }
                else
                {
                    i1 = other.SkipAhead(i1, span1, item0.Document, out item1);
                }
            }

            return new PostingArray(result.ToArray());
        }

        public PostingArray RemoveRepeats()
        {
            List<Posting> p = new List<Posting>();
            Posting last = default;
            foreach (var n in this.list)
            {
                if (n.Document!=last.Document)
                {
                    p.Add(n);
                }
                last = n;
            }

            return new PostingArray(p.ToArray());
        }

        public int Next(int start, out Posting item)
        {
            start = start + 1;
            if (start>=this.list.Length)
            {
                item = default;
                return this.list.Length;
            }    
            else
            {
                item = this.list[start];
                return start;
            }
        }

        public int SkipAhead(int start, int span, Document next, out Posting item)
        {
            item = default;
            if (start >= list.Length) return start;
            item = default;
            int end = 0;
            for (end=start+span; ; end+=span)
            {
                if (end>=this.list.Length)
                {
                    end = this.list.Length;
                    if (this.list[end-1].Document.hash < next.hash) return list.Length;
                    break;
                }
                if (this.list[end].Document.hash >= next.hash) break;
                span++;
            }

            // start binary search between start(inclusive) and end (exclusive)
            while ((end-start)>1)
            {
                int mid = ((end - start) / 2) + start;
                if (this.list[mid].Document.hash < next.hash)
                {
                    start = mid;
                }
                else
                {
                    end = mid;
                }
            }

            item = list[end];
            return end;
        }

        public PostingArray Union(PostingArray other, bool max)
        {
            int min = Math.Max(this.list.Length, other.list.Length);
            if (this.list.Length == 0) return other;
            if (other.list.Length == 0) return this;
            List<Posting> result = new List<Posting>(min);
            int i0 = 0;
            int i1 = 0;
            Posting item0 = this.list[i0];
            Posting item1 = other.list[i1];
            while (i0 < this.list.Length && i1 < other.list.Length)
            {
                if (i0 == this.list.Length)
                {
                    result.Add(item1);
                    i1 = other.Next(i1, out item1);
                }
                else if (i1 == other.list.Length)
                {
                    result.Add(item0);
                    i0 = this.Next(i0, out item0);
                }
                else if (item0.Document == item1.Document)
                {
                    if (max)
                    {
                        result.Add(new Posting(item0.Document, Math.Max(item0.Bm25F, item1.Bm25F)));
                    }
                    else
                    {
                        result.Add(new Posting(item0.Document, item0.Bm25F + item1.Bm25F));
                    }

                    i0 = this.Next(i0, out item0);
                    i1 = other.Next(i1, out item1);

                }
                else if (item0.Document.hash < item1.Document.hash)
                {
                    result.Add(item0);
                    i0 = this.Next(i0, out item0);
                }
                else
                {
                    result.Add(item1);
                    i1 = other.Next(i1, out item1);
                }
            }

            return new PostingArray(result.ToArray());

        }

        public bool TryGetValue(Document d, out float bm25f)
        {
            int start = 0;
            int end = this.list.Length;
            while ((end - start) > 1)
            {
                int mid = (end - start) / 2 + start;
                if (this.list[mid].Document.hash<d.hash)
                {
                    start = mid;
                }
                else
                {
                    end = mid;
                }
            }

            bm25f = 0;
            if (end == this.list.Length) return false;
            if (this.list[end].Document == d)
            {
                bm25f = this.list[end].Bm25F;
                return true;
            }

            return false;
        }

    }
}
