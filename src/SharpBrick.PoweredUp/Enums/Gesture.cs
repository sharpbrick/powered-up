namespace SharpBrick.PoweredUp
{
    public enum Gesture : sbyte
    {
        None = 0, // UNSPECED
        /// <summary>
        /// May be "Tap" gesture.
        /// 
        /// Mapping is not well understood. Please contribute an issue if you have found an issue, additional insight or documentation.
        /// </summary>
        Gesture1 = 1, // UNSPECED

        /// <summary>
        /// Some undocumented gesture (maybe bounce).
        /// 
        /// Mapping is not well understood. Please contribute an issue if you have found an issue, additional insight or documentation.
        /// </summary>
        Gesture2 = 2,

        /// <summary>
        /// May be "Shake" gesture.
        /// 
        /// Mapping is not well understood. Please contribute an issue if you have found an issue, additional insight or documentation.
        /// </summary>
        Gesture3 = 3, // UNSPECED

        /// <summary>
        /// May be "free fall" gesture.
        /// 
        /// Mapping is not well understood. Please contribute an issue if you have found an issue, additional insight or documentation.
        /// </summary>
        Gesture4 = 4, // UNSPECED
    }
}