using System.Text;

internal class ArgumentsParser
{
    public static string[] Parse(string[] args)
    {
        StringBuilder builder = new StringBuilder();
        bool inSingleQuote = false;
        bool inDoubleQuote = false;
        bool escaped = false;

        foreach (var arg in args)
        {
            foreach (char c in arg)
            {
                if (!escaped)
                {
                    switch (c)
                    {
                        case '^':
                            escaped = true;
                            break;
                        case '"':
                            if (!inSingleQuote)
                            {
                                inDoubleQuote = !inDoubleQuote;
                                builder.Append('\n');
                            }
                            break;
                        case '\'':
                            if (!inDoubleQuote)
                            {
                                inSingleQuote = !inSingleQuote;
                                builder.Append('\n');
                            }
                            break;
                        case ' ':
                            if (!inSingleQuote && !inDoubleQuote)
                                builder.Append('\n');
                            else
                                builder.Append(c);
                            break;
                        default:
                            builder.Append(c);
                            break;
                    }
                }
                else
                {
                    builder.Append(c);
                    escaped = false;
                }
            }
            builder.Append('\n');
        }

        if (inSingleQuote || inDoubleQuote)
            throw new ArgumentException("Unterminated quote.");

        return builder.ToString().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
    }
}