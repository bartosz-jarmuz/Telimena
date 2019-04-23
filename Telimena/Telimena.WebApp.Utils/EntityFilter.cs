using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Telimena.WebApp.Utils
{
    public static class EntityFilter
    {
        internal class ReplaceVisitor : ExpressionVisitor
        {
            public ReplaceVisitor(Expression from, Expression to)
            {
                this.from = from;
                this.to = to;
            }

            private readonly Expression from, to;

            public override Expression Visit(Expression node)
            {
                return node == this.from ? this.to : base.Visit(node);
            }
        }

        private static Expression<Func<TFirstParam, TResult>> Compose<TFirstParam, TIntermediate, TResult>(
            this Expression<Func<TFirstParam, TIntermediate>> first, Expression<Func<TIntermediate, TResult>> second)
        {
            ParameterExpression param = Expression.Parameter(typeof(TFirstParam), "param");

            Expression newFirst = first.Body.Replace(first.Parameters[0], param);
            Expression newSecond = second.Body.Replace(second.Parameters[0], newFirst);

            return Expression.Lambda<Func<TFirstParam, TResult>>(newSecond, param);
        }

        public static IQueryable<T> Match<T>(IQueryable<T> data, Expression<Func<string, bool>> filteringExpression, IEnumerable<Expression<Func<T, string>>> filterProperties)
        {
            IEnumerable<Expression<Func<T, bool>>> predicates = filterProperties.Select(selector =>
                selector.Compose(filteringExpression));

            Expression<Func<T, bool>> filter = predicates.Aggregate(
                PredicateBuilder.False<T>(), (aggregate, next) => aggregate.Or(next));
            return data.Where(filter);
        }

        public  static Expression Replace(this Expression expression, Expression searchEx, Expression replaceEx)
        {
            return new ReplaceVisitor(searchEx, replaceEx).Visit(expression);
        }
    }

    internal static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1
            , Expression<Func<T, bool>> expr2)
        {
            Expression secondBody = expr2.Body.Replace(expr2.Parameters[0], expr1.Parameters[0]);
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, secondBody), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> False<T>()
        {
            return f => false;
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1
            , Expression<Func<T, bool>> expr2)
        {
            Expression secondBody = expr2.Body.Replace(expr2.Parameters[0], expr1.Parameters[0]);
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, secondBody), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> True<T>()
        {
            return f => true;
        }
    }
}