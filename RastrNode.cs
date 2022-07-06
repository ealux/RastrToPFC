using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RastrToPFC
{
    // Node
    internal class RastrNode
    {
        public int      Num;    // Номер узла КЗ
        public string   Name;   // Наименование узла
        public double   Unom;   // Номинальное напряжение узла
        public double   delta;  // Угол номинального напряжения узла
        public NodeType Type;   // Тип узла
        public bool     State;  // Состояние                 

        public double Pn;   // Нагрузка P
        public double Qn;   // Нагрузка Q

        public double Pg;   // Генерация P
        public double Qg;   // Генерация Q

        public double Bsh;  // Проводимость шунта B
        public double Gsh;  // Проводимость шунта G

        public RastrNode(int num, string name, double unom, NodeType type, bool state, 
                        double pn, double qn, 
                        double pg, double qg, 
                        double bsh, double gsh, double delta)
        {
            Num = num;
            Name = name;
            Unom = unom;
            Type = type;
            State = state;
            Pn = pn;
            Qn = qn;
            Pg = pg;
            Qg = qg;
            Bsh = bsh;
            Gsh = gsh;
            this.delta = delta;
        }

        public override string ToString()
        {
            string shunt = "";
            if (Bsh != 0 | Gsh != 0)
            {
                if (Bsh != 0 & Gsh == 0) shunt = $", Ysh = new Complex(0, {Math.Round(Bsh, 3).ToString().Replace(",", ".")}e-6)";
                else if (Bsh == 0 & Gsh != 0) shunt = $", Ysh = new Complex({Math.Round(Gsh, 3).ToString().Replace(",", ".")}e-6, 0)";
                else shunt = $", Ysh = new Complex({Math.Round(Gsh, 3).ToString().Replace(",", ".")}e-6, {Math.Round(Bsh, 3).ToString().Replace(",", ".")}e-6)";
            }
            string sgen = "";
            if (Pg != 0 | Qg != 0)
                sgen = $", S_gen = new Complex(" + $"{Math.Round(Pg, 3).ToString().Replace(",", ".")}" +
                                                ", " + $"{Math.Round(Qg, 3).ToString().Replace(",", ".")}" + ")";
            string sload = "";
            if (Pn != 0 | Qn != 0)
                sload = $", S_load = new Complex(" + $"{Math.Round(Pn, 3).ToString().Replace(",", ".")}" +
                                                ", " + $"{Math.Round(Qn, 3).ToString().Replace(",", ".")}" + ")";

            return "new Node(){Num = " + $"{Num}" + ",  Type = NodeType.PQ,    " +
                                "Unom=" + $"{Math.Round(Unom, 3).ToString().Replace(",", ".")}" +
                                $"{sload}" +
                                $"{sgen}" + 
                                $"{shunt}" +
                                "},";
        }
    }

    //Generation
    internal class GenNode : RastrNode
    {
        public double Qmin;
        public double Qmax;
        public double Vpre;

        public GenNode(int num, string name, double unom, NodeType type, bool state,
                      double pn, double qn,
                      double pg, double qg,
                      double bsh, double gsh, double delta,
                      double qmin, double qmax, double vpre)
            : base(num, name, unom, type, state, pn, qn, pg, qg, bsh, gsh, delta)
        {
            Qmin = qmin;
            Qmax = qmax;
            Vpre = vpre;
        }

        public override string ToString()
        {
            string shunt = "";
            if (Bsh != 0 | Gsh != 0)
            {
                if (Bsh != 0 & Gsh == 0) shunt = $", Ysh = new Complex(0, {Math.Round(Bsh, 3).ToString().Replace(",", ".")}e-6)";
                else if (Bsh == 0 & Gsh != 0) shunt = $", Ysh = new Complex({Math.Round(Gsh, 3).ToString().Replace(",", ".")}e-6, 0)";
                else shunt = $", Ysh = new Complex({Math.Round(Gsh, 3).ToString().Replace(",", ".")}e-6, {Math.Round(Bsh, 3).ToString().Replace(",", ".")}e-6)";
            }
            string sgen = "";
            if (Pg != 0 | Qg != 0)
                sgen = $", S_gen = new Complex(" + $"{Math.Round(Pg, 3).ToString().Replace(",", ".")}" +
                                                ", " + $"{Math.Round(Qg, 3).ToString().Replace(",", ".")}" + ")";
            string sload = "";
            if (Pn != 0 | Qn != 0)
                sload = $", S_load = new Complex(" + $"{Math.Round(Pn, 3).ToString().Replace(",", ".")}" +
                                                ", " + $"{Math.Round(Qn, 3).ToString().Replace(",", ".")}" + ")";

            return "new Node(){Num = " + $"{Num}" + ",  Type = NodeType.PV,    " +
                                "Unom=" + $"{Math.Round(Unom, 3).ToString().Replace(",", ".")}" + ", " +
                                "Vpre = " + $"{Math.Round(Vpre, 3).ToString().Replace(",", ".")}" +
                                $"{sload}" +
                                $"{sgen}" +
                                ", Q_min = " + $"{Math.Round(Qmin, 3).ToString().Replace(",", ".")}" + 
                                ", Q_max = " + $"{Math.Round(Qmax, 3).ToString().Replace(",", ".")}" + 
                                $"{shunt}" +
                                "},";
        }
    }

    // Slack
    internal class SlackNode : RastrNode
    {
        public double Vpre;

        public SlackNode(int num, string name, double unom, NodeType type, bool state, 
                        double pn, double qn,
                        double pg, double qg,
                        double bsh, double gsh, double delta, double vpre)
            : base(num, name, unom, type, state, pn, qn, pg, qg, bsh, gsh, delta)
        {
            Vpre = vpre;
        }

        public override string ToString()
        {    
            string shunt = "";
            if (Bsh != 0 | Gsh != 0)
            {
                if (Bsh != 0 & Gsh == 0) shunt = $", Ysh = new Complex(0, {Math.Round(Bsh, 3).ToString().Replace(",", ".")}e-6)";
                else if (Bsh == 0 & Gsh != 0) shunt = $", Ysh = new Complex({Math.Round(Gsh, 3).ToString().Replace(",", ".")}e-6, 0)";
                else shunt = $", Ysh = new Complex({Math.Round(Gsh, 3).ToString().Replace(",", ".")}e-6, {Math.Round(Bsh, 3).ToString().Replace(",",".")}e-6)";
            }
            string sgen = "";
            if (Pg != 0 | Qg != 0)
                sgen = $", S_gen = new Complex(" + $"{Math.Round(Pg, 3).ToString().Replace(",", ".")}" + 
                                                ", " + $"{Math.Round(Qg, 3).ToString().Replace(",", ".")}" + ")";
            string sload = "";
            if (Pn != 0 | Qn != 0)
                sload = $", S_load = new Complex(" + $"{Math.Round(Pn, 3).ToString().Replace(",", ".")}" + 
                                                ", " + $"{Math.Round(Qn, 3).ToString().Replace(",", ".")}" + ")";

            return "new Node(){ Num = " + $"{Num}" + "," +
                              "Type = NodeType.Slack, " +
                              "Unom=Complex.FromPolarCoordinates(" + $"{Math.Round(Vpre, 3).ToString().Replace(",", ".")}" + 
                                                                ", " + $"{Math.Round(delta, 4).ToString().Replace(",", ".")}" + ")"+
                              $"{sload}" +
                              $"{sgen}" +
                              $"{shunt}" + 
                              "},";
        }
    }


    enum NodeType
    {
        Load = 0,
        Gen = 1,
        Slack = 2
    }
}
