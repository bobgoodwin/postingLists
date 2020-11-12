using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostingLists
{
    public class PostingUnion 
    {
        public List<PostingArray> arrays = new List<PostingArray>();
        bool max=true;

        public PostingUnion(string filename)
        {
            arrays.Add(new PostingArray(filename));
        }

        PostingUnion(List<PostingArray> arrays, bool max)
        {
            this.arrays = arrays;
            this.max = max;
        }

        public PostingUnion(PostingArray a)
        {
            this.arrays.Add(a);
        }

        PostingUnion(bool max)
        {
            this.max = max;
        }

        void Resolve()
        {
            if (arrays.Count() == 1) return;
            arrays.Sort((x, y) => x.Count().CompareTo(y.Count()));
            PostingArray array = arrays[0];
            for (int i=1; i< arrays.Count(); i++)
            {
                array = array.Union(arrays[i], max) as PostingArray;
            }

            arrays.Clear();
            arrays.Add(array);
        }

        public IEnumerable<Posting> Postings()
        {
            Resolve();
            return arrays[0].Postings();
        }

        public long Count()
        {
            Resolve();
            return arrays[0].Count();
        }

        public PostingUnion Intersection(PostingUnion iother, bool max)
        {
            PostingUnion other = iother as PostingUnion;
            List<PostingArray> newArray = new List<PostingArray>();
            for (int i=0; i< this.arrays.Count(); i++)
            {
                for (int j=0; j<other.arrays.Count(); j++)
                {
                    newArray.Add(this.arrays[i].Intersection(other.arrays[j], max) as PostingArray);
                }
            }

            PostingUnion result = new PostingUnion(newArray, this.max);
            if (newArray.Count() > 10) result.Resolve();

            return result;
        }

        public PostingUnion Union(PostingUnion iother, bool max)
        {
            PostingUnion other = iother as PostingUnion;
            PostingUnion result = new PostingUnion(max);
            foreach (var a in this.arrays)
            {
                result.arrays.Add(a);
            }
            foreach (var a in other.arrays)
            {
                result.arrays.Add(a);
            }

            return result;
        }

        public bool TryGetValue(Document d, out float bm25f)
        {
            Resolve();
            return this.arrays[0].TryGetValue(d, out bm25f);
        }
    }
}
