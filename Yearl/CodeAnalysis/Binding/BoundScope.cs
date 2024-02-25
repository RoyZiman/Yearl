using System.Collections.Immutable;

namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundScope(BoundScope parent)
    {
        private Dictionary<string, VariableSymbol> _variables = [];
        public BoundScope Parent { get; } = parent;

        public bool TryDeclare(VariableSymbol variable)
        {
            if (_variables.ContainsKey(variable.Name))
                return false;

            _variables.Add(variable.Name, variable);
            return true;
        }

        public bool TryLookup(string name, out VariableSymbol? variable)
        {
            if (_variables.TryGetValue(name, out variable))
                return true;

            if (Parent == null)
                return false;

            return Parent.TryLookup(name, out variable);
        }

        public ImmutableArray<VariableSymbol> GetDeclaredVariables()
        {
            return _variables.Values.ToImmutableArray();
        }
    }
}