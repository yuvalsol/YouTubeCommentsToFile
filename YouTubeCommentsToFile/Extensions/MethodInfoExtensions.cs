#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Reflection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

// https://gist.github.com/jamietre/4476307

public static partial class MethodInfoExtensions
{
    /// <summary>
    /// Return the method signature as a string.
    /// </summary>
    ///
    /// <param name="property">
    /// The property to act on.
    /// </param>
    ///
    /// <returns>
    /// Method signature.
    /// </returns>
    public static StringBuilder GetSignature(this PropertyInfo property)
    {
        var sigBuilder = new StringBuilder();

        var getter = property.GetGetMethod();
        var setter = property.GetSetMethod();
        var primaryDef = LeastRestrictiveVisibility(getter, setter);

        BuildReturnSignature(sigBuilder, primaryDef);

        sigBuilder.Append(" { ");

        if (getter != null)
        {
            if (primaryDef != getter)
            {
                sigBuilder.Append(Visibility(getter));
                sigBuilder.Append(' ');
            }

            sigBuilder.Append("get; ");
        }

        if (setter != null)
        {
            if (primaryDef != setter)
            {
                sigBuilder.Append(Visibility(setter));
                sigBuilder.Append(' ');
            }

            sigBuilder.Append("set; ");
        }

        sigBuilder.Append('}');

        return sigBuilder;
    }

    /// <summary>
    /// Return the method signature as a string.
    /// </summary>
    ///
    /// <param name="method">
    /// The Method.
    /// </param>
    /// <param name="callable">
    /// Return as an callable string(public void a(string b) would return a(b))
    /// </param>
    ///
    /// <returns>
    /// Method signature.
    /// </returns>
    public static StringBuilder GetSignature(this MethodInfo method, bool callable = false)
    {
        var sigBuilder = new StringBuilder();

        BuildReturnSignature(sigBuilder, method, callable);

        sigBuilder.Append('(');

        bool firstParam = true;
        bool secondParam = false;
        foreach (var param in method.GetParameters())
        {
            if (firstParam)
            {
                firstParam = false;

                if (method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false))
                {
                    if (callable)
                    {
                        secondParam = true;
                        continue;
                    }

                    sigBuilder.Append("this ");
                }
            }
            else if (secondParam)
            {
                secondParam = false;
            }
            else
            {
                sigBuilder.Append(", ");
            }

            if (param.IsOut)
                sigBuilder.Append("out ");
            else if (param.ParameterType.IsByRef)
                sigBuilder.Append("ref ");

            bool isParamArray = param.GetCustomAttribute(typeof(ParamArrayAttribute), true) != null;
            if (isParamArray)
                sigBuilder.Append("params ");

            if (!callable)
            {
                TypeName(sigBuilder, param.ParameterType);
                sigBuilder.Append(' ');
            }

            sigBuilder.Append(param.Name);

            if (param.IsOptional)
            {
                sigBuilder.Append(" = ");
                sigBuilder.Append(param.DefaultValue ?? "null");
            }
        }

        sigBuilder.Append(')');

        // generic constraints
        foreach (var arg in method.GetGenericArguments())
        {
            List<string> constraints = [];
            foreach (var constraint in arg.GetGenericParameterConstraints())
            {
                var sb = new StringBuilder();
                TypeName(sb, constraint);
                constraints.Add(sb.ToString());
            }

            var attrs = arg.GenericParameterAttributes;
            GenericParameterAttributes specialConstraints = attrs & GenericParameterAttributes.SpecialConstraintMask;

            if ((specialConstraints & GenericParameterAttributes.ReferenceTypeConstraint) == GenericParameterAttributes.ReferenceTypeConstraint)
                constraints.Add("class");

            if ((specialConstraints & GenericParameterAttributes.NotNullableValueTypeConstraint) == GenericParameterAttributes.NotNullableValueTypeConstraint)
                constraints.Add("struct");

            if ((specialConstraints & GenericParameterAttributes.DefaultConstructorConstraint) == GenericParameterAttributes.DefaultConstructorConstraint)
                constraints.Add("new()");

            if (constraints.Count > 0)
            {
                sigBuilder.Append(" where ");
                TypeName(sigBuilder, arg);
                sigBuilder.Append(": ");
                sigBuilder.Append(string.Join(", ", constraints));
            }
        }

        return sigBuilder;
    }

    private static void TypeName(StringBuilder sigBuilder, Type type)
    {
        var nullableType = Nullable.GetUnderlyingType(type);
        if (nullableType != null)
        {
            TypeName(sigBuilder, nullableType);
            sigBuilder.Append('?');
            return;
        }

        string typeName = type.Name;
        if (typeName.EndsWith('&'))
            typeName = typeName.TrimEnd('&');

        if (!type.IsGenericType)
        {
            if (type.IsArray)
            {
                TypeName(sigBuilder, type.GetElementType());
                sigBuilder.Append("[]");
                return;
            }

            sigBuilder.Append(typeName switch
            {
                "Boolean" or "System.Boolean" => "bool",
                "Byte" or "System.Byte" => "byte",
                "Char" or "System.Char" => "char",
                "Decimal" or "System.Decimal" => "decimal",
                "Double" or "System.Double" => "double",
                "Int32" or "System.Int32" => "int",
                "Int64" or "System.Int64" => "long",
                "Object" or "System.Object" => "object",
                "SByte" or "System.SByte" => "sbyte",
                "Int16" or "System.Int16" => "short",
                "String" or "System.String" => "string",
                "UInt32" or "System.UInt32" => "uint",
                "UInt64" or "System.UInt64" => "ulong",
                "UInt16" or "System.UInt16" => "ushort",
                "Void" => "void",
                _ => string.IsNullOrWhiteSpace(type.FullName) ? typeName : type.FullName.TrimEnd('&')
            });

            return;
        }

        sigBuilder.Append(typeName[..typeName.IndexOf('`')]);
        sigBuilder.Append('<');

        bool first = true;
        foreach (var t in type.GetGenericArguments())
        {
            if (!first)
                sigBuilder.Append(',');
            TypeName(sigBuilder, t);
            first = false;
        }

        sigBuilder.Append('>');
    }

    private static void BuildReturnSignature(StringBuilder sigBuilder, MethodInfo method, bool callable = false)
    {
        if (!callable)
        {
            sigBuilder.Append(Visibility(method));
            sigBuilder.Append(' ');
            if (method.IsStatic)
                sigBuilder.Append("static ");
            TypeName(sigBuilder, method.ReturnType);
            sigBuilder.Append(' ');
        }

        sigBuilder.Append(method.Name);

        // add method generics
        if (method.IsGenericMethod)
        {
            sigBuilder.Append('<');

            bool firstParam = true;
            foreach (var g in method.GetGenericArguments())
            {
                if (firstParam)
                    firstParam = false;
                else
                    sigBuilder.Append(", ");

                TypeName(sigBuilder, g);
            }

            sigBuilder.Append('>');
        }
    }

    private static MethodInfo LeastRestrictiveVisibility(MethodInfo member1, MethodInfo member2)
    {
        if (member1 != null && member2 == null)
            return member1;

        if (member2 != null && member1 == null)
            return member2;

        return (VisibilityValue(member1) < VisibilityValue(member2) ? member1 : member2);
    }

    private static string Visibility(MethodInfo method)
    {
        if (method.IsPublic)
            return "public";
        if (method.IsFamily)
            return "protected";
        if (method.IsAssembly)
            return "internal";
        if (method.IsFamilyOrAssembly)
            return "protected internal";
        if (method.IsFamilyAndAssembly)
            return "private protected";
        if (method.IsPrivate)
            return "private";

        throw new ArgumentException($"Method {method.Name} is missing visibility.", nameof(method));
    }

    private static int VisibilityValue(MethodInfo method)
    {
        if (method.IsPublic)
            return 1;
        if (method.IsFamily)
            return 2;
        if (method.IsAssembly)
            return 3;
        if (method.IsFamilyOrAssembly)
            return 4;
        if (method.IsFamilyAndAssembly)
            return 5;
        if (method.IsPrivate)
            return 6;

        throw new ArgumentException($"Method {method.Name} is missing visibility.", nameof(method));
    }
}
