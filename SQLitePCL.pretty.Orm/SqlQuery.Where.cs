﻿using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using SQLitePCL.pretty.Orm.Attributes;

namespace SQLitePCL.pretty.Orm
{
    public static partial class SqlQuery
    {
        public sealed class WhereClause<T> : ISqlQuery
        {
            private readonly FromClause<T> from;
            private readonly Expression select;
            private readonly Expression where;

            internal WhereClause(FromClause<T> from, Expression select, Expression where)
            {
                this.from = from;
                this.select = select;
                this.where = where;
            }

            public OrderByClause<T> OrderBy<TValue>(Expression<Func<T, TValue>> orderExpr)
            {
                Contract.Requires(orderExpr != null);
                return CreateOrderBy(orderExpr, true);
            }

            public OrderByClause<T> OrderByDescending<TValue>(Expression<Func<T, TValue>> orderExpr)
            {
                Contract.Requires(orderExpr != null);
                return CreateOrderBy(orderExpr, false);
            }

            private OrderByClause<T> CreateOrderBy<TValue>(Expression<Func<T, TValue>> orderExpr, bool asc)
            {  
                Contract.Requires(orderExpr != null);

                var orderBy = new List<Tuple<string, bool>>();
                orderBy.Add(orderExpr.CompileOrderByExpression(asc));
                return new OrderByClause<T>(this, orderBy);
            }

            public WhereClause<T> Where<U,V,W,X,Y,Z>(Expression<Func<T,U,V,W,X,Y,Z,bool>> predExpr)
            {
                Contract.Requires(predExpr != null);
                return this.Where((LambdaExpression) predExpr);
            }

            public WhereClause<T> Where<U,V,W,X,Y>(Expression<Func<T,U,V,W,X,Y,bool>> predExpr)
            {
                Contract.Requires(predExpr != null);
                return this.Where((LambdaExpression) predExpr);
            }

            public WhereClause<T> Where<U,V,W,X>(Expression<Func<T,U,V,W,X,bool>> predExpr)
            {
                Contract.Requires(predExpr != null);
                return this.Where((LambdaExpression) predExpr);
            }

            public WhereClause<T> Where<U,V,W>(Expression<Func<T,U,V,W,bool>> predExpr)
            {
                Contract.Requires(predExpr != null);
                return this.Where((LambdaExpression) predExpr);
            }

            public WhereClause<T> Where<U,V>(Expression<Func<T,U,V,bool>> predExpr)
            {
                Contract.Requires(predExpr != null);
                return this.Where((LambdaExpression) predExpr);
            }

            public WhereClause<T> Where<U>(Expression<Func<T,U,bool>> predExpr)
            {
                Contract.Requires(predExpr != null);
                return this.Where((LambdaExpression) predExpr);
            }

            public WhereClause<T> Where(Expression<Func<T, bool>> predExpr)
            {
                Contract.Requires(predExpr != null);
                return this.Where((LambdaExpression) predExpr);
            }

            private WhereClause<T> Where(LambdaExpression lambda)
            {
                var pred = lambda.Body;
                var where = this.where == null ? pred : Expression.AndAlso(this.where, pred);
                return new WhereClause<T>(from, select, where);
            }

            /// <summary>
            /// Returns a <see cref="TableQuery&lt;T&gt;"/> that limits the result set to a specified number of contiguous elements.
            /// </summary>
            /// <param name="n">The number of elements to return.</param>
            /// <returns>A new <see cref="SQLitePCL.pretty.Orm.TableQuery&lt;T&gt;"/>.</returns>
            public LimitClause<T> Take(int n)
            {
                Contract.Requires(n >= 0);
                return new LimitClause<T>(new OrderByClause<T>(this, new List<Tuple<string, bool>>()), n, null);
            }

            /// <summary>
            /// Returns a <see cref="TableQuery&lt;T&gt;"/> that skips a specified number of elements in the result set and then returns the remaining elements.
            /// </summary>
            /// <param name="n">The number of elements to skip before returning the remaining elements.</param>
            /// <returns>A new <see cref="SQLitePCL.pretty.Orm.TableQuery&lt;T&gt;"/>.</returns>
            public LimitClause<T> Skip(int n)
            {
                Contract.Requires(n >= 0);
                return new LimitClause<T>(new OrderByClause<T>(this, new List<Tuple<string, bool>>()), null, n);
            }

            /// <summary>
            /// Returns a <see cref="TableQuery&lt;T&gt;"/> that returns the element at a specified index in the result set.
            /// </summary>
            /// <returns>The <see cref="SQLitePCL.pretty.Orm.TableQuery&lt;T&gt;"/>.</returns>
            /// <param name="index">Index.</param>
            /// <returns>A new <see cref="SQLitePCL.pretty.Orm.TableQuery&lt;T&gt;"/>.</returns>
            public LimitClause<T> ElementAt(int index)
            {
                Contract.Requires(index >= 0);
                return Skip(index).Take(1);
            }

            public override string ToString()
            {
                return     
                    "SELECT " + select.CompileWhereExpr() + 
                    "\r\n" + from.ToString() + 
                    (where != null ? "\r\nWHERE " + where.CompileWhereExpr() : "");
            }
        }

        private static String CompileWhereExpr(this Expression This)
        {
            if (This is BinaryExpression)
            {
                var bin = (BinaryExpression)This;
                
                var leftExpr = bin.Left.CompileWhereExpr();
                var rightExpr = bin.Right.CompileWhereExpr();

                if (rightExpr == "NULL" && bin.NodeType == ExpressionType.Equal)
                {
                    if (bin.NodeType == ExpressionType.Equal)
                    {
                        return "(" + leftExpr + "IS NULL)";
                    }
                    else if (rightExpr == "NULL" && bin.NodeType == ExpressionType.NotEqual)
                    {
                        return "(" + leftExpr + "IS NOT NULL)";
                    }
                }

                return "(" + leftExpr + " " + GetSqlName(bin) + " " + rightExpr + ")";
            }
            else if (This is ParameterExpression)
            {
                var param = (ParameterExpression)This;
                return ":" + param.Name;
            }
            else if (This is MemberExpression)
            {
                var member = (MemberExpression) This;

                if (member.Expression != null && member.Expression.NodeType == ExpressionType.Parameter)
                {
                    // This is a column in the table, output the column name
                    var columnName = ((PropertyInfo) member.Member).GetColumnName();
                    return "\"" + columnName + "\"";
                }
                else
                {
                    return member.EvaluateExpression().ConvertToSQLiteValue().ToSqlString();
                }
            }
            else if (This.NodeType == ExpressionType.Not)
            {
                var operandExpr = ((UnaryExpression) This).Operand;
                return "NOT(" + operandExpr.CompileWhereExpr() + ")";
            } 
            else if (This is ConstantExpression) 
            {
                return This.EvaluateExpression().ConvertToSQLiteValue().ToSqlString();
            }
            else if (This is MethodCallExpression)
            {
                var call = (MethodCallExpression) This;
                var args = new String[call.Arguments.Count];

                var obj = call.Object != null ? call.Object.CompileWhereExpr() : null;
                
                for (var i = 0; i < args.Length; i++) 
                {
                    args [i] = call.Arguments[i].CompileWhereExpr();
                }
                
                if (call.Method.Name == "Like" && args.Length == 2) 
                {
                    return "(" + args[0] + " LIKE " + args[1] + ")";
                }

                else if (call.Method.Name == "Contains" && args.Length == 2) 
                {
                    return "(" + args[1] + " IN " + args[0] + ")";
                }

                else if (call.Method.Name == "Contains" && args.Length == 1)
                 {
                    if (call.Object != null && call.Object.Type == typeof(string))
                    {
                        return "(" + obj + " LIKE ('%' || " + args[0] + " || '%'))";
                    }
                    else 
                    {
                        return "(" + args[0] + " IN " + obj + ")";
                    }
                }

                else if (call.Method.Name == "StartsWith" && args.Length == 1) 
                {
                    return "(" + obj + " LIKE (" + args[0] + " || '%'))";
                }

                else if (call.Method.Name == "EndsWith" && args.Length == 1) 
                {
                    return "(" + obj + " LIKE ('%' || " + args[0] + "))";
                }

                else if (call.Method.Name == "Equals" && args.Length == 1) 
                {
                    return "(" + obj + " = (" + args[0] + "))";
                }

                else if (call.Method.Name == "Is" && args.Length == 2)
                {
                    return "(" + args[0] + " IS " + args[1] + ")";
                }

                else if (call.Method.Name == "IsNot" && args.Length == 2)
                {
                    return "(" + args[0] + " IS NOT " + args[1] + ")";
                }
            }
            else if (This.NodeType == ExpressionType.Convert) 
            {
                var u = (UnaryExpression) This;
                var ty = u.Type;
                var value = EvaluateExpression(u.Operand);

                return value.ConvertTo(ty).ConvertToSQLiteValue().ToSqlString();
            } 

            throw new NotSupportedException("Cannot compile: " + This.NodeType.ToString());
        }
    }
}
