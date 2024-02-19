using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gaooo
{
    internal class GaoooExpression
    {
        private GaoooSystem _sys;

        public GaoooExpression(GaoooSystem sys)
        {
            _sys = sys;
        }

        private Tuple<object?, object?> AutoConvert(GaoooValue lhsVal, GaoooValue rhsVal)
        {
            object? lhs;
            object? rhs;

            var lhsType = lhsVal.GetType();
            var rhsType = rhsVal.GetType();

            if (lhsType == typeof(GaoooValueString) ||
                rhsType == typeof(GaoooValueString))
            {
                lhs = lhsVal.ToObjectString();
                rhs = rhsVal.ToObjectString();
            }
            else if (lhsType == typeof(double) ||
                     rhsType == typeof(double))
            {
                lhs = Convert.ToDouble(lhsVal.ToObject());
                rhs = Convert.ToDouble(rhsVal.ToObject());
            }
            else
            {
                lhs = lhsVal.ToObject();
                rhs = rhsVal.ToObject();
            }

            return new Tuple<object?, object?>(lhs, rhs);
        }

        private GaoooValue AutoEval(Stack<GaoooValue> tokens, Func<Expression, Expression, Expression> func)
        {
            var r = tokens.Pop().Eval(_sys);
            var l = tokens.Pop().Eval(_sys);
            var srcObjPair = AutoConvert(l, r);
            var le = Expression.Constant(srcObjPair.Item1);
            var re = Expression.Constant(srcObjPair.Item2);
            var ope = func(le, re);
            var obj = Expression.Lambda(ope).Compile().DynamicInvoke();
            return obj.ObjectToGaoooValue();
        }

        public GaoooValue EvalStr(string exp)
        {
            var tokens = new Stack<GaoooValue>();
            var stack = GaoooRpn.Parse(exp);
            foreach (var x in stack)
            {
                if (x == "==")
                {
                    var val = AutoEval(tokens, Expression.Equal);
                    tokens.Push(val);
                }
                else if (x == "!=")
                {
                    var val = AutoEval(tokens, Expression.NotEqual);
                    tokens.Push(val);
                }
                else if (x == ">")
                {
                    var val = AutoEval(tokens, Expression.GreaterThan);
                    tokens.Push(val);
                }
                else if (x == ">=")
                {
                    var val = AutoEval(tokens, Expression.GreaterThanOrEqual);
                    tokens.Push(val);
                }
                else if (x == "<")
                {
                    var val = AutoEval(tokens, Expression.LessThan);
                    tokens.Push(val);
                }
                else if (x == "<=")
                {
                    var val = AutoEval(tokens, Expression.LessThanOrEqual);
                    tokens.Push(val);
                }
                else if (x == "+")
                {
                    var r = tokens.Pop().Eval(_sys);
                    var l = tokens.Pop().Eval(_sys);
                    var lhsIsStr = l.GetType() == typeof(GaoooValueString);
                    var rhsIsStr = r.GetType() == typeof(GaoooValueString);

                    var srcObjPair = AutoConvert(l, r);
                    var lobj = srcObjPair.Item1;
                    var robj = srcObjPair.Item2;

                    if (lhsIsStr || rhsIsStr)
                    {
                        var lstr = lobj?.ToString() ?? string.Empty;
                        var rstr = robj?.ToString() ?? string.Empty;
                        var val = lstr + rstr;
                        tokens.Push(new GaoooValueString(val));
                    }
                    else
                    {
                        var le = Expression.Constant(lobj);
                        var re = Expression.Constant(robj);
                        var ope = Expression.Add(le, re);
                        var val = Expression.Lambda(ope).Compile().DynamicInvoke();
                        tokens.Push(val.ObjectToGaoooValue());
                    }
                }
                else if (x == "-")
                {
                    var val = AutoEval(tokens, Expression.Subtract);
                    tokens.Push(val);
                }
                else if (x == "*")
                {
                    var val = AutoEval(tokens, Expression.Multiply);
                    tokens.Push(val);
                }
                else if (x == "/")
                {
                    var val = AutoEval(tokens, Expression.Divide);
                    tokens.Push(val);
                }
                else if (x == "%")
                {
                    var val = AutoEval(tokens, Expression.Modulo);
                    tokens.Push(val);
                }
                else if (x == "&&")
                {
                    var val = AutoEval(tokens, Expression.AndAlso);
                    tokens.Push(val);
                }
                else if (x == "||")
                {
                    var val = AutoEval(tokens, Expression.OrElse);
                    tokens.Push(val);
                }
                else if (x == "=")
                {
                    var r = tokens.Pop().Eval(_sys);
                    var l = tokens.Pop().ToString();
                    _sys.SetVariable(l, r);
                    tokens.Push(r);
                }
                else if (x.StartsWith(":"))
                {
                }
                else if (x.StartsWith("@"))
                {
                    var funcName = x.Substring(1);
                    new GaoooBuiltinFunctions(_sys).Invoke(funcName, tokens);
                }
                else if (x == "true" || x == "false")
                {
                    tokens.Push(new GaoooValueBoolean(x == "true"));
                }
                else
                {
                    tokens.Push(x.ObjectToGaoooValue());
                }
            }
            return tokens.Count > 0 ? tokens.Peek().Eval(_sys) : new GaoooValueNull();
        }
    }
}
