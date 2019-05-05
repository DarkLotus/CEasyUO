using Assistant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CEasyUO
{

    public enum Result
    {
        Finished,
        Success,
        Running
    }
    public class Stmt {
        public bool DEBUG = true;
        public int Line = 0;
        /// <summary>
        /// Statements return true if finished, false if run again
        /// Blocks return true if they should Execute
        /// </summary>
        /// <returns></returns>
        public virtual bool Execute() {
            if ( DEBUG )
                Console.WriteLine( $"Executing Statement on Line: {Line} " );
            return true;
        }
    }

    abstract class Expr
    {
        public abstract object GetValue();

        public int GetValueInt()
        {
            return int.Parse( GetValue().ToString() );
        }
       
    }

    public class Block : Stmt
    {
        public List<Stmt> statements;

        public Block()
        {
            statements = new List<Stmt>();
        }

        public Stack<Stmt> GetStack()
        {
            statements.Reverse();
                var res = new Stack<Stmt>( statements ); 
            statements.Reverse();
            return res;
        } 

        public void AddStmt(Stmt stmt)
        {
            statements.Add(stmt);
        }

        public override bool Execute()
        {
            Console.WriteLine( "Executing line:  " + Line + " " + this.ToString() );
            return base.Execute();
        }
    }

    public abstract class BaseLoopBlock : Block
    {

    }
    class FindItemStmt : Stmt
    {
        public List<uint> FindIDs = new List<uint>();
        public List<ushort> FindTypes = new List<ushort>();
        public int Index;
        public bool GroundOnly = false;
        public bool ContainerOnly = false;
        public uint ContainerSerial;

        private Expr Filter;
        private Expr Find;

        public FindItemStmt( Expr idType, IntLiteral index, Expr filter )
        {
            Find = idType;
          
            if(index != null)
                Index = index.value;
            Filter = filter;
        }
        public override bool Execute()
        {
            var ids = Find.GetValue().ToString().Split( new[] { '_' } );
            foreach ( var id in ids )
            {
                if ( id.Length == 3 )
                    FindTypes.Add( Utility.EUO2StealthType( id ) );
                else
                    FindIDs.Add( Utility.EUO2StealthID( id ) );
            }
            if ( Filter != null )
            {
                var str = Filter.GetValue().ToString().Trim();
                ContainerOnly = ( str.Contains( "C" ) );
                GroundOnly = ( str.Contains( "G" ) );
                try
                {
                    var id = str.Split( '_' )[1];
                    ContainerSerial = Utility.EUO2StealthID( id );
                }
                catch { }
            }

            var results = new List<IUOEntity>();
            foreach ( var i in FindIDs )
                results.Add( World.FindEntity( i ) );

            foreach ( var i in FindTypes )
                results.AddRange( World.Items.Values.Where( t => t.GraphicID == i ) );

            if ( ContainerOnly )
                results = results.Where( t => t.Parent != null ).ToList();
            if ( GroundOnly )
                results = results.Where( t => t.Parent == null ).ToList();
            var res = results.FirstOrDefault();

            EUOInterpreter.Setvariable( "#FINDID", Utility.UintToEUO( res?.Serial ?? 0) );
            EUOInterpreter.Setvariable( "#FINDTYPE", Utility.UintToEUO( res?.GraphicID ?? 0) );
            EUOInterpreter.Setvariable( "#findx", res?.Position.X ?? 0 );
            EUOInterpreter.Setvariable( "#findy", res?.Position.Y ?? 0 );
            EUOInterpreter.Setvariable( "#findz", res?.Position.Z ?? 0 );
            return base.Execute();
        }
    }
    class PauseStmt : Stmt
    {
        public PauseStmt(   )
        {
     

        }
    }
    class WaitStmt : Stmt
    {
        public Expr Index;
        public WaitStmt( Expr index )
        {
            Index = index;

        }
        public override bool Execute()
        {
            var timeout = (int)Index.GetValue();
            if ( timeout > 999 )
                Thread.Sleep( timeout );
            else
                Thread.Sleep( timeout * 50 );
            return base.Execute();
        }
    }
    class ScanJournalStmt : Stmt
    {
        public Expr Index;
        public ScanJournalStmt( Expr index)
        {
            Index = index;

        }
        public override bool Execute()
        {
            var index = (int)Index.GetValue();
            EUOInterpreter.Setvariable( "#journal", World.Player.GetJournal( index ) );
            return base.Execute();
        }
    }
    class LinesPerCycle : Stmt
    {
        public Expr Index;
        public LinesPerCycle( Expr index )
        {
            Index = index;

        }
    }
    class IgnoreItemStmt : Stmt
    {
        public Expr Index;
        public IgnoreItemStmt( Expr index )
        {
            Index = index;

        }
        public override bool Execute()
        {
            var val = Index.GetValue().ToString();
            if ( val == "reset" )
            {
                World.Player.IgnoredItems.Clear();
                World.Player.IgnoredTypes.Clear();

            }
            else if ( val.Contains( "_" ) )
            {
                var vals = val.Split( '_' );
                foreach ( var v in vals )
                    Add( v );
            }
            else
                Add( val );
            return base.Execute();
        }


        private void Add(string val )
        {
            if ( val.Length > 3 )
                World.Player.IgnoredItems.Add( Utility.EUO2StealthID( val ) );
            else
                World.Player.IgnoredTypes.Add( Utility.EUO2StealthType( val ) );
        }
    }
    class MenuStmt : Stmt
    {
        public List<Expr> Params;
        public MenuStmt( List<Expr> paras )
        {
            Params = paras;
        }
    }
    class MessageStmt : Stmt
    {
        public List<Expr> Params;
        public MessageStmt( List<Expr> paras )
        {
           
            Params = paras;
        }
        public override bool Execute()
        {

            var msg = "";
            foreach ( var p in Params )
                msg += " " + p.GetValue();

            ClientCommunication.SendToServer( new ClientUniMessage( MessageType.Regular, 55, 1, "ENU", null, msg ) );
            return base.Execute();
        }
    }
    class TargetStmt : Stmt
    {
        public List<Expr> Params;
        public TargetStmt( List<Expr> paras )
        {
            Params = paras;
        }
        public override bool Execute()
        {

            var timeout = 100;
            if(Params.Count> 0)
                timeout = Params[0].GetValueInt();
            int cnt = 0;
            while ( !EUOVars.HasTarget && cnt < timeout )
            {
                Thread.Sleep( 100 );
            }
            return base.Execute();
        }
    }
    class ClickStmt : Stmt
    {
        public List<Expr> Params;
        public ClickStmt( List<Expr> paras )
        {
            Params = paras;
        }
        public override bool Execute()
        {
            return base.Execute();
        }
    }
    class MoveStmt : Stmt
    {
        public List<Expr> Params;
        public MoveStmt( List<Expr> paras )
        {
            Params = paras;
        }
        public override bool Execute()
        {
            var x = Params[0].GetValueInt();
            var y = Params[1].GetValueInt();
            int tolerance = 0;
            if(Params.Count>= 3)
                tolerance = Params[2].GetValueInt();
            int timeout = 0;
            if ( Params.Count >= 4 )
                timeout = Params[3].GetValueInt();
            while(Utility.Distance(World.Player.Position,new Point2D(x,y)) > tolerance )
            {
                ClientCommunication.RequestMove( (int)World.Player.Position.GetDirectionTo( new Point2D( x, y ) ) );
                Thread.Sleep( 400 );
            }

            return base.Execute();                
        }
    }
    class EventStmt : Stmt
    {
        public string EventType;
        public List<Expr> Params;
        public EventStmt( string eventType, List<Expr> paras )
        {
            EventType = eventType;
            Params = paras;
        }

        public override bool Execute()
        {
            if ( DEBUG )
                Console.WriteLine( $"Executing Event: {EventType} " );
            switch ( EventType )
            {
                case "macro":
                    switch ( (int)Params[0].GetValue() )
                    {
                        case 13:
                            ClientCommunication.SendToServer( new UseSkill( (int)Params[1].GetValue() ) );
                            break;
                        case 15:
                            World.Player.LastSpell = (int)Params[1].GetValue();
                            ClientCommunication.CastSpell( (int)Params[1].GetValue() );
                            break;
                        case 16:
                            ClientCommunication.CastSpell( World.Player.LastSpell );
                            break;
                        case 17:
                            var obj = Utility.EUO2StealthID( EUOInterpreter.GetVariable<string>( "#lobjectid" ) );
                            ClientCommunication.SendToServer( new DoubleClick( obj ) );

                            break;
                        case 22:
                            var targ = Utility.EUO2StealthID( EUOInterpreter.GetVariable<string>( "#ltargetid" ) );
                            EUOVars.SendTargetLast();
                            break;
                        case 23:
                            Targeting.Target( World.Player.Serial );
                            break;

                    }
                    break;
                case "gump":
                    {
                        switch ( Params[0].GetValue().ToString() )
                        {
                            case "wait":
                                {
                                    int timeout = 10000;
                                    if ( Params.Count > 1 )
                                        timeout = Params[1].GetValueInt();
                                    int max = timeout / 250;
                                    int cnt = 0;
                                    while ( !World.Player.HasGump && cnt++ < max )
                                        Thread.Sleep( 250 );
                                    if ( !World.Player.HasGump )
                                        World.Player?.SendMessage( "Gump not found" );
                                }
                                break;
                            case "last":
                                if ( World.Player?.HasGump == true )
                                    World.Player?.LastGumpResponseAction?.Perform();
                                else
                                    World.Player?.SendMessage( "Gump not found" );
                                break;
                            case "button":
                                int button = Params[1].GetValueInt();
                                World.Player.LastGumpResponseAction = new GumpResponseAction( button, new int[] { }, new GumpTextEntry[] { } );
                                if ( World.Player?.HasGump == true )
                                    World.Player?.LastGumpResponseAction?.Perform();
                                else
                                    World.Player?.SendMessage( "Gump not found" );
                                break;
                        }

                    }
                    break;
                case "contextmenu":
                    uint serial = Utility.EUO2StealthID( Params[1].GetValue().ToString() );
                    ushort index = (ushort)Params[2].GetValueInt();
                    ClientCommunication.SendToServer( new ContextMenuRequest( serial ) );
                    ClientCommunication.SendToServer( new ContextMenuResponse( serial, index ) );
                    break;
            }
            return base.Execute();
        }
    }
    class ExEventStmt : Stmt
    {
        public string EventType;
        public List<Expr> Params;
        public ExEventStmt( string eventType, List<Expr> paras )
        {
            EventType = eventType;
            Params = paras;
        }
    }
    class Tile : Stmt
    {
        public string Command;
        public List<Expr> Params;
        public Tile( string command, List<Expr> paras )
        {
            Command = command;
            Params = paras;
        }
    }

    class Goto : Stmt
    {
        public string Name;
        public Goto( string name )
        {
            Name = name;
        }
    }
    class Continue : Stmt
    {

        public Continue()
        {

        }
    }
    class Break : Stmt
    {

        public Break(   )
        {

        }
    }
    class Label : Stmt
    {
        public string Name;
        public Label(string name)
        {
            Name = name;
        }
    }
    class Func : Block
    {
        public string ident;
        public List<string> vars;

        public Func(string i, List<string> v)
        {
            ident = i;
            vars = v;
        }
    }

    class WhileBlock : BaseLoopBlock
    {
        public Expr Expr;



        public WhileBlock( Expr lexpr )
        {
            Expr = lexpr;

        }
        public override bool Execute()
        {
            return base.Execute();
        }
    }
    class ForBlock : BaseLoopBlock
    {
        public Expr From;
        public Expr Var;
        public Expr To;

        private int Index;
        public ForBlock( Expr var, Expr from, Expr to )
        {
            From = from;
            Var = var;
            To = to;
        }
        public override bool Execute()
        {
            foreach ( var s in statements )
            {
                s.Execute();

            }

            return base.Execute();
        }
    }

    class IfBlock : Block
    {
        public Expr Expr;
        

        public IfBlock(Expr expr)
        {
            Expr = expr;

        }
        public override bool Execute()
        {
            if(Expr is MathExpr ma)
            {
                switch(ma.op)
                {
                    case Symbol.Equal:
                        {
                            if ( ma.leftExpr.GetValue().Equals( ma.rightExpr.GetValue() ) )
                                return true;
                            return false;
                        }
                    case Symbol.NotEqual:
                        {
                            if ( ma.leftExpr.GetValue().Equals( ma.rightExpr.GetValue() ) )
                                return false;
                            return true;
                        }
                        break;
                }
            }
            return base.Execute();
        }
    }

    class ElseIfBlock : Block
    {
        public Expr leftExpr;
        public Symbol op;
        public Expr rightExpr;

        public ElseIfBlock(Expr lexpr, Symbol o, Expr rexpr)
        {
            leftExpr = lexpr;
            op = o;
            rightExpr = rexpr;
        }
    }

    class ElseBlock : Block { }

    class EndIf : Block { }

    class RepeatBlock : Block { }

    class Assign : Stmt
    {
        public Expr ident;
        public Expr value;

        public Assign( Expr i, Expr v)
        {
            ident = i;
            value = v;
        }
        public override bool Execute()
        {
            string varName = "";
            if ( ident is Ident i )
            {
                varName = i.value.ToLowerInvariant();
                EUOInterpreter.Setvariable( varName, value.GetValue() );
            }
            else
            {
                varName = ident.GetValue().ToString().ToLowerInvariant();
                EUOInterpreter.Setvariable( varName, value );
            }
            return base.Execute();
        }
    }

    class Call : Stmt
    {
        public string ident;
        public List<Expr> args;

        public Call(string i, List<Expr> a)
        {
            ident = i;
            args = a;
        }
    }

    class Return : Stmt
    {
        public Expr expr;

        public Return(Expr e)
        {
            expr = e;
        }
    }

    class IntLiteral : Expr
    {
        public int value;

        public IntLiteral(int v)
        {
            value = v;
        }

        public override object GetValue()
        {
            return value;
        }
    }

    class StringLiteral : Expr
    {
        public string value;

        public StringLiteral(string v)
        {
            value = v;
        }
        public override object GetValue()
        {
            return value;
        }
    }

    class Ident : Expr
    {
        public string value;

        public Ident(string v)
        {
            value = v;
        }

        public override object GetValue()
        {
            return EUOInterpreter.GetVariable<string>(value.ToLowerInvariant());
        }
    }

    class MathExpr : Expr
    {
        public Expr leftExpr;
        public Symbol op;
        public Expr rightExpr;

        public MathExpr(Expr lexpr, Symbol o, Expr rexpr)
        {
            leftExpr = lexpr;
            op = o;
            rightExpr = rexpr;
        }
        public override object GetValue()
        {
            switch (op)
            {
                case Symbol.add:
                    if(leftExpr is IntLiteral || rightExpr is IntLiteral)
                        return (int)leftExpr.GetValueInt() + (int)rightExpr.GetValueInt();
                    else
                        return (string)leftExpr.GetValue() + (string)rightExpr.GetValue();
                case Symbol.sub:
                    if(leftExpr is IntLiteral || rightExpr is IntLiteral)
                        return (int)leftExpr.GetValueInt() - (int)rightExpr.GetValueInt();
                    break;
                case Symbol.mul:
                    if(leftExpr is IntLiteral || rightExpr is IntLiteral)
                        return (int)leftExpr.GetValueInt() - (int)rightExpr.GetValueInt();
                    break;
                case Symbol.div:
                    if(leftExpr is IntLiteral || rightExpr is IntLiteral)
                        return (int)leftExpr.GetValueInt() - (int)rightExpr.GetValueInt();
                    break;
                case Symbol.Concat:
                    return leftExpr.GetValue().ToString() + rightExpr.GetValue().ToString();
                case Symbol.In:
                    var val = leftExpr.GetValue().ToString().Replace('_',' ');
                    return rightExpr.GetValue().ToString().Contains( val );

            }
         throw new NotSupportedException();   
        }
    }

    class ParanExpr : Expr
    {
        public Expr value;

        public ParanExpr(Expr v)
        {
            value = v;
        }

        public override object GetValue()
        {
            return value.GetValue();
        }
    }

    class CallExpr : Expr
    {
        public string ident;
        public List<Expr> args;

        public CallExpr(string i, List<Expr> a)
        {
            ident = i;
            args = a;
        }
        public override object GetValue()
        {
            throw new NotImplementedException();
        }
    }

    enum Symbol
    {
        add = 0,
        sub = 1,
        mul = 2,
        div = 3,
        doubleEqual = 5,
        leftParan = 7,
        rightParan = 8,
        leftBrace = 9,
        rightbrace = 10,
        Concat = 11,
        Period = 12,
        And = 13,
        MoreOrEqual = 14,
        LessOrEqual = 15,
        More = 16,
        Equal = 17,
        NotEqual = 18,
        Less = 19,
        Or = 20,
        Abs = 21,
        In = 22
    }
}
