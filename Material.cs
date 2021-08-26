using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icov
{
    class Surface
    {
        private double alphaS=0;
        public double AlphaS
        {
            get { return (alphaS); }
            set { alphaS = value; }
        }

        private double tauS = 0;
        public double TauS
        {
            get { return (tauS); }
            set { tauS = value; }
        }

        private double alphaL=0;
        public double AlphaL
        {
            get { return (alphaL); }
            set { alphaL = value; }
        }

        private double epsilonL=0;
        public double EpsilonL
        {
            get { return (epsilonL); }
            set { epsilonL = value; }
        }


        private String name="";
        public String Name
        {
            get { return (name); }
            set { name = value; }
        }

        public Surface()
        {

        }
    }

    class Material
    {

        public Material()
        {

        }

        public Material(int i)
        {
            id = i;
        }

        private Surface surface = new Surface();
        public Surface Surface
        {
            get { return (surface); }
            set { surface = value; }
        }


        private int id = 0;
        public int Id
        {
            get { return (id); }
            set { id = value; }
        }

        private double q = 0; //Wärmekapazität
        public double Q
        {
            get { return (q); }
            set { q = value; }
        }

        private double d = 0; //Dicke
        public double D
        {
            get { return (d); }
            set { d = value; }
        }

        private double lambda = 0;
        public double Lambda
        {
            get { return (lambda); }
            set { lambda = value; }
        }

        private double rho = 0;
        public double Rho
        {
            get { return (rho); }
            set { rho = value; }
        }

        private double cv = 0;
        public double Cv
        {
            get { return (cv); }
            set { cv = value; }
        }

        private String name = "";
        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public String ToString()
        {
            return (name);
        }
    }
}
