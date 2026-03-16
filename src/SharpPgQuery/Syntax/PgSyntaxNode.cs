using System.Collections.Generic;
using System.Text.Json;

namespace SharpPgQuery.Syntax
{
    /// <summary>
    /// Represents a node in a PostgreSQL syntax tree.
    /// Mirrors the design of <c>Microsoft.CodeAnalysis.SyntaxNode</c>.
    /// Nodes wrap <see cref="JsonElement"/> values from the libpg_query JSON
    /// output and expose children lazily to keep memory usage small.
    /// </summary>
    public class PgSyntaxNode
    {
        private readonly JsonElement _element;
        private readonly PgSyntaxNode? _parent;
        private readonly string? _name;
        private PgSyntaxNodeList? _children;

        internal PgSyntaxNode(JsonElement element, PgSyntaxNode? parent, PgSyntaxKind kind, string? name = null)
        {
            _element = element;
            _parent = parent;
            _name = name;
            Kind = kind;
        }

        /// <summary>Gets the kind of this node.</summary>
        public PgSyntaxKind Kind { get; }

        /// <summary>Gets the parent node, or <c>null</c> if this is the root.</summary>
        public PgSyntaxNode? Parent => _parent;

        /// <summary>
        /// Gets the JSON property name that produced this node, when available.
        /// For array elements or synthesized nodes, this may be <c>null</c>.
        /// </summary>
        public string? Name => _name;

        /// <summary>
        /// Gets whether this node represents a valid (non-error) part of the tree.
        /// </summary>
        public bool IsMissing => _element.ValueKind == JsonValueKind.Undefined;

        /// <summary>
        /// Gets the raw JSON backing this node. Useful for accessing properties
        /// not yet surfaced through the typed API.
        /// </summary>
        public JsonElement RawJson => _element;

        /// <summary>Gets the child nodes of this node (lazily computed).</summary>
        public PgSyntaxNodeList ChildNodes => _children ??= BuildChildren();

        /// <summary>
        /// Returns an ancestor of this node of the given kind, or <c>null</c>.
        /// </summary>
        public PgSyntaxNode? FirstAncestorOrSelf(PgSyntaxKind kind)
        {
            PgSyntaxNode? node = this;
            while (node != null)
            {
                if (node.Kind == kind)
                    return node;
                node = node.Parent;
            }

            return null;
        }

        /// <summary>Enumerates all descendant nodes (depth-first).</summary>
        public IEnumerable<PgSyntaxNode> DescendantNodes()
        {
            var stack = new Stack<PgSyntaxNode>();
            stack.Push(this);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current;
                var children = current.ChildNodes;
                for (int i = children.Count - 1; i >= 0; i--)
                    stack.Push(children[i]);
            }
        }

        /// <summary>Returns the raw JSON representation of this node.</summary>
        public override string ToString() => _element.ToString();

        private PgSyntaxNodeList BuildChildren()
        {
            if (_element.ValueKind == JsonValueKind.Object)
                return PgSyntaxNodeList.FromJsonObject(_element, this);

            if (_element.ValueKind == JsonValueKind.Array)
                return PgSyntaxNodeList.FromJsonArray(_element, this);

            return PgSyntaxNodeList.Empty;
        }
    }
}
