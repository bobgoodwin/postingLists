using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostingLists
{
    public class PostingList
    {
        public string Term;
        PostingUnion[] shards;
        public PostingList(string term, List<string> directories, string filename)
        {
            this.Term = term;
            shards = new PostingUnion[directories.Count()];
            for (int shard = 0; shard < directories.Count; shard++)
            {
                shards[shard] = new PostingUnion(Path.Combine(directories[shard], filename));
            }
        }

        PostingList(string term, int n)
        {
            this.Term = term;
            shards = new PostingUnion[n];
        }

        public long Count
        {
            get
            {
                long cnt = 0;
                foreach (var s in shards) cnt += s.Count();
                return cnt;
            }
        }

        public PostingList Intersection(PostingList other, bool max)
        {
            if (this.shards.Length != other.shards.Length) throw new Exception();
            PostingList result = new PostingList("", this.shards.Length);
            for (int s=0; s<this.shards.Length; s++)
            {
                result.shards[s] = this.shards[s].Intersection(other.shards[s], max);
            }

            return result;
        }

        public IEnumerable<Posting> Postings()
        {
            for (int s = 0; s < this.shards.Length; s++)
            {
                foreach (var n in this.shards[s].Postings())
                {
                    yield return n;
                }
            }
        }

        public PostingList Union(PostingList other, bool max)
        {
            if (this.shards.Length != other.shards.Length) throw new Exception();
            PostingList result = new PostingList("", this.shards.Length);
            for (int s = 0; s < this.shards.Length; s++)
            {
                result.shards[s] = this.shards[s].Union(other.shards[s], max);
            }

            return result;
        }

        public bool TryGetValue(Document d, out float bm25f)
        {
            foreach (var a in this.shards)
            {
                if (a.TryGetValue(d, out bm25f))
                {
                    return true;
                }
            }

            bm25f = 0;
            return false;
        }

    }
}
