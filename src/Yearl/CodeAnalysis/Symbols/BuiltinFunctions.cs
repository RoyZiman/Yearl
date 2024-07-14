using System.Collections.Immutable;
using System.Reflection;

namespace Yearl.CodeAnalysis.Symbols
{
    internal static class BuiltinFunctions
    {
        public static readonly FunctionSymbol Print = new("print", [new ParameterSymbol("text", TypeSymbol.String, 0)], TypeSymbol.Void);
        public static readonly FunctionSymbol Input = new("input", [], TypeSymbol.String);
        public static readonly FunctionSymbol Floor = new("floor", [new ParameterSymbol("num", TypeSymbol.Number, 0)], TypeSymbol.Number);

        internal static IEnumerable<FunctionSymbol> GetAll()
        {
            return typeof(BuiltinFunctions).GetFields(BindingFlags.Public | BindingFlags.Static)
                                               .Where(f => f.FieldType == typeof(FunctionSymbol))
                                               .Select(f => (FunctionSymbol)f.GetValue(null));
        }
    }
}