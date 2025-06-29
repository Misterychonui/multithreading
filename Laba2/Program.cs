using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Threading;

namespace l2
{
    class Frequency_Dictionary
    {
        public Queue<string> queue;
        public Dictionary<string, int> di;
        public string Filename;
        public Task t1;
        public ReaderWriterLock locker;
        public object locker1;
        public int N;
        public Frequency_Dictionary(string a, int n)
        {
            Filename = a;
            queue = new Queue<string>();
            di = new Dictionary<string, int>();
            locker = new ReaderWriterLock();
            locker1 = new object();
            N = n;
        }

        public void ReadFile()
        {
            string text;
            var sep = new char[] { ' ', ',', '.', '-', '(', ')', '!', '/', '?', ';', ':' };
            using (var sr = new StreamReader(Filename))
            {
                while ((text = sr.ReadLine()) != null)
                {
                    string a = text.ToLower();
                    Console.WriteLine(a);
                    string[] w = a.Split(sep, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < w.Length; i++)
                    {
                        try
                        {
                            locker.AcquireReaderLock(Timeout.Infinite);
                            queue.Enqueue(w[i]);
                        }
                        finally
                        {
                            locker.ReleaseReaderLock();
                        }
                    }
                }
            }
        }
        public void CreateDictionary(Task t1, Queue<string> m)
        {

            string s = null;
            queue = m;
            while (!t1.IsCompleted || queue.Count > 0)
            {
                if (queue.Count > 0)
                {
                    try
                    {
                        locker.AcquireReaderLock(Timeout.Infinite);
                        s = queue.Dequeue();
                    }
                    finally
                    {
                        locker.ReleaseReaderLock();
                    }
                    if (s.Length >= N)
                    {
                        int count = s.Length - N;
                        for (int k = 0; k <= count; k++)
                        {
                            string sub = s.Substring(k, N);
                            if (di.ContainsKey(sub))
                            {
                                di[sub]++;
                               
                            }
                            else
                            {
                                di.Add(sub, 1);
                            }
                        }
                    }
                }
            }
            foreach (var da in di)
            {
                Console.WriteLine($"N-грамма {da.Key} : частота {da.Value}");
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            string _file_name_and_N;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(string));
            using (var stream = new StreamReader("a1.xml"))
            {
                _file_name_and_N = (string)xmlSerializer.Deserialize(stream);
            }
            Frequency_Dictionary c = new Frequency_Dictionary(_file_name_and_N.Split(' ')[0], int.Parse(_file_name_and_N.Split(' ')[1]));
            Task task1 = Task.Factory.StartNew(c.ReadFile);
            Task task2 = new Task(() => c.CreateDictionary(task1, c.queue));
            task2.Start();
            Console.ReadKey();
        }
    }
}