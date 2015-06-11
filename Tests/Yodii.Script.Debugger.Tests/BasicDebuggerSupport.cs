using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Yodii.Script.Debugger.Tests
{
    [TestFixture]
    class BasicDebuggerSupport
    {
        [TestCase("let a;", 0)]
        [TestCase( "let a=0;",1 )]
        [TestCase( "let a,b;a=5; b=2; ", 2 )]
        [TestCase( "let a,b,c;a=5; b=2;c= a+b ", 4 )]
        public void check_if_breakpoints_count_matches(string script, int count)
        {          
            ScriptEngine engine = new ScriptEngine();

            Expr exp = ExprAnalyser.AnalyseString( script );

            BreakableVisitor bkv = new BreakableVisitor();
            bkv.VisitExpr( exp );
            Assert.That( bkv.BreakableExprs.Count, Is.EqualTo(count) );
        }
        
        [Test]
        public void add_breakpoint_inside_parsed_script()
        {
            ScriptEngine engine = new ScriptEngine();
            string script = @"let a,b,c;
                               a=5;
                               b=2;
                               c= a+b ";

            Expr exp = ExprAnalyser.AnalyseString( script );

            BreakableVisitor bkv = new BreakableVisitor();
            bkv.VisitExpr( exp );
            Assert.That( bkv.BreakableExprs.Count, Is.EqualTo(4) );
            engine.Breakpoints.AddBreakpoint( bkv.BreakableExprs[3] );
            
            using( var r2 = engine.Execute( exp ) )
            {
                int nbStep = 0;
                while( r2.Status == ScriptEngineStatus.Breakpoint )
                {
                    Assert.That( (r2.Status & ScriptEngineStatus.IsPending), Is.EqualTo( ScriptEngineStatus.IsPending ) );
                    nbStep++;
                    r2.Continue();
                }

                Assert.That( r2.Status, Is.EqualTo( ScriptEngineStatus.IsFinished ) );
                Assert.That( nbStep, Is.EqualTo( 1 ) );
            }
        }
        [Test]
        public void check_the_debuggers_components()
        {
            ScriptEngineDebugger engine = new ScriptEngineDebugger(new GlobalContext());
            string script = @"let a,b,c;
                               a=5;
                               b=2;
                               c= a+b ";

            Expr exp = ExprAnalyser.AnalyseString( script );

            BreakableVisitor bkv = new BreakableVisitor();
            bkv.VisitExpr( exp );
            Assert.That( bkv.BreakableExprs.Count, Is.EqualTo( 4 ) );
            engine.Breakpoints.AddBreakpoint( bkv.BreakableExprs[3] );

            using( var r2 = engine.Execute( exp ) )
            {
                Assert.That( engine.ScopeManager.Vars.Count, Is.EqualTo( 3 ) );

                r2.Continue();
            }
        }
        [Test]
        public void show_vars_from_closure()
        {
            ScriptEngineDebugger engine = new ScriptEngineDebugger(new GlobalContext());
            string script = @"let a = 0;
                              let b = 1;
                              function testfunc(){
                                let b = 2;
                                a = 'test';
                                a = 5;
                               }
                              testfunc();
                              let c = 0;
";

            Expr exp = ExprAnalyser.AnalyseString( script );

            BreakableVisitor bkv = new BreakableVisitor();
            bkv.VisitExpr( exp );
            engine.Breakpoints.AddBreakpoint( bkv.BreakableExprs[5] );
            
            using( var r2 = engine.Execute( exp ) )
            {
                Assert.That( engine.ScopeManager.FindByName( "a" ).Object.ToString(), Is.EqualTo( "test" ) );
                Assert.That( engine.ScopeManager.FindByName( "b" ).Object.ToDouble(), Is.EqualTo( 2.0 ) );
                r2.Continue();
            }
            engine.Breakpoints.RemoveBreakpoint( bkv.BreakableExprs[5] );
            engine.Breakpoints.AddBreakpoint( bkv.BreakableExprs[7] );
            using( var r2 = engine.Execute( exp ) )
            {
                Assert.That( engine.ScopeManager.FindByName( "a" ).Object.ToDouble(), Is.EqualTo( 5.0 ) );
                Assert.That( engine.ScopeManager.FindByName( "b" ).Object.ToDouble(), Is.EqualTo( 1.0 ) );
                r2.Continue();
            }
        }

        struct RuntimeObjNull {}

        [TestCase( "toto", null )]
        [TestCase( "4 + toto", typeof( SyntaxErrorExpr ) )]
        [TestCase( "toto + 4", typeof(SyntaxErrorExpr) )]
        [TestCase( "3 + 4", typeof( SyntaxErrorExpr ) )]
        [TestCase( "This is a string", null )]
        [TestCase( "a\"\"b", null )]
        [TestCase( "null", typeof(RuntimeObjNull) )]
        [TestCase( "\r 7", 7.0 )]
        [TestCase( @"""A""", "A" )]
        [TestCase( @"""A\""B""", "A\"B" )]
        [TestCase( "\"A\\r\"", "A\r" )]
        public void check_if_the_input_and_output_string_are_the_same( string inputString, object expected  )
        {
            GlobalContext ctx = new GlobalContext();
            RuntimeObj result;
             bool error = EvalTokenizerDebugger.TryParse( ctx, inputString, out result );
                
            if( expected == null )  Assert.That( result == null && error );
            else 
            {
                if( expected is string )
                {
                    Assert.That( result.Type, Is.EqualTo( RuntimeObj.TypeString ) );
                    Assert.That( result.ToString(), Is.EqualTo( expected ) );
                }
                else if( expected is double )
                {
                    Assert.That( result.Type, Is.EqualTo( RuntimeObj.TypeNumber ) );
                    Assert.That( result.ToDouble(), Is.EqualTo( expected ) );
                }
                else if( expected is Boolean )
                {
                    Assert.That( result.Type, Is.EqualTo( RuntimeObj.TypeBoolean ) );
                    Assert.That( result.ToBoolean(), Is.EqualTo( expected ) );
                }
                else if( (Type)expected == typeof( RuntimeObjNull ) )
                {
                    Assert.That( result, Is.SameAs( RuntimeObj.Null ) );
                }
                else if( (Type)expected == typeof( SyntaxErrorExpr ) )
                {
                    Assert.That( error == true);
                }
                //

            }
        }

        /*[Test]
        public void testbenoit()
        {
           ScriptEngineDebugger _engine = new ScriptEngineDebugger( new GlobalContext() );

            string script = @"let a,b,c;
                               a=5;
                               b='test';
                               c= a+b 
                               let d = 4";

            string txt = "\"texteeasy\"";
            Expr exp = ExprAnalyser.AnalyseString( script );

            BreakableVisitor bkv = new BreakableVisitor();
            bkv.VisitExpr( exp );
            _engine.Breakpoints.AddBreakpoint( bkv.BreakableExprs[4] );
            using( var r2 = _engine.Execute( exp ) )
            {
                RefRuntimeObj O = _engine.ScopeManager.FindByName( "a" ).Object;
                RuntimeObj result;
                bool error;
                result = EvalTokenizerDebugger.TryParse( txt, out error );
                if( result == null && error == true )
                {
                    Console.WriteLine( "zob" );
                }
                else if( result != null )
                {
                    O.Value = result;
                }
                Assert.That( O.Type == RefRuntimeObj.TypeString );
                Assert.That( txt == EvalTokenizerDebugger.TryEscape( O.Value.ToString() ) );
            }
        }*/
    }
}
