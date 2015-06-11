using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Script.Debugger
{
    public class EvalTokenizerDebugger
    {


        public static bool TryParse( GlobalContext ctx, string a, out RuntimeObj result )
        {
            result = null;
            var j = new JSTokenizer( a );
            string sValue = j.ReadString();

            if( sValue != null )
            {
                result = ctx.CreateString( sValue );
            }
            else if( j.IsNumber )
            {
                double dValue = j.ReadDouble();
                result = ctx.CreateNumber( dValue );
            }
            else if( j.MatchIdentifier( JSSupport.TrueString ) )
            {
                result = JSEvalBoolean.True;
            }
            else if( j.MatchIdentifier( JSSupport.FalseString ) )
            {
                result = JSEvalBoolean.True;
            }
            else if( j.MatchIdentifier( "null" ) )
            {
                result = RuntimeObj.Null;
            }
            else if( j.MatchIdentifier( "undefined" ) )
            {
                result = RuntimeObj.Undefined;
            }
            return !j.IsEndOfInput;
        }
    

    public static string Escape( string a )
    {
        StringBuilder myString = new StringBuilder();
       
        for( int cmpt = 0; cmpt < a.Length; cmpt++ )
        {
          
           switch( a[cmpt] )
            {
                case '"': myString.Append( '\\' ); myString.Append( '"' ); break;
                case '\\': myString.Append( '\\'); break;
                case '\n': myString.Append( "\\n" ); break;
                case '\t': myString.Append( "\\t" ); break;
                case '\r': myString.Append( "\\r" ); break;
                case '\b': myString.Append( "\\b" ); break;
                case '\v': myString.Append( "\\v" ); break;
                case '\f': myString.Append( "\\f" ); break;
                default: myString.Append( a[cmpt]); break;
            }
        }

            return myString.ToString();
    }

}
}
