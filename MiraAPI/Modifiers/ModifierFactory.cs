using System;
using System.Linq;
using System.Linq.Expressions;

namespace MiraAPI.Modifiers;

public static class ModifierFactory
{
    private static Func<object[], BaseModifier>? _constructor;

    private static Func<object[], BaseModifier> CreateConstructor(Type type)
    {
        var constructorInfo = type.GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault() ?? throw new InvalidOperationException($"Type {type} does not have a public constructor.");
        var parameters = constructorInfo.GetParameters();

        var argsParam = Expression.Parameter(typeof(object[]), "args");
        var constructorParams = new Expression[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var paramType = parameters[i].ParameterType;
            var paramAccess = Expression.ArrayIndex(argsParam, Expression.Constant(i));
            var convertedParam = Expression.Convert(paramAccess, paramType);
            constructorParams[i] = convertedParam;
        }

        var newExpr = Expression.New(constructorInfo, constructorParams);
        var lambda = Expression.Lambda<Func<object[], BaseModifier>>(newExpr, argsParam);

        return lambda.Compile();
    }

    public static BaseModifier CreateInstance(Type type, params object[] args)
    {
        _constructor ??= CreateConstructor(type);
        return _constructor(args);
    }
}

/// <summary>
/// Factory for creating instances of a modifier. More efficient than using reflection.
/// </summary>
/// <typeparam name="T">The modifier type.</typeparam>
public static class ModifierFactory<T> where T : BaseModifier
{
    private static readonly Func<object[], T> Constructor = CreateConstructor();

    private static Func<object[], T> CreateConstructor()
    {
        var constructorInfo = typeof(T).GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault() ?? throw new InvalidOperationException($"Type {typeof(T)} does not have a public constructor.");
        var parameters = constructorInfo.GetParameters();

        var argsParam = Expression.Parameter(typeof(object[]), "args");
        var constructorParams = new Expression[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var paramType = parameters[i].ParameterType;
            var paramAccess = Expression.ArrayIndex(argsParam, Expression.Constant(i));
            var convertedParam = Expression.Convert(paramAccess, paramType);
            constructorParams[i] = convertedParam;
        }

        var newExpr = Expression.New(constructorInfo, constructorParams);
        var lambda = Expression.Lambda<Func<object[], T>>(newExpr, argsParam);

        return lambda.Compile();
    }

    /// <summary>
    /// Creates an instance of the modifier.
    /// </summary>
    /// <param name="args">Parameters for the constructor.</param>
    /// <returns>An instance of the modifier.</returns>
    public static T CreateInstance(params object[] args)
    {
        return Constructor(args);
    }
}
