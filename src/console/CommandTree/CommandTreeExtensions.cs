using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Shamir.Console
{
    public static class CommandTreeExtensions
    {
        public static ICommand FindCommand(this ICommandTree tree, ReadOnlySpan<string> args)
        {
            if (tree is null)
            {
                throw new ArgumentNullException(nameof(tree));
            }

            return tree.FindCommand(ImmutableStack<ICommandTree>.Empty, args);
        }

        public static string BuildHelpText(IImmutableStack<ICommandTree> path)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Group");
            sb.Append("    ");

            var pathNodes = path.ToArray();
            for (var i = pathNodes.Length - 1; i > 0; i--)
            {
                sb.Append(pathNodes[i].Name);
                sb.Append(' ');
            }

            var tree = pathNodes[0];
            sb.Append(tree.Name);
            sb.Append(" : ");
            sb.AppendLine(tree.Description);
            sb.AppendLine();

            if (!tree.SubTrees.IsEmpty)
            {
                sb.AppendLine("Subgroups:");

                var maxSpacing = tree.SubTrees.Max(c => c.Name.Length) + 1;
                
                foreach (var child in tree.SubTrees)
                {
                    sb.Append("    ");
                    sb.Append(child.Name);
                    for (var i = 0; i < maxSpacing - child.Name.Length; i++)
                    {
                        sb.Append(' ');
                    }
                    sb.Append(": ");
                    sb.AppendLine(child.Description);
                }

                sb.AppendLine();
            }

            if (!tree.Commands.IsEmpty)
            {
                sb.AppendLine("Commands:");

                var maxSpacing = tree.Commands.Max(c => c.Name.Length) + 1;
                
                foreach (var command in tree.Commands)
                {
                    sb.Append("    ");
                    sb.Append(command.Name);
                    for (var i = 0; i < maxSpacing - command.Name.Length; i++)
                    {
                        sb.Append(' ');
                    }
                    sb.Append(": ");
                    sb.AppendLine(command.Description);
                }
            }
            
            return sb.ToString();
        }
    }
}