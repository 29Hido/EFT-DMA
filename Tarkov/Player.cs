namespace TarkovDMATest.Tarkov;

public class Player
{
    private ulong _address;
    
    public Player(ulong address)
    {
        this._address = address;
    }

    public void setNoRecoil()
    {
        try
        {
            var proceduralWeaponAnimationPtr = Memory.ReadPtr(this._address + Offsets.Player.ProceduralWeaponAnimation);
            Program.Log($"Found ProceduralWeaponAnimation at 0x{proceduralWeaponAnimationPtr.ToString("X")}");

            Memory.WriteValue<int>(proceduralWeaponAnimationPtr + Offsets.ProceduralWeaponAnimation.Mask, 1);
            Program.Log("No recoil activated");
        }
        catch (Exception e)
        {
            Program.Log("Cant find ProceduralWeaponAnimation");
        }
    }
}