﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Script.Debugger
{
    public class BreakableVisitor : ExprVisitor
    {
        readonly List<Expr> _breakableExprs = new List<Expr>();
        public override Expr VisitExpr( Expr e )
        {
            Console.WriteLine( e.ToString() );
            if( e.IsBreakable ) _breakableExprs.Add( e );
            return base.VisitExpr( e );
        }
        public IReadOnlyList<Expr> BreakableExprs
        {
            get { return _breakableExprs; }
        }
    }
}