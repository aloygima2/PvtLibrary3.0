using System;
using System.Collections.Generic;
using System.Text;

namespace PvtLibrary3
{
    public class FluidProperties
    {
        /// <summary>
        /// Gas formation volume factor
        /// </summary>
        public double Bg { get; set; }

        /// <summary>
        /// Oil formation volume factor
        /// </summary>
        public double Bo { get; set; }

        /// <summary>
        /// Water formation volume factor
        /// </summary>
        public double Bw { get; set; }

        public double Co { get; set; }

        /// <summary>
        /// Oil fraction
        /// </summary>
        public double Fo { get; set; }

        /// <summary>
        /// Water fraction
        /// </summary>
        public double Fw { get; set; }

        /// <summary>
        /// Gas viscosity, in cp
        /// </summary>
        public double muG { get; set; }

        /// <summary>
        /// Liquid viscosity, in cp
        /// </summary>
        public double muL { get; set; }

        /// <summary>
        /// Oil viscosity, in cp
        /// </summary>
        public double muO { get; set; }

        /// <summary>
        /// Water viscosity, in cp
        /// </summary>
        public double muW { get; set; }
        
        /// <summary>
        /// Bubble point pressure, in psia
        /// </summary>
        public double Pb { get; set; }

        public double Pressure { get; set; }

        /// <summary>
        /// Gas rate, in Scf/Day
        /// </summary>
        public double Qg { get; set; }

        /// <summary>
        /// Oil rate, in STB/Day
        /// </summary>
        public double Qo { get; set; }

        /// <summary>
        /// Water rate, in STB/Day
        /// </summary>
        public double Qw { get; set; }
        
        /// <summary>
        /// Gas density, in lb/ft^3
        /// </summary>
        public double rhoG { get; set; }

        /// <summary>
        /// Liquid density, in lb/ft^3
        /// </summary>
        public double rhoL { get; set; }

        /// <summary>
        /// Oil density, in lb/ft^3
        /// </summary>
        public double rhoO { get; set; }

        /// <summary>
        /// Water density, in lb/ft^3
        /// </summary>
        public double rhoW { get; set; }

        /// <summary>
        /// Liquid surface tension, in dynes/cm
        /// </summary>
        public double sigmaL { get; set; }

        public double Rs { get; set; }

        /// <summary>
        /// Surface tension of oil, in dynes/cm
        /// </summary>
        public double sigmaO { get; set; }

        /// <summary>
        /// Surface tension of water, dynes/cm
        /// </summary>
        public double sigmaW { get; set; }

        public double Temperature { get; set; }

        /// <summary>
        /// Gas superficial velocity, ft/sec
        /// </summary>
        public double vsg { get; set; }

        /// <summary>
        /// Liquid superficial velocity, in ft/sec
        /// </summary>
        public double vsl { get; set; }

        public double ZFactor { get; set; }
    }
}
