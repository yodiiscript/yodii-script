using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Yodii.Script;
using Yodii.Script.Debugger;


namespace GUI
{
    /// <summary>
    /// Interaction logic for UserControlGUI.xaml
    /// </summary>
    public partial class Watch : UserControl
    {
        ObservableCollection<VarData> _varsCollection = new ObservableCollection<VarData>();
        ScriptEngineDebugger _engine = new ScriptEngineDebugger( new GlobalContext() );

        public Watch(){

            
            string script = @"let a,b,c;
                               a=5;
                               b='test';
                               c= a+b 
                               let d = 4";

            Expr exp = ExprAnalyser.AnalyseString( script );

            BreakableVisitor bkv = new BreakableVisitor();
            bkv.VisitExpr( exp );
            _engine.Breakpoints.AddBreakpoint( bkv.BreakableExprs[4] );
            using( var r2 = _engine.Execute( exp ) )
            {
               // r2.Continue();
}


        
      InitializeComponent();
            
            
      
    }

    public ObservableCollection<VarData> VarsCollection
    { get { return _varsCollection; } }

   

    private void add_Click( object sender, RoutedEventArgs e )
    {
        var Test = _engine.ScopeManager.FindByName( addVars.Text );
        if( Test == null )
        {
            _varsCollection.Add( new VarData
            {
                VarName = addVars.Text,
                VarValue = "var doesn't exist",
                VarType = "var doesn't exist",
            } );
        }
        else
        {
            _varsCollection.Add( new VarData
            {
                VarName = addVars.Text,
                VarValue = TryEscape(Test.Object.ToString()),
                VarType = Test.Object.Type.ToString(),
            } );
        }
    }

    private void clearButton_Click( object sender, RoutedEventArgs e )
    {
        _varsCollection.Clear();
        
    }

    

    private void TextBox_KeyDown( object sender, System.Windows.Input.KeyEventArgs e )
    {
        if( e.Key == System.Windows.Input.Key.Enter )
        {
            
            TextBox tb = (TextBox)sender;
            var a = tb.Text;




            GUI.VarData obj2 = tb.DataContext as GUI.VarData;
            RefRuntimeObj O = _engine.ScopeManager.FindByName( obj2.VarName ).Object;
           
            RuntimeObj result;
            bool error;
            result = TryParse(a, out error);
            if( result == null && error == true )
            {
                MessageBoxResult popUp = MessageBox.Show( "Invalid Input" );
                tb.Text = obj2.VarValue;
               
                 
            } else if (result != null)
            {
                O.Value = result;
            }

           
            
           
          

        }

        

    }

    private string TryEscape( string a )
    {
        StringBuilder MyString = new StringBuilder();
       

        for( int cmpt = 0; cmpt < a.Length; cmpt++ )
        {

           
           switch( a[cmpt] )
            {
                case '"': MyString.Append( '\\' ); MyString.Append( '"' ); break;
                case '\\': MyString.Append( '\\'); break;
                case '\n': MyString.Append( "\\n" ); break;
                case '\t': MyString.Append( "\\t" ); break;
                case '\r': MyString.Append( "\\r" ); break;
                case '\b': MyString.Append( "\\b" ); break;
                case '\v': MyString.Append( "\\v" ); break;
                case '\f': MyString.Append( "\\f" ); break;
                default: MyString.Append( a[cmpt]); break;
            }
        }

            return MyString.ToString();
    }


    private RuntimeObj TryParse( string a, out bool hasError )
    {
        hasError = false;
        RuntimeObj result;
        var j = new JSTokenizer( a );
        string sValue = j.ReadString();
        if( sValue != null )
        {
            result = _engine.Context.CreateString( sValue );
        }
        else if( j.IsNumber )
        {
            double dValue = j.ReadDouble();
            result = _engine.Context.CreateNumber( dValue );
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
        else
        {
            return null;
        }
        if( j.IsEndOfInput ) return result;
        hasError = true;
        return null;
    }
  }  

  public class VarData
  {
    public string VarName { get; set; }
    public string VarType { get; set; }
    public string VarValue { get; set; }
  }
       
}
