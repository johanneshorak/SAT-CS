using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace Icov
{
    class Compartment
    {
        private double l, w, h;
        public double L
        {
            get { return l;  }
            set { l = value; }
        }

        public double W
        {
            get { return w; }
            set { w = value; }
        }

        public double H
        {
            get { return h; }
            set { h = value; }
        }

        private double volume = 0;
        public double Volume
        {
            get { return volume; }
            set { volume = value; }
        }

        private double mass = 0;
        public double Mass
        {
            get { return mass; }
            set { mass = value; }
        }

        private DataPoints q;
        public DataPoints Q
        {
            get { return q; }
            set { q = value; }
        }

        private DataPoints t;
        public DataPoints T
        {
            get { return t; }
            set { t = value; }
        }

        private bool interiorFittings;
        public bool InteriorFittings {
            get { return interiorFittings; }
            set { interiorFittings = InteriorFittings; }
        }

        public double ConversionFactorJ2K
        {
            get { return (1 / (Mass * Physics.C_CP_AIR )); }
        }

        public double ConversionFactorK2J
        {
            get { return ((Mass * Physics.C_CP_AIR)); }
        }

        public double VolumeFittings
        {
            get
            {
                return volumeFittings;
            }

            set
            {
                volumeFittings = value;
            }
        }

        public double MassFittings
        {
            get
            {
                return massFittings;
            }

            set
            {
                massFittings = value;
            }
        }

        public DataPoints QFittings
        {
            get
            {
                return qFittings;
            }

            set
            {
                qFittings = value;
            }
        }

        public DataPoints TFittings
        {
            get
            {
                return tFittings;
            }

            set
            {
                tFittings = value;
            }
        }

        private double volumeFittings;
        private double massFittings;
        DataPoints qFittings;
        DataPoints tFittings;

        public Compartment(double length, double width, double height, double T0, Log log)
        {
            Volume = length * width * height;
            Mass = Volume * Physics.C_RHO_AIR;

            L = length;
            W = width;
            H = height;

            Q = new DataPoints(log);
            T = new DataPoints(log);

            Q.addPoints(0, Mass * Physics.C_CP_AIR * T0);
            T.addPoints(0, T0);

            Q.series.Name = "Q Kabinenluft";
            Q.series.ChartType = SeriesChartType.Line;
            T.series.Name = "T Kabinenluft";
            T.series.ChartType = SeriesChartType.Line;

            InteriorFittings = false;
        }

        public Compartment(double length, double width, double height, double T0, bool interior, Log log)
        {
            Volume = length * width * height;
            Mass = Volume * Physics.C_RHO_AIR;

            L = length;
            W = width;
            H = height;

            Q = new DataPoints(log);
            T = new DataPoints(log);

            Q.addPoints(0, Mass * Physics.C_CP_AIR * T0);
            T.addPoints(0, T0);

            Q.series.Name = "Q Kabinenluft";
            Q.series.ChartType = SeriesChartType.Line;
            T.series.Name = "T Kabinenluft";
            T.series.ChartType = SeriesChartType.Line;

            InteriorFittings = interior;

            if(InteriorFittings)
            {
                VolumeFittings = 0.5 * 0.7 * 0.2 * 6; //Volumen von 6 Sitzen
                MassFittings = VolumeFittings * Physics.C_RHO_INTERIOR;

                QFittings = new DataPoints(log);
                TFittings = new DataPoints(log);

                QFittings.addPoints(0, mass * Physics.C_CV_INTERIOR * T0);
                TFittings.addPoints(0, T0);

                QFittings.series.Name = "Q Interior";
                QFittings.series.ChartType = SeriesChartType.Line;
                TFittings.series.Name = "T Interior";
                TFittings.series.ChartType = SeriesChartType.Line;

            }
        }

    }
}
