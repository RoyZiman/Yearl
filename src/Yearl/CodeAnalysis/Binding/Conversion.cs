using Yearl.CodeAnalysis.Symbols;

namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class Conversion
    {
        public static readonly Conversion None = new(exists: false, isIdentity: false, isImplicit: false);
        public static readonly Conversion Identity = new(exists: true, isIdentity: true, isImplicit: true);
        public static readonly Conversion Implicit = new(exists: true, isIdentity: false, isImplicit: true);
        public static readonly Conversion Explicit = new(exists: true, isIdentity: false, isImplicit: false);

        private Conversion(bool exists, bool isIdentity, bool isImplicit)
        {
            Exists = exists;
            IsIdentity = isIdentity;
            IsImplicit = isImplicit;
        }

        public bool Exists { get; }
        public bool IsIdentity { get; }
        public bool IsImplicit { get; }
        public bool IsExplicit => Exists && !IsImplicit;

        public static Conversion Classify(TypeSymbol from, TypeSymbol to)
        {
            if (from == to)
                return Identity;

            if (from != TypeSymbol.Void && to == TypeSymbol.Dynamic)
            {
                return Implicit;
            }

            if (from == TypeSymbol.Dynamic && to != TypeSymbol.Void)
            {
                return Explicit;
            }

            if (from == TypeSymbol.Bool || from == TypeSymbol.Number)
            {
                if (to == TypeSymbol.String)
                    return Explicit;
            }

            if (from == TypeSymbol.String)
            {
                if (to == TypeSymbol.Bool || to == TypeSymbol.Number)
                    return Explicit;
            }

            return None;
        }
    }
}