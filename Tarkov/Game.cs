namespace TarkovDMATest.Tarkov;

public class Game
{
    private ulong _unityBase;
    private ulong _localGameWorld;
    private Player _player;
    public Game(ulong unityBase)
    {
        this._unityBase = unityBase;
        this.getGOM();
        
        if (this._player != null)
        {
            this._player.setNoRecoil();
        }
    }

    private void getGOM()
    {
        try
        {
            var addr = Memory.ReadPtr(this._unityBase + Offsets.ModuleBase.GameObjectManager);
            GameObjectManager gom = Memory.ReadValue<GameObjectManager>(addr);
            Console.WriteLine($"Found Game Object Manager at 0x{addr.ToString("X")}");
                
            var activeNodes = Memory.ReadPtr(gom.ActiveNodes);
            var lastActiveNode = Memory.ReadPtr(gom.LastActiveNode);
            var gameWorld = this.GetObjectFromList(activeNodes, lastActiveNode, "GameWorld");
            
            this._localGameWorld = Memory.ReadPtrChain(gameWorld, Offsets.GameWorld.To_LocalGameWorld);
            Program.Log($"Found LocalGameWorld at 0x{this._localGameWorld.ToString("X")}");

            this.getLocalPlayer();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Cant find GOM");
        }
    }

    private void getLocalPlayer()
    {
        try
        {
            var localPlayer = Memory.ReadPtr(this._localGameWorld + Offsets.LocalGameWorld.MainPlayer);
            Program.Log($"Found LocalPlayer at 0x{localPlayer.ToString("X")}");
            this._player = new Player(localPlayer);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Cant find LocalPlayer");
        }
    }
    
    private ulong GetObjectFromList(ulong activeObjectsPtr, ulong lastObjectPtr, string objectName)
        {
            var activeObject = Memory.ReadValue<BaseObject>(Memory.ReadPtr(activeObjectsPtr));
            var lastObject = Memory.ReadValue<BaseObject>(Memory.ReadPtr(lastObjectPtr));

            if (activeObject.obj != 0x0 && lastObject.obj == 0x0)
            {
                Program.Log("Waiting for lastObject to be populated...");
                while (lastObject.obj == 0x0)
                {
                    lastObject = Memory.ReadValue<BaseObject>(Memory.ReadPtr(lastObjectPtr));
                    Thread.Sleep(1000);
                }
            }

            while (activeObject.obj != 0x0 && activeObject.obj != lastObject.obj)
            {
                ulong objectNamePtr = Memory.ReadPtr(activeObject.obj + Offsets.GameObject.ObjectName);
                string objectNameStr = Memory.ReadString(objectNamePtr, 64);

                if (string.Equals(objectNameStr, objectName, StringComparison.OrdinalIgnoreCase))
                {
                    ulong _localGameWorld = Memory.ReadPtrChain(activeObject.obj, Offsets.GameWorld.To_LocalGameWorld);
                    if (!Memory.ReadValue<bool>(_localGameWorld + Offsets.LocalGameWorld.RaidStarted))
                    {
                        activeObject = Memory.ReadValue<BaseObject>(activeObject.nextObjectLink);
                        continue;
                    }

                    Program.Log($"Found object {objectNameStr}");
                    return activeObject.obj;
                }

                activeObject = Memory.ReadValue<BaseObject>(activeObject.nextObjectLink);
            }

            if (lastObject.obj != 0x0)
            {
                ulong objectNamePtr = Memory.ReadPtr(lastObject.obj + Offsets.GameObject.ObjectName);
                string objectNameStr = Memory.ReadString(objectNamePtr, 64);

                if (string.Equals(objectNameStr, objectName, StringComparison.OrdinalIgnoreCase))
                {
                    Program.Log($"Found object {objectNameStr}");
                    return lastObject.obj;
                }
            }

            Program.Log($"Couldn't find object {objectName}");
            return 0;
        }
}