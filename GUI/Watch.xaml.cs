using System.Collections.ObjectModel;
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
                VarValue = Test.Object.ToString(),
                VarType = Test.Object.Type.ToString(),
            } );
        }
    }

    private void clearButton_Click( object sender, RoutedEventArgs e )
    {
        _varsCollection.Clear();
        
    }

    private void Change_Value( object sender, RoutedEventArgs e )
    {
        
        //_engine.ScopeManager.FindByName(Watche.)
    }

    private void TextBox_KeyDown( object sender, System.Windows.Input.KeyEventArgs e )
    {
        if( e.Key == System.Windows.Input.Key.Enter )
        {
           
            TextBox tb = (TextBox)sender;
            var a = tb.Text;
            GUI.VarData obj2 = tb.DataContext as GUI.VarData;
            RefRuntimeObj O = _engine.ScopeManager.FindByName( obj2.VarName ).Object;
            O.Value = new JSEvalNumber( double.Parse(a) );
           
            
        }
    }

   
  }

  public class VarData
  {
    public string VarName { get; set; }
    public string VarType { get; set; }
    public string VarValue { get; set; }
  }
       
}
