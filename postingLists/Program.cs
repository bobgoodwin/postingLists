using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PostingLists
{

    class Program
    {
        static long tot = 0;
        static long sofar = 0;
        static DateTime start;
        static void Main(string[] args)
        {
            string srcDir = @"D:\local\M0";
            string dstDir = @"C:\local\S0";
            srcDir = @"c:\S0";
            dstDir = @"c:\S0\SS";
            Directory.CreateDirectory(dstDir);
            foreach (var src in Directory.EnumerateFiles(srcDir))
            {
                string fn = Path.GetFileName(src);
                string dst = Path.Combine(dstDir, fn);
                if (File.Exists(dst))
                {
                    if (new FileInfo(src).Length != new FileInfo(dst).Length)
                    {
                        File.Delete(dst);
                    }
                }
                if (!File.Exists(dst))
                {
                    tot += new FileInfo(src).Length;
                }
            }

            start = DateTime.Now;
            int parallel = 48;
            List<Task> tasks = new List<Task>();
            foreach (var src in Directory.EnumerateFiles(srcDir))
            {
                string fn = Path.GetFileName(src);
                string dst = Path.Combine(dstDir, fn);
                if (!File.Exists(dst))
                {
                    while (tasks.Count() >= parallel)
                    {
                        int index = Task.WaitAny(tasks.ToArray());
                        tasks.RemoveAt(index);
                    }

                    string src0 = src;
                    tasks.Add(Task.Run(() => Process(src0, dst)));
                }
            }

            Task.WaitAll(tasks.ToArray());

        }

        static void Process(string src, string dst)
        {
            string fn = Path.GetFileName(src);

            if (fn.StartsWith("_"))
            {
                PostingArray plist = new PostingArray(src);
                plist.Sort();
                plist = plist.RemoveRepeats();
                plist.Write(dst);
            }
            else
            {
                File.Copy(src, dst, true);
            }

            sofar += new FileInfo(src).Length;
            double remain = (DateTime.Now - start).TotalHours * (tot - sofar) / sofar;
            Console.WriteLine($"{sofar / 1e6:0,000} MBs processed {remain:0.00} hours remaining");
        }
    }
}
