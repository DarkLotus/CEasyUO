using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEasyUO.Scripting
{

   /* class SpracheParser
    {
        public static readonly Parser<string> Ident =
        ( from open in Parse.Chars(new[] {'%','#','!' } )
          from content in Parse.Letter.AtLeastOnce().Text()
          select content ).Token();


        public static readonly Parser<(string,string)> Assign =
                from set in Parse.String("set")
        from id in Ident
        from val in STRINGORNUM
                select ( id, val );


        public static readonly Parser<string> String = Parse.Letter.AtLeastOnce().Text().Token();

        public static readonly Parser<string> STRINGORNUM = Parse.Letter.AtLeastOnce().Text().Token().Or<string>(Parse.Number);

        public SpracheParser(string input)
        {

           // var res = Assign.Parse( input );
           // res = Assign.Parse( input );
        }
    }*/
}
