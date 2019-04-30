using Assistant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEasyUO
{
    class EUOVars
    {
        public static bool AllowGround { get; private set; }
        public static uint CurrentID { get; private set; }
        public static byte CurFlags { get; private set; }
        public static TargetInfo LastTarget { get; private set; } = new TargetInfo();
        public static bool HasTarget { get; private set; }

        public static void Initialize()
        {
            PacketHandler.RegisterClientToServerViewer( 0x6C, new PacketViewerCallback( TargetResponse ) );
            PacketHandler.RegisterServerToClientViewer( 0x6C, new PacketViewerCallback( NewTarget ) );
            PacketHandler.RegisterServerToClientViewer( 0xAA, new PacketViewerCallback( CombatantChange ) );
        }


        private static void TargetResponse( PacketReader p, PacketHandlerEventArgs args )
        {
            TargetInfo info = new TargetInfo();
            info.Type = p.ReadByte();
            info.TargID = p.ReadUInt32();
            info.Flags = p.ReadByte();
            info.Serial = p.ReadUInt32();
            info.X = p.ReadUInt16();
            info.Y = p.ReadUInt16();
            info.Z = p.ReadInt16();
            info.Gfx = p.ReadUInt16();
            LastTarget = info;
            HasTarget = false;
        }
        private static void CombatantChange( PacketReader p, PacketHandlerEventArgs args )
        {
            Serial ser = p.ReadUInt32();
        }

        private static void NewTarget( PacketReader p, PacketHandlerEventArgs args )
        {
            AllowGround = p.ReadBoolean(); // allow ground
            CurrentID = p.ReadUInt32(); // target uid
            CurFlags = p.ReadByte(); // flags

            LastTarget.TargID = CurrentID;
            LastTarget.Flags = CurFlags;
            HasTarget = true;
        }

        internal static void SendTargetLast()
        {
            ClientCommunication.SendToServer( new TargetResponse( EUOVars.LastTarget ) ); //Targeting.Target( targ );
            ClientCommunication.SendToClient( new CancelTarget( EUOVars.CurrentID ) );
            HasTarget = false;
        }
    }
}
