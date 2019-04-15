using Assistant;
using CEasyUO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


namespace CEasyUO
{
    public class EUOInterpreter
    {
        private List<Stmt> m_Statements = new List<Stmt>();
        public Stmt CurrentStatment;


        private Dictionary<string,int> Labels = new Dictionary<string, int>();
        private Dictionary<string, int> Subs = new Dictionary<string, int>();
        private static Dictionary<string,object> Variables = new Dictionary<string, object>();

        private static Stack<Stmt> CurrentStack = null;

        private static Stack<Stack<Stmt>> OldStacks = new Stack<Stack<Stmt>>();

        private static Block m_CurBlock;
        private static Block CurrentBlock { get { return m_CurBlock; } set { m_CurBlock = value; } }

        public string Script { get; internal set; }
        public int CurrentLine => CurrentStatment?.Line ?? 0;
        public List<Stmt> AST => m_Statements;

        public bool Paused { get; internal set; }

        private static Stack<Block> BlockStack = new Stack<Block>();

        public EUOInterpreter( string script )
        {
            Script = script;
            var parser = new EUOParser( script );
            m_Statements = parser.GenerateAST();
        }

        Thread ScriptThread;
        public bool Running = false;

        public void Run()
        {
            if ( ScriptThread?.IsAlive == true && Thread.CurrentThread != ScriptThread )
                return;
            if ( ScriptThread == null || Thread.CurrentThread != ScriptThread )
            {
                ScriptThread = new Thread( new ThreadStart( Run ) );
                ScriptThread.IsBackground = true;
                ScriptThread.Start();
                return;
            }
            Running = true;
            while(Running)
            {
                while ( Paused )
                    Thread.Sleep( 50 );
                Statement();
                while ( CurrentStatment != null )
                {
                    Statement();
                    Thread.Sleep( 50 );
                    while ( Paused )
                        Thread.Sleep( 50 );
                }
                Thread.Sleep( 50 );
            }


            Console.WriteLine( "Script was stopped" );
            return;
        }

        public void Stop()
        {
            if ( !Running )
                return;
            Paused = false;
            Running = false;
            Thread.Sleep( 500 );
            if(ScriptThread.IsAlive)
                ScriptThread.Abort();
            ScriptThread = null;

            CurrentStatment = null;
            CurrentBlock = null;
            CurrentStack = null;
        }

        public void Statement()
        {
            if(CurrentBlock == null && CurrentStack == null)
            {
                CurrentBlock = m_Statements[0] as Block;
                CurrentStack = CurrentBlock.GetStack();
                CurrentStatment = CurrentStack.Pop();
            }

            
            if ( CurrentStatment is Goto go )
            {
                HandleGoto( go );
            }
            else if ( CurrentStatment is Call call )
            {
                HandleCall( call );
            }
            else if ( CurrentStatment is Break )
            {
                //pop till we leave a loop TODO

                CurrentBlock = BlockStack.Pop();
                CurrentStack = OldStacks.Pop();
                CurrentStatment = CurrentStack.Pop();
            }
            else if ( CurrentStatment is Return ret )
            {
                if ( ret.expr != null )
                    Setvariable( "#result", ret.expr.GetValue() );
                //pop till we leave a Func TODO
                CurrentBlock = BlockStack.Pop();
                CurrentStack = OldStacks.Pop();
                CurrentStatment = CurrentStack.Pop();
            }
            else if ( CurrentStatment is Continue )
            {
                CurrentStack = CurrentBlock.GetStack();
                CurrentStatment = CurrentStack.Pop();
            }
            else if ( CurrentStatment is Block block )
            {
                if ( CurrentStatment.Execute() )
                {
                    if ( block is BaseLoopBlock loop )
                        CurrentStack.Push( block );
                    OldStacks.Push( CurrentStack );

                    BlockStack.Push( CurrentBlock );
                    CurrentBlock = block;
                    CurrentStack = CurrentBlock.GetStack();
                    CurrentStatment = CurrentStack.Pop();
                    //foreach ( var s in CurrentBlock.statements )
                    //    CurrentStack.Push( s );
                }
                else
                {
                    if ( CurrentStack.Count > 0 )
                        CurrentStatment = CurrentStack.Pop();
                    else
                        CurrentStatment = null;
                }

            }
            //DEBUG DONT EXECUTE STATEMENTS
            else if(CurrentStatment != null && CurrentStatment.Execute() )
            {
                if(CurrentStack.Count > 0)
                    CurrentStatment = CurrentStack.Pop();
                else
                    CurrentStatment = null;
            }
                //
            if ( CurrentStatment == null )
            {
                if(BlockStack.Count > 0 )
                {
                    CurrentBlock = BlockStack.Pop();
                    CurrentStack = OldStacks.Pop();
                    if( CurrentStack.Count > 0)
                        CurrentStatment = CurrentStack.Pop();
                }
                else
                {
                    CurrentBlock = null;
                    CurrentStack = null;
                }
            }
        }

        private void HandleCall( Call call )
        {
            int index = 1;
            foreach ( var p in call.args )
                Setvariable( $"%{index++}", p.GetValue() );
            OldStacks.Push( CurrentStack );

            BlockStack.Push( CurrentBlock );
            var func = ( m_Statements[0] as Block ).statements.FirstOrDefault( t => ( ( t is Func tt ) && tt.ident == call.ident ) ) as Block;
            CurrentBlock = func;
            CurrentStack = CurrentBlock.GetStack();
            CurrentStatment = CurrentStack.Pop();
        }

        private void HandleGoto( Goto go )
        {
            //find the label
            //clear current execution stack and set to label and below
            var label = FindLabel( go.Name, m_Statements );
            if(label != null )
            {
                CurrentStack = label;
            }
            CurrentStatment = CurrentStack.Pop();
        }

        private Stack<Stmt> FindLabel( string name, List<Stmt> st )
        {
            var res = new List<Stmt>();
            var found = false; ;
            foreach(var s in st)
            {
                if ( found )
                {
                    res.Add( s );
                    continue;
                }

                if(s is Label lb && lb.Name == name )
                {
                    found = true;
                    res.Add( s );
                }
                if(s is Block block)
                {
                    var r = FindLabel( name, block.statements );
                    if ( r != null)
                        return r;
                }
            }
            res.Reverse();
            if ( found )
                return new Stack<Stmt>( res);
            return null;
        }

       
       

      

        private Stack<bool> IfStack = new Stack<bool>();
        
    /*    private void ParseIF()
        {
            CurrentIndex++;
            if ( CurrentToken.TokenName == Lexer.Tokens.LeftParan )
            {
                CurrentIndex++;
            }
            var lexpr = ParseExpr();

            var op = CurrentToken;  
            CurrentIndex++;
            var rexpr = ParseExpr();
            if ( CurrentToken.TokenName == Lexer.Tokens.RightParan )
            {
                CurrentIndex++;
            }
            (var startIf, var endIf) = parseBlock();

            var cnt = IfStack.Count;

            if(op.TokenName == Lexer.Tokens.Equal )
            {
                if ( lexpr.GetValue().Equals( rexpr.GetValue() ) )
                {
                    CurrentIndex = startIf;
                    if ( CurrentToken.TokenName == Lexer.Tokens.Else || NextToken.TokenName == Lexer.Tokens.Else ) IfStack.Push( true );
                }
            }
            else if ( op.TokenName == Lexer.Tokens.NotEqual )
            {
                if ( !lexpr.GetValue().Equals( rexpr.GetValue() ) )
                {
                    CurrentIndex = startIf; if ( CurrentToken.TokenName == Lexer.Tokens.Else || NextToken.TokenName == Lexer.Tokens.Else ) IfStack.Push( true );
                }
            }
            else if ( op.TokenName == Lexer.Tokens.MoreOrEqual || op.TokenName == Lexer.Tokens.MoreOrEqual2 )
            {
                if ( (int)lexpr.GetValue() >= (int)rexpr.GetValue()  )
                {
                    CurrentIndex = startIf; if ( CurrentToken.TokenName == Lexer.Tokens.Else || NextToken.TokenName == Lexer.Tokens.Else ) IfStack.Push( true );
                }
            }
            else if ( op.TokenName == Lexer.Tokens.LessOrEqual || op.TokenName == Lexer.Tokens.LessOrEqual2 )
            {
                if ( (int)lexpr.GetValue() <= (int)rexpr.GetValue() )
                {
                    CurrentIndex = startIf; if ( CurrentToken.TokenName == Lexer.Tokens.Else || NextToken.TokenName == Lexer.Tokens.Else ) IfStack.Push( true );
                }
            }
            else if ( op.TokenName == Lexer.Tokens.More )
            {
                if ( (int)lexpr.GetValue() > (int)rexpr.GetValue() )
                {
                    CurrentIndex = startIf; if ( CurrentToken.TokenName == Lexer.Tokens.Else || NextToken.TokenName == Lexer.Tokens.Else ) IfStack.Push( true );
                }
            }
            else if ( op.TokenName == Lexer.Tokens.Less )
            {
                if ( (int)lexpr.GetValue() < (int)rexpr.GetValue() )
                {
                    CurrentIndex = startIf; if( CurrentToken.TokenName == Lexer.Tokens.Else || NextToken.TokenName == Lexer.Tokens.Else ) IfStack.Push( true );
                }
            }

            if ( cnt == IfStack.Count ) // false
                if ( CurrentToken.TokenName == Lexer.Tokens.Else || NextToken.TokenName == Lexer.Tokens.Else ) IfStack.Push( false );

        }
    */
      

        private void EventMacro()
        {
          /*  if (CurrentToken.TokenName == Lexer.Tokens.IntLiteral)
            {
                int idOne = int.Parse(CurrentToken.TokenValue);
                int idTwo = 0;
                if (NextToken.TokenName == Lexer.Tokens.IntLiteral)
                {
                    CurrentIndex++;
                    idTwo = int.Parse(CurrentToken.TokenValue);
                }

                switch (idOne)
                {
                    case 22: // last target
                        var targ = Form1.EUO2StealthID(GetVariable<string>("#ltargetid"));
                         Targeting.Target( targ );
                        //Player.Targeting.TargetTo(targ);
                        break;
                    case 13:
                        ClientCommunication.SendToServer( new UseSkill( idTwo ) );
                        // Player.UseSkill(idTwo);
                        break;
                        
                }
                    
            }
            else
            {
                throw new Exception($"Unhandled event {CurrentToken.TokenValue} at line {CurrentLine}");
            }*/
        }
        public static T GetVariable<T>(string name)
        {
            name = name.ToLowerInvariant();

            try
            {
                switch ( name )
                {
                    case "#charname":
                        return (T)(object)( World.Player?.Name ?? "N/A" );
                    case "#sex":
                        return (T)(object)( World.Player?.Female  );
                    case "#str":
                        return (T)(object)( World.Player?.Str.ToString() ?? "0" );
                    case "#dex":
                        return (T)(object)( World.Player?.Dex.ToString() ?? "0" );
                    case "#int":
                        return (T)(object)( World.Player?.Int.ToString() ?? "0" );
                    case "#hits":
                        return (T)(object)( World.Player?.Hits.ToString() ?? "0" );
                    case "#maxhits":
                        return (T)(object)( World.Player?.HitsMax.ToString() ?? "0" );
                    case "#stamina":
                        return (T)(object)( World.Player?.Stam.ToString() ?? "0" );
                    case "#maxstam":
                        return (T)(object)( World.Player?.StamMax.ToString() ?? "0" );
                    case "#mana":
                        return (T)(object)( World.Player?.Mana.ToString() ?? "0" );
                    case "#maxmana":
                        return (T)(object)( World.Player?.ManaMax.ToString() ?? "0" );
                    case "#maxstats":
                        return (T)(object)( World.Player?.StatCap.ToString() ?? "0" );
                    case "#luck":
                        return (T)(object)( World.Player?.Luck.ToString() ?? "0" );
                    case "#weight":
                        return (T)(object)( World.Player?.Weight.ToString() ?? "0" );
                    case "#maxweight":
                        return (T)(object)( World.Player?.MaxWeight.ToString() ?? "0" );
                    case "#mindmg":
                        return (T)(object)( World.Player?.DamageMin.ToString() ?? "0" );
                    case "#maxdmg":
                        return (T)(object)( World.Player?.DamageMax.ToString() ?? "0" );
                    case "#gold":
                        return (T)(object)( World.Player?.Gold.ToString() ?? "0" );
                    case "#followers":
                        return (T)(object)( World.Player?.Followers.ToString() ?? "0" );
                    case "#maxfol":
                        return (T)(object)( World.Player?.FollowersMax.ToString() ?? "0" );
                    case "#ar":
                        return (T)(object)( World.Player?.AR.ToString() ?? "0" );
                    case "#fr":
                        return (T)(object)( World.Player?.FireResistance.ToString() ?? "0" );
                    case "#cr":
                        return (T)(object)( World.Player?.ColdResistance.ToString() ?? "0" );
                    case "#pr":
                        return (T)(object)( World.Player?.PoisonResistance.ToString() ?? "0" );
                    case "#er":
                        return (T)(object)( World.Player?.EnergyResistance.ToString() ?? "0" );


                    case "#charposx":
                        return (T)(object)( World.Player?.Position.X.ToString() ?? "0" );
                    case "#charposy":
                        return (T)(object)( World.Player?.Position.Y.ToString() ?? "0" );
                    case "#charposz":
                        return (T)(object)( World.Player?.Position.Z.ToString() ?? "0" );
                    case "#chardir":
                        return (T)(object)( (int)World.Player?.Direction );
                    case "#charstatus":
                        return (T)(object)( World.Player.GetStatusCode() );
                    case "#charid":
                        return (T)(object)Utility.UintToEUO( World.Player.Serial.Value ).ToString();
                    case "#chartype":
                        return (T)(object)Utility.UintToEUO( World.Player.GraphicID ).ToString();
                    case "#charghost":
                        return (T)(object)World.Player.IsGhost.ToString();
                    case "#charbackpackid":
                        return (T)(object)Utility.UintToEUO( World.Player?.Backpack?.Serial ?? 0 ).ToString();


                    case "#lobjectid":
                        return (T)(object)Utility.UintToEUO( World.Player.LastObject.Value ).ToString();
                    case "#lobjecttype":
                        return (T)(object)Utility.UintToEUO( World.Player.LastObjectAsEntity?.GraphicID ?? 0 ).ToString();
                    case "#ltargetid":
                        return (T)(object)Utility.UintToEUO( EUOVars.LastTarget?.Serial ?? 0 ).ToString();
                    case "#ltargetx":
                        return (T)(object)( EUOVars.LastTarget?.X.ToString() ?? "0" );
                    case "#ltargety":
                        return (T)(object)( EUOVars.LastTarget?.Y.ToString() ?? "0" );
                    case "#ltargetz":
                        return (T)(object)( EUOVars.LastTarget?.Z.ToString() ?? "0" );
                    case "#ltargetkind":
                        return (T)(object)( EUOVars.LastTarget?.Type ?? 0 ).ToString();
                    case "#ltargettile":
                        return (T)(object)( EUOVars.LastTarget?.Gfx ?? 0 ).ToString();
                    case "#lskill":
                        return (T)(object)World.Player.LastSkill.ToString();
                    case "#lspell":
                        return (T)(object)World.Player.LastSpell.ToString();
                    case "#lgumpbutton":
                        return (T)(object)(World.Player.LastGumpResponseAction?.Button.ToString() ?? "0");
                    case "#gumpserial":
                        return (T)(object)Utility.UintToEUO( World.Player.CurrentGumpS ).ToString();
                    case "#gumptype":
                        return (T)(object)Utility.UintToEUO( World.Player.CurrentGumpI ).ToString();


                    case "#gumpposx":
                        return (T)(object)( World.Player.LastGumpX.ToString() ?? "0" );
                    case "#gumpposy":
                        return (T)(object)( World.Player.LastGumpY.ToString() ?? "0" );
                    case "#gumpsizex":
                        return (T)(object)( World.Player.LastGumpWidth.ToString() ?? "0" );
                    case "#gumpsizey":
                        return (T)(object)( World.Player.LastGumpHeight.ToString() ?? "0" );


                    case "#contkind":
                        if(World.Player.LastContainerOpenedAt > World.Player.LastGumpOpenedAt)
                            return (T)(object)Utility.UintToEUO( World.Player.LastContainerGumpGraphic ).ToString();
                        else
                            return (T)(object)Utility.UintToEUO( World.Player.CurrentGumpI ).ToString();

                    case "#contid":
                        if ( World.Player.LastContainerOpenedAt > World.Player.LastGumpOpenedAt )
                            return (T)(object)Utility.UintToEUO( World.Player.LastContainer?.Serial ?? 0 ).ToString();
                        else
                            return (T)(object)Utility.UintToEUO( World.Player.CurrentGumpS ).ToString();

                    case "#conttype":// container item type
                        if ( World.Player.LastContainerOpenedAt > World.Player.LastGumpOpenedAt )
                            return (T)(object)Utility.UintToEUO( World.Player.LastContainer?.GraphicID ?? 0 ).ToString();
                        else
                            return (T)(object)Utility.UintToEUO( World.Player.CurrentGumpI ).ToString();

                    case "#sysmsg":
                        return (T)(object)( World.Player.LastSystemMessage ?? "N/A" );
                    case "#targcurs":
                        return (T)(object)( Targeting.HasTarget );
                }
            } catch
            {
                return (T)(object)"X";
            }
          
         

            if (! Variables.ContainsKey( name ) )
            {
                Setvariable( name, "X" );
            }
            var res = Variables[name];
            if ( res is T result )
                return result;
           if(typeof(T) == typeof(string) )
            {
                return (T)(object)res.ToString();
            }
            return (T)res ;
        }

        public static void Setvariable(string key, object value )
        {
            key = key.ToLowerInvariant();
            if ( Variables.ContainsKey( key ) )
                Variables[key] = value;
            else
                Variables.Add( key, value );
            if(value.ToString() != "x")
                switch ( key )
                {
                    case "#lobjectid":
                        if(World.Player != null)
                            World.Player.LastObject = Utility.EUO2StealthID( value.ToString() );
                        break;
                    case "#ltargetid":
                        EUOVars.LastTarget.Serial = Utility.EUO2StealthID( value.ToString() );
                        break;
                }
        }
        private void Set()
        {
          /*  CurrentIndex++;
            var variableName = ParseExpr();
            string varName = "";
            var value = ParseExpr().GetValue();
            if ( variableName is Ident i )
            {
                varName = i.value.ToLowerInvariant();
                Setvariable( varName, value );
            }
            else
            {
                varName = variableName.GetValue().ToString().ToLowerInvariant();
                Setvariable( varName, value );
            }
           
    */

                }

      
    }
}