using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;

namespace CopyTreeToFile
{
    class Program
    {
        private static List<SourceTree> ChildTrees { get; set; }
        private static List<SourceTree> ParentTrees { get; set; }

        private static string resultFile = "ResultFile.txt";

        static void Main()
        {
            string pathFile = "SourceTree.csv";
            GenerateFileTree(pathFile, 10);

            var stList = ReadSourceTrees(pathFile);

            var groupSourceTrees = stList.OrderBy(x => x.TextElement).GroupBy(x => x.Pid).OrderBy(x => x.Key);
            ChildTrees = new List<SourceTree>();
            ParentTrees = new List<SourceTree>();
            foreach (var sourceTrees in groupSourceTrees)
            {
                foreach (var sourceTree in sourceTrees)
                {
                    if (!ChildTrees.Contains(sourceTree))
                    {
                        ParentTrees.Add(sourceTree);
                        string resText = string.Format("{0} {1} {2}", sourceTree.Id, sourceTree.Pid, sourceTree.TextElement);
                        Console.WriteLine(resText);
                        WriteToFile(resultFile, resText);
                        GetChilds(sourceTree.Id, stList, 1);
                    }
                }
            }

            Console.ReadKey();
        }

        public static void GenerateFileTree(string pathFile, int countElement)
        {
            if (string.IsNullOrEmpty(pathFile))
                throw new ArgumentNullException(pathFile);

            if (File.Exists(pathFile))
                return;

            List<SourceTree> stList = new List<SourceTree>();
            Random pidRandom = new Random();
            Random numberTextElementRandom = new Random();
            for (int i = 1; i <= countElement; i++)
            {
                var st = new SourceTree();
                st.Id = i;
                do
                {
                    st.Pid = pidRandom.Next(10);
                }
                while (st.Pid == st.Id);
                    
                st.TextElement = "A" + numberTextElementRandom.Next(20);
                stList.Add(st);
            }

            using (StreamWriter sw = new StreamWriter(pathFile))
            {
                var csvWriter = new CsvWriter(sw);
                csvWriter.WriteRecords(stList);
            }
        }

        public static List<SourceTree> ReadSourceTrees(string pathFile)
        {
            if (string.IsNullOrEmpty(pathFile))
                throw new ArgumentNullException(pathFile);
            if (!File.Exists(pathFile))
                throw new FileNotFoundException("Файл не найден!");

            List<SourceTree> stList;
            using (StreamReader sr = new StreamReader(pathFile))
            {
                var csvReader = new CsvReader(sr);
                stList = csvReader.GetRecords<SourceTree>().ToList();
            }

            return stList;
        }

        public static void GetChilds(int chId, List<SourceTree> sourceTrees, int levelTree)
        {
            levelTree++;
            var listTree = (from x in sourceTrees
                where x.Pid == chId
                orderby x.TextElement
                select x).ToList();

            if (!listTree.Any())
                return;
            ChildTrees.AddRange(listTree);
            foreach (var leaf in listTree)
            {
                if (!ParentTrees.Contains(leaf))
                {
                    string resText = string.Format(new string(' ', levelTree) + "{0} {1} {2}", leaf.Id, leaf.Pid, leaf.TextElement);
                    Console.WriteLine(resText);
                    WriteToFile(resultFile, resText);
                    GetChilds(leaf.Id, sourceTrees, levelTree);
                }
            }
        }

        public static void WriteToFile(string pathFile, string textTree)
        {
            try
            {
                bool fExists = File.Exists(pathFile);
                using (StreamWriter sw = new StreamWriter(pathFile, fExists, Encoding.Default))
                {
                    sw.WriteLine(textTree);
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException("Файл не удалось сохранить!", e);
            }
        }
    }
}
