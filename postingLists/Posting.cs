using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostingLists
{
    public struct Posting
    {
        ulong item;
        public Document Document
        {
            get
            {
                return new Document(item >> 16);
            }
        }
        public float Bm25F
        {
            get
            {
                return (float)(item & 0xffff) / 100f;
            }
        }
        public ulong Item => item;
        public Posting(ulong item)
        {
            this.item = item;
        }

        public Posting(Document d, float f)
        {
            this.item = (d.hash << 16) + (uint)(f * 100f);
        }
    }

}
