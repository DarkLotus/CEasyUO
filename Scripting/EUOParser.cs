using CEasyUO.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEasyUO
{
    class EUOParser
    {
        private List<Token> m_Tokens;
        private int CurrentIndex = 0;

        private Token CurrentToken => m_Tokens.Count >= CurrentIndex ? m_Tokens[CurrentIndex] : null;
        private Token NextToken => m_Tokens.Count > CurrentIndex+1 ? m_Tokens[CurrentIndex+1] : null;
        private Token LastToken => m_Tokens[CurrentIndex - 1];
        public string Script { get; internal set; }

        public EUOParser(string script )
        {
            Script = script;
            Lexer lexer = new CEasyUO.Lexer() { InputString = script };

            List<Token> tokens = new List<Token>();
            int line = 1;
            int tok = 0;
            while ( true )
            {

                Token t = lexer.GetToken();
                if ( t == null )
                {
                    break;
                }

                t.Line = line;
                if ( t.TokenName == Lexer.Tokens.NewLine )
                    line++;
                if ( t.TokenName.ToString() == "Undefined" )
                {
                    var rrr = tokens.Last();
                    throw new Exception( $"Undefined token: {t.TokenValue} " );
                }
                if ( t.TokenName.ToString() != "Whitespace" && t.TokenName.ToString() != "Undefined" )
                {
                    tokens.Add( t );
                }
                if ( t.TokenName == Lexer.Tokens.Label )
                {
                    //Labels.Add( t.TokenValue.TrimEnd( new[]{':'} ).ToLowerInvariant(), tok + 1 );
                }
                if ( t.TokenName == Lexer.Tokens.Function )
                {
                    //Subs.Add( t.TokenValue.TrimEnd( new[] { ':' } ).ToLowerInvariant(), tok + 1 );
                }
                tok++;
            }
            m_Tokens = tokens;
            m_Tokens.Add( new Token( Lexer.Tokens.NewLine, "" ) );

            m_Tokens.Add( new Token( Lexer.Tokens.EOF, "" ) );
            if ( m_Tokens == null || m_Tokens.Count <= 1 )
                throw new Exception( "Parse Error" );
            // Player = PlayerMobile.GetPlayer();
           // GenerateAST();
        }

        public List<Stmt> GenerateAST()
        {
            Block currentBlock = new Block();
            var blockstack = new Stack<Block>();
            Token tok = null;
            var tree = new List<Stmt>();
            var running = true;

            while ( running )
            {
                try
                {
                    tok = CurrentToken;
                    CurrentIndex++;
                    if ( tok == null )
                        break;
                }
                catch { }
                if ( currentBlock is IfBlock ifb )
                {
                    if ( BlockEndsAtThisLine != -1 && tok.Line > BlockEndsAtThisLine )
                    {
                       // currentBlock.AddStmt( new EndIf() );
                        Block block = currentBlock;

                        if ( blockstack.Count > 0 )
                        {
                            currentBlock = blockstack.Pop();
                            currentBlock.AddStmt( block );
                        }
                        BlockEndsAtThisLine = -1;
                    }
                    
                }
                if ( currentBlock is ForBlock || currentBlock is WhileBlock )
                {
                    if ( BlockEndsAtThisLine != -1 && tok.Line > BlockEndsAtThisLine )
                    {
                        Block block = currentBlock;

                        if ( blockstack.Count > 0 )
                        {
                            currentBlock = blockstack.Pop();
                            currentBlock.AddStmt( block );
                        }
                        BlockEndsAtThisLine = -1;
                    }
                }

                switch ( tok.TokenName )
                {
                    case Lexer.Tokens.LeftBrace:
                    case Lexer.Tokens.Comment:
                    case Lexer.Tokens.NewLine:
                        continue;
                    case Lexer.Tokens.Call:
                        currentBlock.AddStmt( ParseCall() );
                        continue;
                    case Lexer.Tokens.Tile:
                        currentBlock.AddStmt( ParseTile() );
                        continue;
                    case Lexer.Tokens.Scanjournal:
                        currentBlock.AddStmt( ParseScanJournal() );
                        continue;
                    case Lexer.Tokens.ExEvent:
                        currentBlock.AddStmt( ParseExEvent() );
                        continue;
                    case Lexer.Tokens.Target:
                        currentBlock.AddStmt( ParseTarget() );
                        continue;
                    case Lexer.Tokens.Msg:
                        currentBlock.AddStmt( ParseMessage() );
                        continue;
                    case Lexer.Tokens.Move:
                        currentBlock.AddStmt( ParseMove() );
                        continue;
                    case Lexer.Tokens.Menu:
                        currentBlock.AddStmt( ParseMenu() );
                        continue;
                    case Lexer.Tokens.Click:
                        currentBlock.AddStmt( ParseClick() );
                        continue;
                    case Lexer.Tokens.Pause:
                        currentBlock.AddStmt( new PauseStmt() { Line = tok.Line } );
                        continue;
                    case Lexer.Tokens.Continue:
                        currentBlock.AddStmt( new Continue() { Line = tok.Line } );
                        continue;
                    case Lexer.Tokens.IgnoreItem:
                        currentBlock.AddStmt( ParseIgnoreItem() );
                        continue;
                    case Lexer.Tokens.LinesPerCycle:
                        currentBlock.AddStmt( new LinesPerCycle( ParseExpr() ) { Line = tok.Line } );
                        continue;
                    case Lexer.Tokens.Wait:
                        currentBlock.AddStmt( new WaitStmt( ParseExpr() ) { Line = tok.Line } );
                        continue;
                    case Lexer.Tokens.Label:
                        currentBlock.AddStmt( new Label( tok.TokenValue.TrimEnd( new[] { ':' } ).ToLowerInvariant() ) { Line = tok.Line } );
                        CurrentIndex++;
                        continue;
                    case Lexer.Tokens.Break:
                        currentBlock.AddStmt( new Break() { Line = tok.Line } );
                        CurrentIndex++;
                        continue;
                    case Lexer.Tokens.Goto:
                        {
                            currentBlock.AddStmt( new Goto( CurrentToken.TokenValue.TrimEnd( new[] { ':' } ).ToLowerInvariant() ) { Line = tok.Line } );
                            CurrentIndex++;

                            Block block = currentBlock;

                           /* if ( blockstack.Count > 0 && block is Func )
                            {
                                currentBlock = blockstack.Pop();
                                currentBlock.AddStmt( block );
                            }*/

                        }
                        continue;
                    case Lexer.Tokens.Event:
                        currentBlock.AddStmt( ParseEvent() );
                        continue;
                    case Lexer.Tokens.FindItem:
                        currentBlock.AddStmt( ParseFindItem() );
                        continue;
                        
                }

                if ( tok.TokenName == Lexer.Tokens.Import )
                {
                    //  Program.imports.Add( ParseImport() );
                }

                else if ( tok.TokenName == Lexer.Tokens.Function )
                {
                    Block block = currentBlock;
                    if ( blockstack.Count > 0 && block is Func )
                    {
                        currentBlock = blockstack.Pop();
                        currentBlock.AddStmt( block );
                    }

                    Func func = ParseFunc();
                   
                    if ( currentBlock != null )
                    {
                        blockstack.Push( currentBlock );
                        currentBlock = func;
                    }
                }
                else if ( tok.TokenName == Lexer.Tokens.If )
                {
                    IfBlock ifblock = ParseIf();

                    if ( currentBlock != null )
                    {
                        blockstack.Push( currentBlock );
                        currentBlock = ifblock;
                    }
                }
                /* else if ( tok.TokenName == Lexer.Tokens.ElseIf )
                 {
                     ElseIfBlock elseifblock = ParseElseIf();

                     if ( currentBlock != null )
                     {
                         blockstack.Push( currentBlock );
                         currentBlock = elseifblock;
                     }
                 }*/
                else if ( tok.TokenName == Lexer.Tokens.Else )
                {
                    if ( currentBlock != null )
                    {
                        blockstack.Push( currentBlock );
                        currentBlock = new ElseBlock() { Line = tok.Line };
                    }
                }
                else if ( tok.TokenName == Lexer.Tokens.For )
                {
                        blockstack.Push( currentBlock );
                    currentBlock = ParseFor();
                }
                else if ( tok.TokenName == Lexer.Tokens.While )
                {
                    blockstack.Push( currentBlock );
                    currentBlock = ParseWhile();
                }
                else if ( tok.TokenName == Lexer.Tokens.Repeat )
                {
                    if ( currentBlock != null )
                    {
                        blockstack.Push( currentBlock );
                        currentBlock = new RepeatBlock() { Line = tok.Line };
                    }
                }
                else if ( tok.TokenName == Lexer.Tokens.Set )
                {
                    Assign a = ParseAssign();
                    currentBlock.AddStmt( a );
                }
                /*else if ( tok.TokenName == Lexer.Tokens.Ident )
                {
                    if ( tokens.Peek().TokenName == Lexer.Tokens.Equal )
                    {
                        tokens.pos--;
                        Assign a = ParseAssign();
                        currentBlock.AddStmt( a );
                    }
                    else if ( tokens.Peek().TokenName == Lexer.Tokens.LeftParan )
                    {
                        tokens.pos--;
                        Call c = ParseCall();
                        currentBlock.AddStmt( c );
                    }
                }*/
                else if ( tok.TokenName == Lexer.Tokens.Return )
                {
                    Return r = ParseReturn();
                    currentBlock.AddStmt( r );
                    Block block = currentBlock;

                    if ( blockstack.Count > 0 && block is Func )
                    {
                        currentBlock = blockstack.Pop();
                        currentBlock.AddStmt( block );
                    }
                }
                else if ( tok.TokenName == Lexer.Tokens.RightBrace )
                {
                    if ( currentBlock is Func )
                    {
                        currentBlock.AddStmt( new Return( null ) );
                        //tree.Add( currentBlock );
                        //currentBlock = null;
                        Block block = currentBlock;

                        if ( blockstack.Count > 0 )
                        {
                            currentBlock = blockstack.Pop();
                            currentBlock.AddStmt( block );
                        }
                    }
                    else if ( currentBlock is IfBlock || currentBlock is ElseIfBlock || currentBlock is ElseBlock )
                    {
                        //currentBlock.AddStmt( new EndIf() );
                        Block block = currentBlock;

                        if ( blockstack.Count > 0 )
                        {
                            currentBlock = blockstack.Pop();
                            currentBlock.AddStmt( block );
                        }
                    }
                    else if ( currentBlock is RepeatBlock )
                    {
                        Block block = currentBlock;

                        if ( blockstack.Count > 0 )
                        {
                            currentBlock = blockstack.Pop();
                            currentBlock.AddStmt( block );
                        }
                    }
                    else if ( currentBlock is ForBlock || currentBlock is WhileBlock )
                    {
                        Block block = currentBlock;

                        if ( blockstack.Count > 0 )
                        {
                            currentBlock = blockstack.Pop();
                            currentBlock.AddStmt( block );
                        }
                    }
                }
                else if ( tok.TokenName == Lexer.Tokens.EOF )
                {
                    if ( currentBlock is Func )
                    {
                        currentBlock.AddStmt( new Return( null ) );
                        //tree.Add( currentBlock );
                        //currentBlock = null;
                        Block block = currentBlock;

                        if ( blockstack.Count > 0 )
                        {
                            currentBlock = blockstack.Pop();
                            currentBlock.AddStmt( block );
                        }
                    }
                    tree.Add( currentBlock );
                    running = false;
                }
                else
                {
                    Console.WriteLine( $"Unhandled Token at Line: {tok.Line} " + tok.TokenName + " " + tok.TokenValue );
                }
            }
            return tree;
        }

        private Stmt ParseMenu()
        {
            var exps = new List<Expr>();
            while ( CurrentToken.TokenName != Lexer.Tokens.NewLine )
                exps.Add( ParseExpr() );
            return new MenuStmt( exps )
            {
                Line = CurrentToken.Line
            };
        }

        private Stmt ParseTarget()
        {
            var exps = new List<Expr>();
            while ( CurrentToken.TokenName != Lexer.Tokens.NewLine )
                exps.Add( ParseExpr() );
            return new TargetStmt( exps )
            {
                Line = CurrentToken.Line
            };
        }

        private Stmt ParseClick()
        {
            var exps = new List<Expr>();
            while ( CurrentToken.TokenName != Lexer.Tokens.NewLine )
                exps.Add( ParseExpr() );
            return new ClickStmt( exps )
            {
                Line = CurrentToken.Line
            };
        }

        private Stmt ParseMove()
        {
            var exps = new List<Expr>();
            while ( CurrentToken.TokenName != Lexer.Tokens.NewLine )
                exps.Add( ParseExpr() );
            return new MoveStmt( exps ) { Line = CurrentToken.Line };
        }

        private Stmt ParseMessage()
        {
            var exps = new List<Expr>();
            while ( CurrentToken.TokenName != Lexer.Tokens.NewLine )
                exps.Add( ParseExpr() );
            return new MessageStmt(exps ) { Line = CurrentToken.Line };
        }

        private Stmt ParseIgnoreItem()
        {
            var expr = ParseExpr();
            return new IgnoreItemStmt( expr ) { Line = CurrentToken.Line };
        }

        private Stmt ParseScanJournal()
        {
            var expr = ParseExpr();
            return new ScanJournalStmt( expr ) { Line = CurrentToken.Line };
        }

        private Block ParseWhile()
        {
            if ( CurrentToken.TokenName == Lexer.Tokens.LeftParan )
            {
                CurrentIndex++;
            }
            var lexpr = ParseExpr();

            //var op = CurrentToken;
           // CurrentIndex++;
           // var rexpr = ParseExpr();
            if ( CurrentToken.TokenName == Lexer.Tokens.RightParan )
            {
                CurrentIndex++;
            }
            BlockEndsAtThisLine = -1;
            if ( CurrentToken.TokenName == Lexer.Tokens.IntLiteral )
            {
                BlockEndsAtThisLine = int.Parse( CurrentToken.TokenValue ) + CurrentToken.Line;
            }
            else if ( CurrentToken.TokenName == Lexer.Tokens.LeftBrace || NextToken.TokenName == Lexer.Tokens.LeftBrace )
            {

            }
            else
            {
                BlockEndsAtThisLine = CurrentToken.Line + 1;
            }
            return new WhileBlock( lexpr ) { Line = CurrentToken.Line };
        }

        private Stmt ParseTile()
        {
            var command = CurrentToken.TokenValue;
            CurrentIndex++;
            var exps = new List<Expr>();
            while ( CurrentToken.TokenName != Lexer.Tokens.NewLine )
                exps.Add( ParseExpr() );
            return new Tile( command, exps ) { Line = CurrentToken.Line };
        }

        private Block ParseFor()
        {
            if ( CurrentToken.TokenName == Lexer.Tokens.LeftParan )
            {
                CurrentIndex++;
            }
            var variable = ParseExpr();

            var startIndex = ParseExpr();

            var endIndex = ParseExpr();
 
            if ( CurrentToken.TokenName == Lexer.Tokens.RightParan )
            {
                CurrentIndex++;
            }
            BlockEndsAtThisLine = -1;
            if ( CurrentToken.TokenName == Lexer.Tokens.IntLiteral )
            {
                BlockEndsAtThisLine = int.Parse( CurrentToken.TokenValue ) + CurrentToken.Line;
            }
            else if ( CurrentToken.TokenName == Lexer.Tokens.LeftBrace || NextToken.TokenName == Lexer.Tokens.LeftBrace )
            {

            }
            else
            {
                BlockEndsAtThisLine = CurrentToken.Line + 1;
            }
            return new ForBlock( variable, startIndex, endIndex ) { Line = CurrentToken.Line };

        }
        private ExEventStmt ParseExEvent()
        {
            if ( CurrentToken.TokenName != Lexer.Tokens.StringLiteral )
            {
                throw new ParseException();
            }
            var type = CurrentToken.TokenValue;
            CurrentIndex++;
            var exps = new List<Expr>();
            while ( CurrentToken.TokenName != Lexer.Tokens.NewLine )
                exps.Add( ParseExpr() );

            return new ExEventStmt( type, exps ) { Line = CurrentToken.Line };
        }
        private EventStmt ParseEvent()
        {
            if ( CurrentToken.TokenName != Lexer.Tokens.StringLiteral )
            {
                throw new ParseException();
            }
            var type = CurrentToken.TokenValue;
            CurrentIndex++;
            var exps = new List<Expr>();
            while ( CurrentToken.TokenName != Lexer.Tokens.NewLine )
                exps.Add( ParseExpr() );

            return new EventStmt( type, exps ) { Line = CurrentToken.Line };
        }

        private FindItemStmt ParseFindItem()
        {
            if ( CurrentToken.TokenName != Lexer.Tokens.StringLiteral )
            {
               // throw new Exception();
            }
            var ids = ParseExpr(); ;// CurrentToken.TokenValue;
            IntLiteral index = null;
            Expr filter = null;
            if ( CurrentToken.TokenName == Lexer.Tokens.IntLiteral )
            {
                index = new IntLiteral(int.Parse(CurrentToken.TokenValue));
                CurrentIndex++;
            }
            if(CurrentToken.TokenName == Lexer.Tokens.StringLiteral )
            {
                filter = ParseExpr();
            }

            return new FindItemStmt( ids, index, filter ) { Line = CurrentToken.Line };
          /*  foreach ( var idStr in ids )
            {
                if ( idStr.Length == 3 )
                {
                    var id = Form1.EUO2StealthType( idStr );
                    foreach ( var i in World.Items.Values )
                    {
                        if ( i.ItemID == id && ( ( container != 0 && i.ContainerID == container ) || ( ground && i.ContainerID == 0 ) ) )
                        {
                            Setvariable( "#findid", Form1.uintToEUO( i.Serial.Value ) );
                            Setvariable( "#findtype", Form1.uintToEUO( i.ItemID.Value ) );

                            Setvariable( "#findx", i.Position.X );
                            Setvariable( "#findy", i.Position.Y );
                            Setvariable( "#findz", i.Position.Z );

                        }
                    }
                    foreach ( var i in World.Mobiles.Values )
                    {
                        if ( i.Body == id )
                        {
                            Setvariable( "#findid", Form1.uintToEUO( i.Serial.Value ) );
                            Setvariable( "#findtype", Form1.uintToEUO( i.Body ) );

                            Setvariable( "#findx", i.Position.X );
                            Setvariable( "#findy", i.Position.Y );
                            Setvariable( "#findz", i.Position.Z );

                        }
                    }
                }
                else
                {
                    var id = Form1.EUO2StealthID( idStr );
                    if ( World.Items.ContainsKey( id ) )
                    {
                        Setvariable( "#findid", id );
                    }
                    else if ( World.Mobiles.ContainsKey( id ) )
                    {

                    }
                }

            }*/
        }
 



        private Call ParseCall()
        {
            var subname = "";
            //if ( CurrentToken.TokenName == Lexer.Tokens.StringLiteral )
            subname = CurrentToken.TokenValue;
            var exrps = new List<Expr>();
            CurrentIndex++;
            while ( CurrentToken.TokenName != Lexer.Tokens.NewLine )
                exrps.Add( ParseExpr() );
            return new Call( subname, exrps ) { Line = CurrentToken.Line };
        }

        private Assign ParseAssign()
        {
            var variableName = ParseExpr();
            string varName = "";
            var value = ParseExpr();
            return new Assign( variableName, value ) { Line = CurrentToken.Line };

        }

        private Return ParseReturn()
        {
            var exrps = new List<Expr>();
            if ( CurrentToken.TokenName != Lexer.Tokens.NewLine )
                return new Return( ParseExpr() ) { Line = CurrentToken.Line };
            return new Return( null ) { Line = CurrentToken.Line };
        }

        private int BlockEndsAtThisLine = -1;
        private IfBlock ParseIf()
        {
            if ( CurrentToken.TokenName == Lexer.Tokens.LeftParan )
            {
                CurrentIndex++;
            }
            var lexpr = ParseExpr();

            //var op = CurrentToken;
           // CurrentIndex++;
            //var rexpr = ParseExpr();
            if ( CurrentToken.TokenName == Lexer.Tokens.RightParan )
            {
                CurrentIndex++;
            }
            BlockEndsAtThisLine = -1;
            if ( CurrentToken.TokenName == Lexer.Tokens.IntLiteral )
            {
                BlockEndsAtThisLine = int.Parse( CurrentToken.TokenValue ) + CurrentToken.Line;
            }
            else if ( CurrentToken.TokenName == Lexer.Tokens.LeftBrace || NextToken.TokenName == Lexer.Tokens.LeftBrace )
            {

            }
            else
            {
                BlockEndsAtThisLine = CurrentToken.Line + 1;
            }
            return new IfBlock( lexpr ) { Line = CurrentToken.Line };


        }

        private Func ParseFunc()
        {
            string ident = "";
            List<string> vars = new List<string>();

            if ( CurrentToken.TokenName == Lexer.Tokens.StringLiteral )
            {
                ident = CurrentToken.TokenValue.ToString();
            }
            else
                throw new ParseException( CurrentToken );
            CurrentIndex++;
            return new Func( ident, null ) { Line = CurrentToken.Line };
        }

        Expr ParseExpr()
        {
            Expr ret = null;
            Token t = CurrentToken;

            /*if ( NextToken.TokenName == Lexer.Tokens.LeftParan )
            {
                 string ident = "";

                 if (t.TokenName == Lexer.Tokens.Ident || t.TokenName == Lexer.Tokens.BuildInIdent)
                 {
                     ident = t.TokenValue.ToString();
                 }

                 CurrentIndex++;

                 if (NextToken.TokenName == Lexer.Tokens.RightParan)
                 {
                     ret = new CallExpr(ident, new List<Expr>());
                 }
                 else
                 {
                     //ret = new CallExpr(ident, ParseCallArgs());
                 }
        }
            else*/
            if ( t.TokenName == Lexer.Tokens.IntLiteral )
            {
                if(t.TokenValue.Contains("s"))
                    ret = new IntLiteral( Convert.ToInt32( t.TokenValue.TrimEnd('s').ToString() )*1000 );
                else
                    ret = new IntLiteral( Convert.ToInt32( t.TokenValue.ToString() ) );

            }
            else if ( t.TokenName == Lexer.Tokens.StringLiteral )
            {
                StringLiteral s = new StringLiteral( t.TokenValue.ToString() );
                ret = s;
            }
            else if ( t.TokenName == Lexer.Tokens.Ident || t.TokenName == Lexer.Tokens.BuildInIdent )
            {
                string ident = t.TokenValue.ToString();

                Ident i = new Ident( ident );
                ret = i;
            }
            else if ( t.TokenName == Lexer.Tokens.LeftParan )
            {
                CurrentIndex++;
                Expr e = ParseExpr();

                if ( CurrentToken.TokenName == Lexer.Tokens.RightParan )
                {
                    //CurrentIndex++;
                }

                ParanExpr p = new ParanExpr( e );

                if ( NextToken.TokenName == Lexer.Tokens.Add )
                {
                    CurrentIndex++; CurrentIndex++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr( p, Symbol.add, expr );
                }
                else if ( NextToken.TokenName == Lexer.Tokens.Sub )
                {
                    CurrentIndex++; CurrentIndex++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr( p, Symbol.sub, expr );
                }
                else if ( NextToken.TokenName == Lexer.Tokens.Mul )
                {
                    CurrentIndex++; CurrentIndex++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr( p, Symbol.mul, expr );
                }
                else if ( NextToken.TokenName == Lexer.Tokens.Div )
                {
                    CurrentIndex++; CurrentIndex++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr( p, Symbol.div, expr );
                }
                else if ( NextToken.TokenName == Lexer.Tokens.Comma )
                {
                    CurrentIndex++; CurrentIndex++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr( p, Symbol.Concat, expr );
                }
                else if ( NextToken.TokenName == Lexer.Tokens.Period )
                {
                    CurrentIndex++; CurrentIndex++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr( p, Symbol.Period, expr );
                }
                else if ( NextToken.TokenName == Lexer.Tokens.And )
                {
                    CurrentIndex++; CurrentIndex++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr( p, Symbol.And, expr );
                }
                else if ( NextToken.TokenName == Lexer.Tokens.MoreOrEqual || NextToken.TokenName == Lexer.Tokens.MoreOrEqual2 )
                {
                    CurrentIndex++; CurrentIndex++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr( p, Symbol.MoreOrEqual, expr );
                }
                else if ( NextToken.TokenName == Lexer.Tokens.LessOrEqual || NextToken.TokenName == Lexer.Tokens.LessOrEqual2 )
                {
                    CurrentIndex++; CurrentIndex++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr( p, Symbol.LessOrEqual, expr );
                }
                else if ( NextToken.TokenName == Lexer.Tokens.Less )
                {
                    CurrentIndex++; CurrentIndex++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr( p, Symbol.Less, expr );
                }
                else if ( NextToken.TokenName == Lexer.Tokens.More )
                {
                    CurrentIndex++; CurrentIndex++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr( p, Symbol.More, expr );
                }
                else if ( NextToken.TokenName == Lexer.Tokens.Equal )
                {
                    CurrentIndex++; CurrentIndex++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr( p, Symbol.Equal, expr );
                }
                else if ( NextToken.TokenName == Lexer.Tokens.In )
                {
                    CurrentIndex++; CurrentIndex++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr( p, Symbol.In, expr );
                }
                else if ( NextToken.TokenName == Lexer.Tokens.NotEqual )
                {
                    CurrentIndex++; CurrentIndex++;
                    Expr expr = ParseExpr();
                    ret = new MathExpr( p, Symbol.NotEqual, expr );
                }
                else
                {
                    ret = p;
                }
            }

            if (NextToken != null &&( NextToken.TokenName == Lexer.Tokens.Add || NextToken.TokenName == Lexer.Tokens.Sub || NextToken.TokenName == Lexer.Tokens.Mul || NextToken.TokenName == Lexer.Tokens.Div || 
                NextToken.TokenName == Lexer.Tokens.Comma || NextToken.TokenName == Lexer.Tokens.Period || NextToken.TokenName == Lexer.Tokens.In
                 || NextToken.TokenName == Lexer.Tokens.LessOrEqual || NextToken.TokenName == Lexer.Tokens.LessOrEqual2 || NextToken.TokenName == Lexer.Tokens.MoreOrEqual || NextToken.TokenName == Lexer.Tokens.MoreOrEqual2
                 || NextToken.TokenName == Lexer.Tokens.Less || NextToken.TokenName == Lexer.Tokens.More || NextToken.TokenName == Lexer.Tokens.Equal || NextToken.TokenName == Lexer.Tokens.NotEqual ))
            {
                Expr lexpr = ret;
                Symbol op = 0;

                if ( NextToken.TokenName == Lexer.Tokens.Add )
                {
                    op = Symbol.add;
                }
                else if ( NextToken.TokenName == Lexer.Tokens.Sub )
                {
                    op = Symbol.sub;
                }
                else if ( NextToken.TokenName == Lexer.Tokens.In )
                {
                    op = Symbol.In;
                }
                else if ( NextToken.TokenName == Lexer.Tokens.Mul )
                {
                    op = Symbol.mul;
                }
                else if ( NextToken.TokenName == Lexer.Tokens.Div )
                {
                    op = Symbol.div;
                }
                else if ( NextToken.TokenName == Lexer.Tokens.Comma )
                {
                    op = Symbol.Concat;
                }
                else if ( NextToken.TokenName == Lexer.Tokens.Period )
                {
                    op = Symbol.Period;
                }
                else if ( NextToken.TokenName == Lexer.Tokens.MoreOrEqual || NextToken.TokenName == Lexer.Tokens.MoreOrEqual2)
                {
                    op = Symbol.MoreOrEqual;
                }
                else if ( NextToken.TokenName == Lexer.Tokens.LessOrEqual || NextToken.TokenName == Lexer.Tokens.LessOrEqual2 )
                {
                    op = Symbol.LessOrEqual;
                }
                else if ( NextToken.TokenName == Lexer.Tokens.Less )
                {
                    op = Symbol.Less;
                }
                else if ( NextToken.TokenName == Lexer.Tokens.More )
                {
                    op = Symbol.More;
                }
                else if ( NextToken.TokenName == Lexer.Tokens.Equal )
                {
                    op = Symbol.Equal;
                }
                else if ( NextToken.TokenName == Lexer.Tokens.NotEqual )
                {
                    op = Symbol.NotEqual;
                }
                CurrentIndex++;
                CurrentIndex++;
                Expr rexpr = ParseExpr();

                ret = new MathExpr( lexpr, op, rexpr );
                if ( CurrentToken.TokenName == Lexer.Tokens.And )
                {
                    lexpr = ret;
                    CurrentIndex++;
                    rexpr = ParseExpr();
                    ret = new MathExpr( lexpr, Symbol.And, rexpr );
                } else if ( CurrentToken.TokenName == Lexer.Tokens.Or )
                {
                    lexpr = ret;
                    CurrentIndex++;
                    rexpr = ParseExpr();
                    ret = new MathExpr( lexpr, Symbol.Or, rexpr );
                }
                else if ( CurrentToken.TokenName == Lexer.Tokens.Abs )
                {
                    lexpr = ret;
                    CurrentIndex++;
                    ret = new MathExpr( lexpr, Symbol.Abs, null );
                }
                
                return ret;
            }
            

            CurrentIndex++;
            return ret;
        }

    }
}
