using System;
using System.Linq.Expressions;

namespace MiraAPI.Modifiers;

/// <summary>
/// Factory for creating instances of a modifier. More efficient than using reflection.
/// </summary>
public static class ModifierFactory
{
    private static Func<object[], BaseModifier>? _constructor;

    private static Func<object[], BaseModifier> CreateConstructor(Type type, params object[] args)
    {
        var constructorInfo = Array.Find(
            type.GetConstructors(),
            x =>
            {
                var parameters = x.GetParameters();
                return parameters.Length == args.Length && Array.TrueForAll(
                    parameters,
                    t => t.ParameterType.IsInstanceOfType(args[t.Position]));
            }) ?? throw new InvalidOperationException(
            $"Could not find a constructor for type {type} with the specified arguments.");

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

    /// <summary>
    /// Creates a modifier with the specified type and arguments.
    /// </summary>
    /// <param name="type">Modifier type.</param>
    /// <param name="args">Arguments.</param>
    /// <returns>An instance of the modifier.</returns>
    public static BaseModifier CreateInstance(Type type, params object[] args)
    {
        _constructor ??= CreateConstructor(type, args);
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

    private static Func<object[], T> CreateConstructor(params object[] args)
    {
        var constructorInfo = Array.Find(
            typeof(T).GetConstructors(),
            x =>
            {
                var parameters = x.GetParameters();
                return parameters.Length == args.Length && Array.TrueForAll(
                    parameters,
                    t => t.ParameterType.IsInstanceOfType(args[t.Position]));
            }) ?? throw new InvalidOperationException($"Could not find a constructor for type {typeof(T)} with the specified arguments.");

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
