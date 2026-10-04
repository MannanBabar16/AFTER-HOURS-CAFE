namespace Mannan.Coffee
{
    /// <summary>
    /// Lifecycle states for the espresso workstation.
    /// Strictly limits valid player actions and prevents state corruption.
    /// </summary>
    public enum EspressoMachineState
    {
        /// <summary>
        /// Workstation is idle; cup has not been placed under the portafilter spouts.
        /// </summary>
        Idle,

        /// <summary>
        /// Cup is positioned directly under the spouts; ready for extraction.
        /// </summary>
        CupReady,

        /// <summary>
        /// Espresso extraction is actively in progress; liquid is pouring into the cup.
        /// </summary>
        Extracting,

        /// <summary>
        /// Extraction is finished; espresso is ready to be inspected, cleared, or served.
        /// </summary>
        Completed
    }
}
