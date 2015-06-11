using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
        ScriptEngine.Result _res;
        public Watch(){

            
            string script = @"let a,b,c;
                               a=5;
                               b='test';
                                a = true;
                               c= a+b 
                               let d = 4";

            Expr exp = ExprAnalyser.AnalyseString( script );

            BreakableVisitor bkv = new BreakableVisitor();
            bkv.VisitExpr( exp );
            _engine.Breakpoints.AddBreakpoint( bkv.BreakableExprs[1] );
            _engine.Breakpoints.AddBreakpoint( bkv.BreakableExprs[4] );
            _res = _engine.Execute( exp );
             if( !_res.CanContinue )
             {
                 _res.Dispose();
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
                VarValue = EvalTokenizerDebugger.Escape(Test.Object.ToString()),
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
           
            error = EvalTokenizerDebugger.TryParse(_engine.Context, a, out result);
            if( result == null && error == true )
            {
                MessageBoxResult popUp = MessageBox.Show( "Invalid Input" );
                tb.Text = obj2.VarValue;
               
                 
            } else if (result != null)
            {
               
                O.Value = result;
                obj2.Refresh(_engine);

            }
        }           
    }

    private void _continue_Click( object sender, RoutedEventArgs e )
    {
        
        if( !_res.CanContinue )
        {
            _res.Dispose();
        }
        else
        {
            _res.Continue();
        }
        foreach( VarData v in _varsCollection )
        {
            v.Refresh( _engine );
        }        
    }

}  

 

  public class VarData : INotifyPropertyChanged
  {
      string _name;
      string _type;
      string _value;


      public string VarName
      {
          get { return _name; }
          set { _name = value; }
      }
    public string VarType { 
        get{return _type;}
        set{ _type = value;
        RaisePropertyChanged( "VarType" );           
            }
    }
    public string VarValue { 
        get{return _value;}
        set{_value = value;
            RaisePropertyChanged("VarValue");
        }
    }
    public void Refresh(ScriptEngineDebugger engine)
    {
        var Test = engine.ScopeManager.FindByName( this.VarName );

        this.VarValue = EvalTokenizerDebugger.Escape(Test.Object.ToString());
        this.VarType = Test.Object.Type.ToString();
    }

    #region INotifyPropertyChanged Members
    protected void RaisePropertyChanged( [CallerMemberName] string name = null )
    {
        if( PropertyChanged != null )
        {
            PropertyChanged( this, new PropertyChangedEventArgs( name ) );
        }

    }
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
       
}
