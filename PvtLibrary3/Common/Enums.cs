using System;
using System.Collections.Generic;
using System.Text;

namespace PvtLibrary3.Common
{
    /// <summary>
    /// Type of gas
    /// </summary>
    public enum GasType
    {
        /// <summary>
        /// Natural gas
        /// </summary>
        Natural,
        /// <summary>
        /// Gas condensate
        /// </summary>
        Condensate
    }

    /// <summary>
    /// Choice of black oil correlation
    /// </summary>
    public enum BlackOilCorr
    {
        /// <summary>
        /// AlMarhoun blackoil correlation
        /// </summary>
        AlMarhoun,
        /// <summary>
        /// DeGhetto blackoil correlation
        /// </summary>
        DeGhetto,
        /// <summary>
        /// Glaso blackoil correlation
        /// </summary>
        Glaso,
        /// <summary>
        /// Lasater blackoil correlation
        /// </summary>
        Lasater,
        /// <summary>
        /// Petrosky et al blackoil correlation
        /// </summary>
        Petrosky,
        /// <summary>
        /// Standing correlation
        /// </summary>
        Standing,
        /// <summary>
        /// Vazquez and Beggs correlation
        /// </summary>
        VazquezBeggs
    }

    public enum GasViscosityCorr
    {
        GasViscosityLee
    }

    /// <summary>
    /// Choice of Oil viscosity correlation
    /// </summary>
    public enum OilViscosityCorr
    {
        BeggsRobinson,
        Beal_et_al,
        Petrosky_et_al,
        Egbogah_et_al,
        Bergman_Sutton,
        DeGhetto,
    }

    /// <summary>
    /// Pseudocritical pressure and temperature correlation
    /// </summary>
    public enum PseudoCritPtCorr
    {
        /// <summary>
        /// Sutton correlation
        /// </summary>
        Sutton,
        /// <summary>
        /// Standing correlation
        /// </summary>
        Standing
    }

    /// <summary>
    /// Z factor correlations
    /// </summary>
    public enum ZfactorCorr
    {
        /// <summary>
        /// Hallyarbourough correlation
        /// </summary>
        HallYarborough,
        /// <summary>
        /// Beggs and Brill correlation
        /// </summary>
        BeggsBrill
    }   

    /// <summary>
    ///option for  choosing the surface separator sequence.
    /// </summary>
    public enum SeparatorStage
    {
        /// <summary>
        /// denotes one separator sequence - stock tank condition
        /// </summary>
        SingleStage,
        /// <summary>
        /// denotes two separator sequence.
        /// the primary separator and the stock tank
        /// </summary>
        TwoStages,
        /// <summary>
        /// Three stage surface separator sequence
        /// </summary>
        ThreeStage
    }
}
