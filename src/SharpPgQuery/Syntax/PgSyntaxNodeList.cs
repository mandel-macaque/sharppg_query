using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;

namespace SharpPgQuery.Syntax
{
    /// <summary>
    /// An immutable, ordered list of <see cref="PgSyntaxNode"/> children.
    /// Mirrors the design of <c>Microsoft.CodeAnalysis.ChildSyntaxList</c>.
    /// Instances are built lazily from a <see cref="JsonElement"/>.
    /// </summary>
    public sealed class PgSyntaxNodeList : IReadOnlyList<PgSyntaxNode>
    {
        /// <summary>Gets a singleton empty list.</summary>
        public static readonly PgSyntaxNodeList Empty = new PgSyntaxNodeList(Array.Empty<PgSyntaxNode>());

        private readonly PgSyntaxNode[] _nodes;

        private PgSyntaxNodeList(PgSyntaxNode[] nodes) => _nodes = nodes;

        /// <inheritdoc/>
        public int Count => _nodes.Length;

        /// <inheritdoc/>
        public PgSyntaxNode this[int index] => _nodes[index];

        /// <inheritdoc/>
        public IEnumerator<PgSyntaxNode> GetEnumerator() =>
            ((IEnumerable<PgSyntaxNode>)_nodes).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _nodes.GetEnumerator();

        /// <summary>
        /// Builds a <see cref="PgSyntaxNodeList"/> from the properties of a JSON object,
        /// mapping each property value to a child node.
        /// </summary>
        internal static PgSyntaxNodeList FromJsonObject(JsonElement element, PgSyntaxNode parent)
        {
            var list = new List<PgSyntaxNode>();
            foreach (var property in element.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.String ||
                    property.Value.ValueKind == JsonValueKind.Number ||
                    property.Value.ValueKind == JsonValueKind.True ||
                    property.Value.ValueKind == JsonValueKind.False ||
                    property.Value.ValueKind == JsonValueKind.Null)
                    continue; // leaf values – not represented as child nodes

                var kind = SyntaxKindMap.Resolve(property.Name);
                list.Add(new PgSyntaxNode(property.Value, parent, kind, property.Name));
            }

            return new PgSyntaxNodeList(list.ToArray());
        }

        /// <summary>
        /// Builds a <see cref="PgSyntaxNodeList"/> from the elements of a JSON array.
        /// </summary>
        internal static PgSyntaxNodeList FromJsonArray(JsonElement element, PgSyntaxNode parent)
        {
            int length = element.GetArrayLength();
            var nodes = new PgSyntaxNode[length];
            int index = 0;
            foreach (var item in element.EnumerateArray())
            {
                var kind = item.ValueKind == JsonValueKind.Object
                    ? SyntaxKindMap.ResolveFromObjectKeys(item)
                    : PgSyntaxKind.Unknown;
                nodes[index++] = new PgSyntaxNode(item, parent, kind);
            }

            return new PgSyntaxNodeList(nodes);
        }
    }
}
