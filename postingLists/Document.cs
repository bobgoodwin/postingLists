using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostingLists
{
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    public struct Document
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
    {
        public readonly ulong hash;

        public Document(ulong hash)
        {
            this.hash = hash;
        }
        public static bool operator ==(Document a, Document b)
        {
            return a.hash == b.hash;
        }
        public static bool operator !=(Document a, Document b)
        {
            return a.hash != b.hash;
        }

        public override string ToString()
        {
            return ""+hash;
        }
    }
}

