using System.Collections.Immutable;
using compilers.CodeAnalysis.Symbol;

namespace compilers.CodeAnalysis.Binding
{
    internal sealed class BoundScope
    {
        private Dictionary<string, BoundVariableExpression>? _variables;
        private Dictionary<string, FunctionSymbol>? _functions;

        public BoundScope(BoundScope? parent)
        {
            Parent = parent;
        }
        public BoundScope? Parent { get; }
        public bool TryDeclareVariable(BoundVariableExpression variable)
        {
            if (_variables == null)
                _variables = new Dictionary<string, BoundVariableExpression>();
            if (_variables.ContainsKey(variable.Variable.Name))
                return false;
            _variables.Add(variable.Variable.Name, variable);
            return true;
        }
        public bool TryLookupVariable(string name, out BoundVariableExpression? variable)
        {

            if (_variables != null && _variables.TryGetValue(name, out variable))
                return true;
            if (Parent == null)
            {
                variable = null;
                return false;
            }
            return Parent.TryLookupVariable(name, out variable);
        }
        public bool TryDeclareFunction(FunctionSymbol function)
        {
            if (_functions == null)
                _functions = new Dictionary<string, FunctionSymbol>();
            if (_functions.ContainsKey(function.Name))
                return false;
            _functions.Add(function.Name, function);
            return true;
        }
        public bool TryLookupFunction(string name, out FunctionSymbol? function)
        {
            if (_functions != null && _functions.TryGetValue(name, out function))
                return true;
            if (Parent == null)
            {
                function = null;
                return false;
            }
            return Parent.TryLookupFunction(name, out function);
        }
        public ImmutableArray<BoundVariableExpression> GetDeclaredVariables()
        {
            if (_variables == null)
                return ImmutableArray<BoundVariableExpression>.Empty;
            return _variables.Values.ToImmutableArray();
        }
        public ImmutableArray<FunctionSymbol> GetDeclaredFunctions()
        {
            if (_functions == null)
                return ImmutableArray<FunctionSymbol>.Empty;
            return _functions!.Values.ToImmutableArray();
        }
    }
}