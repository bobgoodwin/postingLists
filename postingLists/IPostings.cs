using PostingLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostingLists
{
    public interface IPostings
    {
        long Count { get; }
        IEnumerable<Posting> Postings();

        IPostings Intersection(IPostings other, bool max);
        IPostings Union(IPostings other, bool max);
    }
}
