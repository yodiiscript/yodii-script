#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Script.Tests\StatementTests.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Yodii.Script;

namespace Yodii.Script.Tests
{
    [TestFixture]
    public class StatementTests
    {
        [Test]
        public void evaluating_basic_numbers_expressions()
        {
            RuntimeObj o;
            {
                o = ScriptEngine.Evaluate( "6;7+3" );
                Assert.IsInstanceOf<JSEvalNumber>( o );
                Assert.That( o.ToDouble(), Is.EqualTo( 10 ) );
            }
            {
                o = ScriptEngine.Evaluate( "6;7+3;typeof 6 == 'number' ? 2173 : 3712" );
                Assert.IsInstanceOf<JSEvalNumber>( o );
                Assert.That( o.ToDouble(), Is.EqualTo( 2173 ) );
            }
        }

        [Test]
        public void local_variables_definition_and_assignments()
        {
            string s = @"var i;
                         var j;
                         i = 37;
                         j = i*100+12;";
            RuntimeObj o = ScriptEngine.Evaluate( s );
            Assert.IsInstanceOf<JSEvalNumber>( o );
            Assert.That( o.ToDouble(), Is.EqualTo( 3712 ) );
        }

        [Test]
        public void declaring_a_local_variables_do_not_evaluate_to_undefined_like_in_javascript()
        {
            string s = @"var i = 37;
                         var j = i*100+12;";
            RuntimeObj o = ScriptEngine.Evaluate( s );
            Assert.IsInstanceOf<JSEvalNumber>( o );
            Assert.AreEqual( o.ToString(), "3712" );
        }

        [Test]
        public void variables_evaluate_to_RefRuntimeObj_objects()
        {
            string s = @"var i = 37;
                         var j = i*100+12;
                         j;";
            RuntimeObj o = ScriptEngine.Evaluate( s );
            Assert.IsInstanceOf<RefRuntimeObj>( o );
            Assert.That( o.ToDouble(), Is.EqualTo( 3712 ) );
        }


        [Test]
        public void number_assignment_operators_are_supported()
        {
            string s = @"   var i = 0;
                            var bug = '';

                            i += 0+1; i *= 2*1; i <<= 1<<0; i -= 7-6;
                            if( i !== (((0+1)*(2*1))<<(1<<0))-(7-6) ) bug = 'Bug in +, *, << or -';

                            // i = 3
                            i += 4; i &= 2 | 1; 
                            if( i !== (7&2|1) ) bug = 'Bug in &';

                            // i = 3
                            i |= 7+1;
                            if( i !== 11 ) bug = 'Bug in |';

                            // i = 11
                            i >>= 1+1;
                            if( i !== 2 ) bug = 'Bug in >>';

                            // i = 2
                            i ^= 1+8;
                            if( i !== (2^(1+8)) ) bug = 'Bug in ^';

                            // i = 11
                            i ^= -3712;
                            if( i !== (11^-3712) ) bug = 'Bug in ~';

                            // i = -3701
                            i >>>= 2;
                            if( i !== (-3701>>>2) || i !== 1073740898 ) bug = 'Bug in >>>';

                            // i = 1073740898;
                            i &= 2|4|32|512|4096;
                            if( i !== 1073740898 & (2|4|32|512|4096) ) bug = 'Bug in &';

                            // i = 4130
                            i %= -(1+5+3);
                            if( i !== (4130%-(1+5+3)) || i !== 8 ) bug = 'Bug in %';
                            
                            i = 8;
                            i /= 3.52;
                            if( i !== 8/3.52 ) bug = 'Bug in /';
                        
                            bug.toString();
";
            RuntimeObj o = ScriptEngine.Evaluate( s );
            Assert.IsInstanceOf<JSEvalString>( o );
            Assert.That( o.ToString(), Is.EqualTo( String.Empty ) );
        }

        [Test]
        public void simple_if_block()
        {
            string s = @"var i = 37;
                         var j;
                         if( i == 37 ) 
                         {
                            j = 3712;
                            i += j;
                         }
                         // i = 0: the 0 value is the result;
                         if( j > 3000 ) i = 0;";
            RuntimeObj o = ScriptEngine.Evaluate( s );
            Assert.IsInstanceOf<JSEvalNumber>( o );
            Assert.That( o.ToDouble(), Is.EqualTo( 0 ) );
        }

        [Test]
        public void comparing_to_undefined_keyword_works()
        {
            string s = @"var ResultAsRefRuntimeObject = 8;
                         var X;
                         if( X === undefined ) ResultAsRefRuntimeObject;";
            RuntimeObj o = ScriptEngine.Evaluate( s );
            Assert.IsInstanceOf<RefRuntimeObj>( o );
            Assert.That( o.ToDouble(), Is.EqualTo( 8 ) );
        }

        [Test]
        public void post_incrementation_works()
        {
            string s = @"var i = 0;
                         if( i++ == 0 && i++ == 1 && i++ == 2 ) i;";
            RuntimeObj o = ScriptEngine.Evaluate( s );
            Assert.IsInstanceOf<RefRuntimeObj>( o );
            Assert.That( o.ToDouble(), Is.EqualTo( 3 ) );
        }

        [Test]
        public void pre_incrementation_works()
        {
            string s = @"var i = 0;
                         if( ++i == 1 && ++i == 2 && ++i == 3 && ++i == 4 ) i;";
            RuntimeObj o = ScriptEngine.Evaluate( s );
            Assert.IsInstanceOf<RefRuntimeObj>( o );
            Assert.That( o.ToDouble(), Is.EqualTo( 4 ) );
        }


        [Test]
        public void while_loop_works()
        {
            string s = @"var i = 0;
                         while( i < 10 ) i++;
                         i;";
            RuntimeObj o = ScriptEngine.Evaluate( s );
            Assert.IsInstanceOf<RefRuntimeObj>( o );
            Assert.That( o.ToDouble(), Is.EqualTo( 10 ) );
        }
        [Test]
        public void while_loop_with_empty_block_works()
        {
            string s = @"var i = 0;
                         while( i++ < 10 );
                         i;";
            RuntimeObj o = ScriptEngine.Evaluate( s );
            Assert.IsInstanceOf<RefRuntimeObj>( o );
            Assert.That( o.ToDouble(), Is.EqualTo( 11 ) );
        }

        [Test]
        public void while_loop_with_block_works()
        {
            string s = @"var i = 0;
                         var j = 0;
                         while( i < 10 ) { 
                            i++;
                            if( i%2 == 0 ) j += 10;
                         }
                         j;";
            RuntimeObj o = ScriptEngine.Evaluate( s );
            Assert.IsInstanceOf<RefRuntimeObj>( o );
            Assert.That( o.ToDouble(), Is.EqualTo( 50 ) );
        }

        [Test]
        public void do_while_loop_with_block_works()
        {
            string s = @"var i = 0;
                         var j = 0;
                         do
                         { 
                            i++;
                            if( i%2 == 0 ) j += 10;
                         }
                         while( i < 10 );
                         j;";
            RuntimeObj o = ScriptEngine.Evaluate( s );
            Assert.IsInstanceOf<RefRuntimeObj>( o );
            Assert.That( o.ToDouble(), Is.EqualTo( 50 ) );
        }

        [Test]
        public void do_while_loop_expects_a_block()
        {
            string s = @"var i = 0;
                         do i++; while( i < 10 );
                         i;";
            RuntimeObj o = ScriptEngine.Evaluate( s );
            Assert.IsInstanceOf<RuntimeError>( o );
        }

        [Test]
        public void while_loop_support_break_statement()
        {
            string s = @"
                        var i = 0, j = '';
                        while( true )
                        {
                            if( i++ >= 5 ) break;
                            j += 'a';
                        }
                        j;";
            RuntimeObj o = ScriptEngine.Evaluate( s );
            Assert.IsInstanceOf<RefRuntimeObj>( o );
            Assert.That( o.ToString(), Is.EqualTo( "aaaaa" ) );
        }

        [Test]
        public void while_loop_support_continue_statement()
        {
            string s = @"
                        var i = 0, j = '';
                        while( ++i < 10 )
                        {
                            if( i%2 == 0 ) continue;
                            j += 'a';
                        }
                        j;";
            RuntimeObj o = ScriptEngine.Evaluate( s );
            Assert.IsInstanceOf<RefRuntimeObj>( o );
            Assert.That( o.ToString(), Is.EqualTo( "aaaaa" ) );
        }

        [Test]
        public void do_while_loop_support_break_statement()
        {
            string s = @"
                        var i = 0, j = '';
                        do
                        {
                            if( i++ >= 4 ) break;
                            j += 'a';
                        }
                        while( i < 1000 );
                        j;";
            RuntimeObj o = ScriptEngine.Evaluate( s );
            Assert.IsInstanceOf<RefRuntimeObj>( o );
            Assert.That( o.ToString(), Is.EqualTo( "aaaa" ) );
        }

        [Test]
        public void multiple_variables_declaration_is_supported_and_they_can_reference_previous_ones()
        {
            string s = @"var i = 1, j = i*200+34, k = 'a string';
                         k+i+j;";
            RuntimeObj o = ScriptEngine.Evaluate( s );
            Assert.IsInstanceOf<JSEvalString>( o );
            Assert.That( o.ToString(), Is.EqualTo( "a string1234" ) );
        }

        [Test]
        public void lexical_scope_is_enough_with_curly()
        {
            string s = @"
                        var i = 0, j = 'a';
                        {
                            var i = 't'; 
                        }
                        i+j;";
            RuntimeObj o = ScriptEngine.Evaluate( s );
            Assert.IsInstanceOf<JSEvalString>( o );
            Assert.That( o.ToString(), Is.EqualTo( "0a" ) );
        }


    }
}
