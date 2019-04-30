using Assistant;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CEasyUO
{
    public partial class CEasyUOMainForm : Form
    {
        public CEasyUOMainForm()
        {
            InitializeComponent();
            this.Text = $"CEasyUO {Assembly.GetExecutingAssembly().GetName().Version}";
            SetupVarsTimer();
            btnPause.Enabled = false;
            btnStop.Enabled = false;
        }

        private void SetupVarsTimer()
        {
            treeVarTree.Nodes.Add( new TreeNode( "Character Info" ) );
            treeVarTree.Nodes.Add( new TreeNode( "Status Bar" ) );
            treeVarTree.Nodes.Add( new TreeNode( "Container Info" ) );
            treeVarTree.Nodes.Add( new TreeNode( "Last Action" ) );
            treeVarTree.Nodes.Add( new TreeNode( "Find Item" ) );
            treeVarTree.Nodes.Add( new TreeNode( "Extended Info" ) );
            Thread t = new Thread( new ThreadStart( () => {
                while ( true )
                {
                    Thread.Sleep( 1000 );
                    if(this.IsHandleCreated)
                    if ( InvokeRequired )
                    {
                       BeginInvoke( new MethodInvoker( UpdateVars ) );
                    }
                    else
                    {
                       UpdateVars();
                        // Do things
                    }
                }
                
                

            } ) );
            t.IsBackground = true;
            t.Start();

        }

        private void UpdateVars()
        {
            if(Interpreter != null && Interpreter.CurrentStatment != null)
            {

            }
            try
            {
                //txtDebug.Text = "CurrentLine: " + Parser?.CurrentLine ?? "0";
                if ( !Engine.IsInstalled || World.Player == null )
                {
                    return;
                }

                Dictionary<string, object> charinfo = BuildCharInfo();


                Dictionary<string, object> last = BuildLastInfo();

                Dictionary<string, object> container = new Dictionary<string, object>();
                container.Add( "#GUMPPOSX", EUOInterpreter.GetVariable<string>( "#GUMPPOSX" ) );
                container.Add( "#GUMPPOSY", EUOInterpreter.GetVariable<string>( "#GUMPPOSY" ) );
                container.Add( "#GUMPSIZEX", EUOInterpreter.GetVariable<string>( "#GUMPSIZEX" ) );
                container.Add( "#GUMPSIZEY", EUOInterpreter.GetVariable<string>( "#GUMPSIZEY" ) );
                container.Add( "#CONTKIND", EUOInterpreter.GetVariable<string>( "#CONTKIND" ) );
                container.Add( "#CONTID", EUOInterpreter.GetVariable<string>( "#CONTID" ) );
                container.Add( "#CONTTYPE", EUOInterpreter.GetVariable<string>( "#CONTTYPE" ) );
                container.Add( "#CONTHP", "N/A" );

                container.Add( "#GUMPSERIAL", EUOInterpreter.GetVariable<string>( "#GUMPSERIAL" ) );
                container.Add( "#GUMPTYPE", EUOInterpreter.GetVariable<string>( "#GUMPTYPE" ) );

                Dictionary<string, object> find = new Dictionary<string, object>();
                find.Add( "#FINDID", EUOInterpreter.GetVariable<string>( "#findid" ) );
                find.Add( "#FINDTYPE", EUOInterpreter.GetVariable<string>( "#FINDTYPE" ) );
                find.Add( "#FINDX", EUOInterpreter.GetVariable<string>( "#FINDX" ) );
                find.Add( "#FINDY", EUOInterpreter.GetVariable<string>( "#FINDY" ) );
                find.Add( "#FINDZ", EUOInterpreter.GetVariable<string>( "#FINDZ" ) );
                find.Add( "#FINDDIST", EUOInterpreter.GetVariable<string>( "#FINDZ" ) );
                find.Add( "#FINDKIND", EUOInterpreter.GetVariable<string>( "#FINDZ" ) );
                find.Add( "#FINDSTACK", EUOInterpreter.GetVariable<string>( "#FINDZ" ) );
                find.Add( "#FINDBAGID", EUOInterpreter.GetVariable<string>( "#FINDZ" ) );
                find.Add( "#FINDMOD", EUOInterpreter.GetVariable<string>( "#FINDZ" ) );
                find.Add( "#FINDREP", EUOInterpreter.GetVariable<string>( "#FINDZ" ) );
                find.Add( "#FINDCOL", EUOInterpreter.GetVariable<string>( "#FINDZ" ) );
                find.Add( "#FINDINDEX", EUOInterpreter.GetVariable<string>( "#FINDZ" ) );
                find.Add( "#FINDCNT", EUOInterpreter.GetVariable<string>( "#FINDZ" ) );


                Dictionary<string, object> status = new Dictionary<string, object>();
                status.Add( "#CHARNAME", EUOInterpreter.GetVariable<string>( "#CHARNAME" ) );
                status.Add( "#SEX", EUOInterpreter.GetVariable<string>( "#SEX" ) );
                status.Add( "#STR", EUOInterpreter.GetVariable<string>( "#STR" ) );
                status.Add( "#DEX", EUOInterpreter.GetVariable<string>( "#DEX" ) );
                status.Add( "#INT", EUOInterpreter.GetVariable<string>( "#INT" ) );
                status.Add( "#HITS", EUOInterpreter.GetVariable<string>( "#HITS" ) );
                status.Add( "#MAXHITS", EUOInterpreter.GetVariable<string>( "#MAXHITS" ) );
                status.Add( "#MANA", EUOInterpreter.GetVariable<string>( "#MANA" ) );
                status.Add( "#MAXMANA", EUOInterpreter.GetVariable<string>( "#MAXMANA" ) );
                status.Add( "#STAMINA", EUOInterpreter.GetVariable<string>( "#STAMINA" ) );
                status.Add( "#MAXSTAM", EUOInterpreter.GetVariable<string>( "#MAXSTAM" ) );
                status.Add( "#MAXSTATS", EUOInterpreter.GetVariable<string>( "#MAXSTATS" ) );
                status.Add( "#LUCK", EUOInterpreter.GetVariable<string>( "#LUCK" ) );
                status.Add( "#WEIGHT", EUOInterpreter.GetVariable<string>( "#WEIGHT" ) );
                status.Add( "#MAXWEIGHT", EUOInterpreter.GetVariable<string>( "#MAXWEIGHT" ) );
                status.Add( "#MINDMG", EUOInterpreter.GetVariable<string>( "#MINDMG" ) );
                status.Add( "#MAXDMG", EUOInterpreter.GetVariable<string>( "#MAXDMG" ) );
                status.Add( "#GOLD", EUOInterpreter.GetVariable<string>( "#GOLD" ) );
                status.Add( "#FOLLOWERS", EUOInterpreter.GetVariable<string>( "#FOLLOWERS" ) );
                status.Add( "#MAXFOL", EUOInterpreter.GetVariable<string>( "#MAXFOL" ) );
                status.Add( "#AR", EUOInterpreter.GetVariable<string>( "#AR" ) );
                status.Add( "#FR", EUOInterpreter.GetVariable<string>( "#FR" ) );
                status.Add( "#CR", EUOInterpreter.GetVariable<string>( "#CR" ) );
                status.Add( "#PR", EUOInterpreter.GetVariable<string>( "#PR" ) );
                status.Add( "#ER", EUOInterpreter.GetVariable<string>( "#ER" ) );

                Dictionary<string, object> extended = new Dictionary<string, object>();
                extended.Add( "#JOURNAL", EUOInterpreter.GetVariable<string>( "#JOURNAL" ) );
                extended.Add( "#JCOLOR", EUOInterpreter.GetVariable<string>( "#JCOLOR" ) );
                extended.Add( "#JINDEX", EUOInterpreter.GetVariable<string>( "#JINDEX" ) );
                extended.Add( "#SYSMSG", EUOInterpreter.GetVariable<string>( "#SYSMSG" ) );
                extended.Add( "#TARGCURS", EUOInterpreter.GetVariable<string>( "#TARGCURS" ) );
                extended.Add( "#CURSKIND", EUOInterpreter.GetVariable<string>( "#CURSKIND" ) );
                extended.Add( "#PROPERTY", EUOInterpreter.GetVariable<string>( "#PROPERTY" ) );

                Dictionary<string, object> results = new Dictionary<string, object>();
                results.Add( "#RESULT", EUOInterpreter.GetVariable<string>( "#RESULT" ) );
                results.Add( "#STRRES", EUOInterpreter.GetVariable<string>( "#STRRES" ) );
                results.Add( "#MENURES", EUOInterpreter.GetVariable<string>( "#MENURES" ) );
                results.Add( "#DISPRES", EUOInterpreter.GetVariable<string>( "#DISPRES" ) );

                foreach ( TreeNode n in treeVarTree.Nodes )
                {
                    if ( n.Text == "Last Action" )
                        UpdateChildren( (TreeNode)n, last );
                    if ( n.Text == "Character Info" )
                        UpdateChildren( (TreeNode)n, charinfo );
                    if ( n.Text == "Find Item" )
                        UpdateChildren( (TreeNode)n, find );
                    if ( n.Text == "Status Bar" )
                        UpdateChildren( (TreeNode)n, status );
                    if ( n.Text == "Container Info" )
                        UpdateChildren( (TreeNode)n, container );
                    if ( n.Text == "Result Variables" )
                        UpdateChildren( (TreeNode)n, results );
                    if ( n.Text == "Extended Info" )
                        UpdateChildren( (TreeNode)n, extended );
                }
            }
            catch ( Exception e )
            {

                Console.WriteLine( e.Message + e.StackTrace );
            }
        }

        private Dictionary<string, object> BuildLastInfo()
        {
           var last = new Dictionary<string, object>();
            last.Add( "#LOBJECTID", EUOInterpreter.GetVariable<string>( "#LOBJECTID" ) );
            last.Add( "#LOBJECTTYPE", EUOInterpreter.GetVariable<string>( "#LOBJECTTYPE" ) );
            last.Add( "#LTARGETID", EUOInterpreter.GetVariable<string>( "#LTARGETID" ) );
            last.Add( "#LTARGETTYPE", EUOInterpreter.GetVariable<string>( "#LTARGETTYPE" ) );
            last.Add( "#LTARGETX", EUOInterpreter.GetVariable<string>( "#LTARGETX" ) );
            last.Add( "#LTARGETY", EUOInterpreter.GetVariable<string>( "#LTARGETY" ) );
            last.Add( "#LTARGETZ", EUOInterpreter.GetVariable<string>( "#LTARGETZ" ) );
            last.Add( "#LTARGETKIND", EUOInterpreter.GetVariable<string>( "#LTARGETKIND" ) );
            last.Add( "#LTARGETTILE", EUOInterpreter.GetVariable<string>( "#LTARGETTILE" ) );
            last.Add( "#LSKILL", EUOInterpreter.GetVariable<string>( "#LSKILL" ) );
            last.Add( "#LSPELL", EUOInterpreter.GetVariable<string>( "#LSPELL" ) );

            last.Add( "#LGUMPBUTTON", EUOInterpreter.GetVariable<string>( "#LGUMPBUTTON" ) );

            return last;
        }

        private Dictionary<string, object> BuildCharInfo()
        {
           var charinfo = new Dictionary<string, object>();
            charinfo.Add( "#CHARPOSX", EUOInterpreter.GetVariable<string>( "#CHARPOSX" ) );
            charinfo.Add( "#CHARPOSY", EUOInterpreter.GetVariable<string>( "#CHARPOSY" ) );
            charinfo.Add( "#CHARPOSZ", EUOInterpreter.GetVariable<string>( "#CHARPOSZ" ) );
            charinfo.Add( "#CHARDIR", EUOInterpreter.GetVariable<string>( "#CHARDIR" ) );
            charinfo.Add( "#CHARSTATUS", EUOInterpreter.GetVariable<string>( "#CHARSTATUS" ) ); ;
            charinfo.Add( "#CHARID", EUOInterpreter.GetVariable<string>( "#CHARID" ) );
            charinfo.Add( "#CHARTYPE", EUOInterpreter.GetVariable<string>( "#CHARTYPE" ) );
            charinfo.Add( "#CHARGHOST", EUOInterpreter.GetVariable<string>( "#CHARGHOST" ) );
            charinfo.Add( "#CHARBACKPACKID", EUOInterpreter.GetVariable<string>( "#CHARBACKPACKID" ) );
            return charinfo;
        }

        private void UpdateChildren( TreeNode n, Dictionary<string, object> dict )
        {   try
            {
                foreach ( var c in dict )
                {
                    if ( n.Nodes.ContainsKey( c.Key ) )
                        n.Nodes[c.Key].Text = c.Key + ": " + c.Value.ToString();
                    else
                        n.Nodes.Add( c.Key, c.Key + ": " + c.Value.ToString() );
                }
            }
            catch (Exception e)
            {
                Debugger.Break();
                Console.WriteLine( e.Message + e.StackTrace );
            }
            
        }
 
        public EUOInterpreter Interpreter;
        private FileStream m_OpenFile;

        public string m_FilePath { get; private set; }

        private void btnPlayClicked( object sender, EventArgs e )
        {
            if ( Interpreter == null || Interpreter.Script != txtScriptEntry.Text )
            {
                Interpreter = new EUOInterpreter( txtScriptEntry.Text );
                UpdateAST();
            }

            if(Interpreter.Running && Interpreter.Paused)
            {
                Interpreter.Paused = false;
            }
            else if ( !Interpreter.Running )
            {
                Interpreter.Run();
            }

            btnPlay.Enabled = false;
            btnStop.Enabled = true;
            btnPause.Enabled = true;
            txtDebug.Text = "Running...";

        }

        private void btnPauseClicked( object sender, EventArgs e )
        {
            if ( Interpreter == null )
                return;
            if ( !Interpreter.Running )
                return;

            Interpreter.Paused = true;

            btnPlay.Enabled = true;
            btnStop.Enabled = true;
            btnPause.Enabled = false;
            txtDebug.Text = "Paused on Line: " + Interpreter.CurrentLine;
        }

        private void btnStopClicked( object sender, EventArgs e )
        {
            if ( Interpreter == null )
                return;
            if ( Interpreter.Running )
                Interpreter.Stop();
            btnPlay.Enabled = true;
            btnStop.Enabled = false;
            btnPause.Enabled = false;
            txtDebug.Text = "Stopped...";
        }

        private void btnStepClicked( object sender, EventArgs e )
        {
            try
            {
                if ( Interpreter == null || Interpreter.Script != txtScriptEntry.Text )
                {
                    Interpreter = new EUOInterpreter( txtScriptEntry.Text );
                    UpdateAST();
                }
                    

                Interpreter.Statement();
                txtDebug.Text = "Current Line: " + Interpreter.CurrentLine + " Current Statement: " + Interpreter.CurrentStatment?? "null";
                var start = txtScriptEntry.GetFirstCharIndexFromLine( Interpreter.CurrentLine -1 );
                var end = txtScriptEntry.GetFirstCharIndexFromLine( Interpreter.CurrentLine ) - 1;

                txtScriptEntry.SelectionStart = 0;
                txtScriptEntry.SelectionLength = txtScriptEntry.Text.Length;
                txtScriptEntry.SelectionBackColor = SystemColors.Window;


                txtScriptEntry.SelectionStart = start;
                txtScriptEntry.SelectionLength = end - start;
                txtScriptEntry.SelectionBackColor = Color.Red;
                txtScriptEntry.SelectionBullet = true;
                txtScriptEntry.SelectionLength = 0;

            } catch(Exception ee )
            {
                txtDebug.Text = "E: " + ee.Message;
            }
           
            
        }

        private void btnNew_Click( object sender, EventArgs e )
        {
            txtScriptEntry.Text = "";
            if(m_OpenFile != null)
            {
                m_OpenFile.Close();
                m_OpenFile = null;
            }
            m_FilePath = null;
        }

        private void btnOpen_Click( object sender, EventArgs e )
        {
            var diag = new OpenFileDialog();
            if(diag.ShowDialog() == DialogResult.OK )
            {
                m_OpenFile = File.Open( diag.FileName, FileMode.OpenOrCreate );
                m_FilePath = diag.FileName;
                using(var sr = new StreamReader( m_OpenFile ) )
                    txtScriptEntry.Text = sr.ReadToEnd();
            }
        }

        private void btnSave_Click( object sender, EventArgs e )
        {
            if ( m_FilePath != null )
            {
                m_OpenFile = File.Open( m_FilePath, FileMode.Truncate );
                using ( var sw = new StreamWriter( m_OpenFile ) )
                { 
                    sw.Write( txtScriptEntry.Text );
                    //sw.Flush();
                }

            }
            else
            {
                var diag = new SaveFileDialog();
                if ( diag.ShowDialog() == DialogResult.OK )
                {
                    m_OpenFile = File.Open( diag.FileName, FileMode.OpenOrCreate );
                    m_FilePath = diag.FileName;
                    using(var sw = new StreamWriter( m_OpenFile ) )
                    {
                        sw.Write( txtScriptEntry.Text );
                        //sw.Flush();
                    }
                    
                }
            }

        }

        private void btnCompile_Click( object sender, EventArgs e )
        {
            UpdateVars();
            UpdateAST();
        }

        public void UpdateAST()
        {
            if ( Interpreter == null || Interpreter.Script != txtScriptEntry.Text )
                Interpreter = new EUOInterpreter( txtScriptEntry.Text );
            tree_AST.Nodes.Clear();

            foreach ( var n in ( Interpreter.AST.First() as Block ).statements )
                tree_AST.Nodes.AddRange( AddTree( n ) );
        }

        private TreeNode[] AddTree( Stmt n )
        {
            
            var node = new TreeNode( n.ToString() );
            if ( n is Block b )
            {
                foreach(var s in b.statements)
                    node.Nodes.AddRange( AddTree( s ) );
            }
            return new TreeNode[] { node };

        }
    }
}
