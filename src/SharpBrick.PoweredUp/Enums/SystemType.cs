namespace SharpBrick.PoweredUp
{
    public enum SystemType : byte
    {
        LegoWeDo20_WeDoHub = 0b000_00000,

        LegoDuplo_DuploTrain = 0b001_00000,

        LegoSystem_BoostHub = 0b010_00000,
        LegoSystem_TwoPortHub = 0b010_00001,
        LegoSystem_TwoPortHandset = 0b010_00010,
        LegoSystem_Mario = 0b0100_0011,  // UNSPECED, https://github.com/bricklife/LEGO-Mario-Reveng, 0x43,

        LegoTechnic_MediumHub = 0b100_00000, // UNSPECED
    }
}