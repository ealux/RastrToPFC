using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RastrToPFC
{
    // Branch
    internal class RastrBranch
    {
        public int    Start, End;        
        public BranchType Type;                    
        public bool   State;                    

        public double R,X,B,G;

        public RastrBranch(int start, int end,
                        BranchType type, bool state,
                        double r, double x, double b, double g)
        {
            Start = start;
            End = end;
            Type = type;
            State = state;
            R = r;
            X = x;
            B = b;
            G = g;
        }
    }

    // Line
    internal class LineBranch : RastrBranch
    {
        public LineBranch(int start, int end,
                        BranchType type, bool state,
                        double r, double x, double b, double g)
            : base(start, end, type, state, r, x, b, g) { }

        public override string ToString()
        {
            string shunt = "";
            if (B != 0 | G != 0)
            {
                if (B != 0 & G == 0) shunt = $", Ysh = new Complex(0, {Math.Round(-B, 3).ToString().Replace(",", ".")}e-6)";
                else if (B == 0 & G != 0) shunt = $", Ysh = new Complex({Math.Round(G, 3).ToString().Replace(",", ".")}e-6, 0)";
                else shunt = $", Ysh = new Complex({Math.Round(G, 3).ToString().Replace(",", ".")}e-6, {Math.Round(-B, 3).ToString().Replace(",", ".")}e-6)";
            }

            return "new Branch(){Start = " + $"{Start}" + ", End = " + $"{End}, " +
                                $"Ktr=1" +
                                $", Y=1/new Complex(" + $"{Math.Round(R, 4).ToString().Replace(",", ".")}" +
                                                    ", " + $"{Math.Round(X, 4).ToString().Replace(",", ".")}" + ")" +
                                $"{shunt}" +
                                "},";
        }
    }

    // Breaker
    internal class BreakerBranch : RastrBranch
    {
        public BreakerBranch(int start, int end,
                        BranchType type, bool state,
                        double r, double x, double b, double g)
            : base(start, end, type, state, r, x, b, g) { }

        public override string ToString()
        {
            string shunt = "";
            if (B != 0 | G != 0)
            {
                if (B != 0 & G == 0) shunt = $", Ysh = new Complex(0, {Math.Round(B, 3).ToString().Replace(",", ".")}e-6)";
                else if (B == 0 & G != 0) shunt = $", Ysh = new Complex({Math.Round(G, 3).ToString().Replace(",", ".")}e-6, 0)";
                else shunt = $", Ysh = new Complex({Math.Round(G, 3).ToString().Replace(",", ".")}e-6, {Math.Round(B, 3).ToString().Replace(",", ".")}e-6)";
            }
            string y = "";
            if (R == 0 | X == 0)
                y = $", Y=1/(new Complex(0, 0.001))";
            else
                y = $", Y=1/(new Complex({Math.Round(R, 4).ToString().Replace(",", ".")}, {Math.Round(X, 4).ToString().Replace(",", ".")}))";


            return "new Branch(){Start = " + $"{Start}" + ", End = " + $"{End}, " +
                                $"Ktr=1" +
                                $"{y}" +                                
                                $"{shunt}" +
                                "},";
        }
    }

    // Trans
    internal class TransBranch : RastrBranch
    {
        public double Ktr;

        public TransBranch(int start, int end,
                        BranchType type, bool state, 
                        double r, double x, double b, double g,
                        double ktr) 
            : base(start, end, type, state, r, x, b, g)
        {
            this.Ktr = ktr;
        }

        public override string ToString()
        {
            string shunt = "";
            if (B != 0 | G != 0)
            {
                if (B != 0 & G == 0) shunt = $", Ysh = new Complex(0, {Math.Round(-B, 3).ToString().Replace(",", ".")}e-6)";
                else if (B == 0 & G != 0) shunt = $", Ysh = new Complex({Math.Round(G, 3).ToString().Replace(",", ".")}e-6, 0)";
                else shunt = $", Ysh = new Complex({Math.Round(G, 3).ToString().Replace(",", ".")}e-6, {Math.Round(-B, 3).ToString().Replace(",", ".")}e-6)";
            }

            return "new Branch(){Start = " + $"{Start}" + ", End = " + $"{End}, " +
                                $"Ktr={Math.Round(Ktr, 6).ToString().Replace(",", ".")}" +
                                $", Y=1/new Complex(" + $"{Math.Round(R, 4).ToString().Replace(",", ".")}" +
                                                    ", " + $"{Math.Round(X, 4).ToString().Replace(",", ".")}" + ")" +
                                $"{shunt}" + 
                                "},";
        }
    }


    internal  enum BranchType
    {
        Line = 0,
        Breaker = 1,
        Trans = 2
    }
}
