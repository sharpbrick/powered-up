namespace SharpBrick.PoweredUp
{
    // UNSPECED: https://github.com/bricklife/LEGO-Mario-Reveng/blob/master/IOType-0x4a.md
    public enum MarioPants : sbyte
    {
        None = 0b00_0000,
        Tanooki = 0b00_1010,
        Propeller = 0b00_1100,
        Cat = 0b01_0001,
        Fire = 0b01_0010,
        Penguin = 0b01_0100,
        Normal = 0b10_0001,
        Builder = 0b10_0010,
    }
}