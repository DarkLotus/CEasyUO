using CEasyUO;
using CUO_API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Assistant
{
    public class Engine
    {

        public Engine()
        {

        }
        public static ClientVersions ClientVersion { get; private set; }

        public static bool UseNewMobileIncoming => ClientVersion >= ClientVersions.CV_70331;

        public static bool UsePostHSChanges => ClientVersion >= ClientVersions.CV_7090;

        public static bool UsePostSAChanges => ClientVersion >= ClientVersions.CV_7000;

        public static bool UsePostKRPackets => ClientVersion >= ClientVersions.CV_6017;
        private static string _rootPath = null;
        public static string RootPath => _rootPath ?? ( _rootPath = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ) );
        public static string UOFilePath { get; internal set; }
        public static bool IsInstalled { get; internal set; }

        public static unsafe void Install( PluginHeader* header )
        {
            Console.WriteLine( "Install Invoked CEasyUO" );
            try
            {
                ClientVersion = (ClientVersions)header->ClientVersion;
                if ( !ClientCommunication.InstallHooks( header ) )
                {
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                    return;
                }
                UOFilePath = Marshal.GetDelegateForFunctionPointer<OnGetUOFilePath>( header->GetUOFilePath )();
                Ultima.Files.SetMulPath( UOFilePath );
                Ultima.Multis.PostHSFormat = UsePostHSChanges;
                Thread t = new Thread( () =>
                {
                    Thread.CurrentThread.Name = "EasyUO Main Thread";
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault( false );
                    Application.Run( new CEasyUOMainForm() );
                } );
                 t.SetApartmentState( ApartmentState.STA );
                PacketHandlers.Initialize();
               // Targeting.Initialize();
                Spell.Initialize(); EUOVars.Initialize();
                t.IsBackground = true;

                t.Start();
                IsInstalled = true;
            }
            catch (Exception e)
            {
                Debugger.Break();
                Console.WriteLine( e.Message );
            }
          
          

        }


        internal static void LogCrash( Exception e )
        {
        }
    }
}
