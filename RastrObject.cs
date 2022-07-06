using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASTRALib;

namespace RastrToPFC
{
    internal class RastrObject
    {
        public string fileSavePath;
        private string file;

        private const string nodesName = "node";
        private const string branchesName = "vetv";

        private int nodesCount;
        private int branchesCount;

        private readonly RastrClass   rastr;
        private readonly IRastr _rastr;

        public List<RastrNode> Nodes;
        public List<RastrBranch> Branches;

        // Manipulate data from rastr storage
        private ITable GetTable(string tablename) => (ITable)_rastr.Tables.Item(tablename);
        private ICol GetInternalCol(string name, string tablename) => (ICol)GetTable(tablename).Cols.Item(name);
        private object GetInternalValue(string colName, int rowId, string tablename) => GetInternalCol(colName, tablename).ZN[rowId];


        // ctor
        public RastrObject(string filename)
        {
            this._rastr = new RastrClass();
            _rastr.Load(RG_KOD.RG_REPL, filename, String.Empty);

            fileSavePath = filename.Replace(filename.Split("\\")[filename.Split("\\").Length - 1], "");
            file = filename.Split("\\")[filename.Split("\\").Length - 1];

            Console.WriteLine("===================================");
            Console.WriteLine($"\nЗагружен файл: {filename.Split("\\")[filename.Split("\\").Length -1]}");

            nodesCount = GetTable(nodesName).Count;
            branchesCount = GetTable(branchesName).Count;

            Nodes = GenerateNodesFromStorage();
            Console.WriteLine($"Список узлов сформирован. Узлов: {Nodes.Count} (База: {Nodes.Where(n=>n.Type==NodeType.Slack).Count()}" +
                                                                                $"\tНагрузка: {Nodes.Where(n => n.Type == NodeType.Load).Count()}" +
                                                                                $"\tГенерация: {Nodes.Where(n => n.Type == NodeType.Gen).Count()})");
            Branches = GenerateBranchesFromStorage();
            Console.WriteLine($"Список ветвей сформирован. Ветвей: {Branches.Count}");
        }

        public void PrintPFCFile()
        {

            #region Prepare Text


            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Logger.LogInfo(\" =========================================== \");");
            sb.AppendLine($"Logger.LogInfo(\"{Nodes.Count} nodes: {Nodes.Where(n => n.Type == NodeType.Gen).Count()} - PV" +
                                                              $"  {Nodes.Where(n => n.Type == NodeType.Load).Count()} - PQ" +
                                                              $"  {Nodes.Where(n => n.Type == NodeType.Slack).Count()} - Slack\");");
            sb.AppendLine("Logger.LogInfo(\" =========================================== \");");

            // Nodes
            sb.AppendLine("var nodes = new List<Node>()\n{\n");
            foreach (var item in Nodes)
                sb.AppendLine("\t" + item.ToString());  // node
            sb.AppendLine("};\n\n");

            // Branches
            sb.AppendLine("var branches = new List<Branch>()\n{\n");
            foreach (var item in Branches)
                sb.AppendLine("\t" + item.ToString());  // branch
            sb.AppendLine("};\n\n");

            // Engine
            sb.AppendLine("var options = new CalculationOptions();\n");
            sb.AppendLine("var engine = new Engine(nodes, branches, options);\n\n");
            sb.AppendLine("return engine;");

            #endregion

            var doc = $"{fileSavePath}{DateTime.UtcNow.ToLocalTime():dd.MM.yy}__{file}.txt";
            var data = sb.ToString();

            using (StreamWriter sr = new StreamWriter(doc))
            {
                sr.WriteLine(data);
            }          
        }

        #region Generate Data From Rastr
        private List<RastrNode> GenerateNodesFromStorage()
        {
            ConcurrentBag<RastrNode> nodes = new ConcurrentBag<RastrNode>();

            Parallel.For(0, nodesCount, i =>
            {
                int Num = (int)GetInternalValue("ny", i, nodesName);
                string Name = (string)GetInternalValue("name", i, nodesName);
                double Unom = (double)GetInternalValue("uhom", i, nodesName);
                double Delta = (double)GetInternalValue("delta", i, nodesName);
                bool State = !(bool)GetInternalValue("sta", i, nodesName);

                double Pn = (double)GetInternalValue("pn", i, nodesName);
                double Qn = (double)GetInternalValue("qn", i, nodesName);

                double Pg = (double)GetInternalValue("pg", i, nodesName);
                double Qg = (double)GetInternalValue("qg", i, nodesName);

                double Bsh = (double)GetInternalValue("bsh", i, nodesName);
                double Gsh = (double)GetInternalValue("gsh", i, nodesName);

                RastrNode node;
                NodeType type;

                switch (GetInternalValue("tip", i, nodesName))
                {
                    case 0:
                        type = NodeType.Slack;
                        double vpre_slack = (double)GetInternalValue("vras", i, nodesName);
                        node = new SlackNode(Num, Name, Unom, type, State, Pn, Qn, Pg, Qg, Bsh, Gsh, Delta, vpre_slack);
                        nodes.Add(node);
                        break;
                    case 1:
                        type = NodeType.Load;
                        node = new RastrNode(Num, Name, Unom, type, State, Pn, Qn, Pg, Qg, Bsh, Gsh, Delta);
                        nodes.Add(node);
                        break;
                    case 2:
                    case 3:
                    case 4:
                        type = NodeType.Gen;
                        double Qmin = (double)GetInternalValue("qmin", i, nodesName);
                        double Qmax = (double)GetInternalValue("qmax", i, nodesName);
                        double Vpre = (double)GetInternalValue("vzd", i, nodesName);
                        node = new GenNode(Num, Name, Unom, type, State, Pn, Qn, Pg, Qg, Bsh, Gsh, Delta,
                                           Qmin, Qmax, Vpre);
                        nodes.Add(node);
                        break;
                }
            });

            var result = nodes.OrderBy(n => n.Num).ToList();

            return result;
        }

        private List<RastrBranch> GenerateBranchesFromStorage()
        {
            ConcurrentBag<RastrBranch> branches = new ConcurrentBag<RastrBranch>();

            Parallel.For(0, branchesCount, i =>
            {
                int Start = (int)GetInternalValue("ip", i, branchesName);
                int End = (int)GetInternalValue("iq", i, branchesName);
                //var sta = GetInternalValue("sta", i, branchesName);
                bool State = (int)GetInternalValue("sta", i, branchesName) == 0 ? true : false;

                double R = (double)GetInternalValue("r", i, branchesName);
                double X = (double)GetInternalValue("x", i, branchesName);
                double G = (double)GetInternalValue("g", i, branchesName);
                double B = (double)GetInternalValue("b", i, branchesName);

                RastrBranch branch;
                BranchType type;

                switch (GetInternalValue("tip", i, branchesName))
                {
                    case 0:
                        type = BranchType.Line;
                        branch = new LineBranch(Start, End, type, State, R, X, B, G);
                        branches.Add(branch);
                        break;
                    case 1:
                        type = BranchType.Trans;
                        var ktr = (double)GetInternalValue("ktr", i, branchesName);
                        branch = new TransBranch(Start, End, type, State, R, X, B, G, ktr);
                        branches.Add(branch);
                        break;
                    case 2:
                        type = BranchType.Breaker;
                        branch = new BreakerBranch(Start, End, type, State, R, X, B, G);
                        branches.Add(branch);
                        break;
                }
            });

            var result = branches.OrderBy(b => b.Start).ToList();

            return result;
        }

        #endregion

    }
}
