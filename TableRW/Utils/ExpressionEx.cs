using E = System.Linq.Expressions.Expression;

namespace TableRW.Utils.Ex;
public static class ExpressionEx {

    //internal static Expression UpdateBlockType(this BlockExpression block, Expression resultValue) {
    //    return E.Block(resultValue.Type, block.Variables, block.Expressions.ReplaceLastElement(resultValue));
    //}


    public static Expression ExtractBody(this LambdaExpression lmd, params object[] newParams) {
        var modify = new UpdateParameters(lmd.Parameters, newParams);
        return modify.Visit(lmd.Body);
    }

    public static Expression UpdateParameters(this Expression exp,
        ParameterExpression old, ParameterExpression @new
    ) {
        var modify = new UpdateParameters(new[] { old }, @new);
        return modify.Visit(exp);
    }

    internal static Expression ReplaceMemberAccess(this Expression expr, MemberInfo member, Expression newExpr) {
        var replace = new ReplaceMemberAccess(member, newExpr);
        return replace.Visit(expr);
    }

    internal static MethodCallExpression Call(
        this Expression instance, string methodName, params Expression[] argsExpr
    ) {
        var method = instance.Type.GetMethod(methodName, argsExpr.Select(e => e.Type).ToArray());
        return E.Call(instance, method, argsExpr);
    }


    //public static Expression ExtractBody<T>(this T lmd, object newParams)
    //where T : LambdaExpression {
    //    var modify = new UpdateParameters(lmd.Parameters, newParams);
    //    return modify.Visit(lmd.Body);
    //}

    //public static T UpdateParameters<T>(this T lmd, params object[] newParams)
    //where T : LambdaExpression {
    //    var modify = new UpdateParameters(lmd.Parameters, newParams);
    //    return (T)modify.Visit(lmd);
    //}


    //public static T UpdateParameter<T>(this T lmd, string newParam)
    //where T : LambdaExpression {
    //    if (lmd.Parameters.Count > 1) { throw new NotSupportedException("Please use: UpdateParameters<T>(this T, params string[])"); }
    //    if (lmd.Parameters.Count == 0) { return lmd; }

    //    var modify = new UpdateParameter(lmd.Parameters[0], newParam);
    //    return (T)modify.Visit(lmd);
    //}

    //public static T UpdateParameter<T>(this T lmd, ParameterExpression newParam)
    //where T : LambdaExpression {
    //    if (lmd.Parameters.Count > 1) { throw new NotSupportedException("Please use: UpdateParameters<T>(this T, params string[])"); }
    //    if (lmd.Parameters.Count == 0) { return lmd; }

    //    var modify = new UpdateParameter(newParam);
    //    return (T)modify.Visit(lmd);
    //}

    //public static LambdaExpression UpdateParameter2<T, T2>(this T lmd, ParameterExpression newParam)
    //where T : LambdaExpression where T2 : LambdaExpression {
    //    if (lmd.Parameters.Count > 1) { throw new NotSupportedException("Please use: UpdateParameters<T>(this T, params string[])"); }
    //    if (lmd.Parameters.Count == 0) { return lmd; }

    //    var modify = new UpdateParameter(newParam);
    //    return modify.VisitAndConvert(lmd, "-");
    //}

    //public static T OnlySupported<T>(this Expression exp) where T : Expression {
    //    return exp is T res ? res : throw new NotSupportedException($"Only `{typeof(T).Name}` is supported");
    //}
}
