public class LexicalAnalyzer
{
    private readonly string input;
    private int index;
    private State currentState;
    private readonly SemanticActions semanticActions;
    private readonly Dictionary<string, TokenType> keywords;

    public LexicalAnalyzer(string input)
    {
        this.input = input;
        this.index = 0;
        this.currentState = State.S;
        this.semanticActions = new SemanticActions();

        // Define keywords and their corresponding token types
        this.keywords = new Dictionary<string, TokenType>
        {
            {"if", TokenType.KeyWord},
            {"else", TokenType.KeyWord},
            {"while", TokenType.KeyWord},
            {"for", TokenType.KeyWord},
            {"print", TokenType.KeyWord},
            {"scan", TokenType.KeyWord},
            {"int", TokenType.KeyWord},
            {"double", TokenType.KeyWord},
            {"bool", TokenType.KeyWord},
            {"null", TokenType.KeyWord},
            {"true", TokenType.KeyWord},
            {"false", TokenType.KeyWord},
            {"pi", TokenType.KeyWord},
        };
    }

    public void Analyze()
    {
        while (index < input.Length)
        {
            char currentChar = input[index];

            switch (currentState)
            {
                case State.S:
                    switch (currentChar)
                    {
                        case ' ':
                            index++;
                            break;
                        case var digit when char.IsDigit(digit):
                            currentState = State.N;
                            break;
                        case var letter when char.IsLetter(letter):
                            currentState = State.I;
                            break;
                        case '=':
                        case '-':
                        case '+':
                        case '*':
                        case '/':
                        case '(':
                        case ')':
                        case '>':
                        case '<':
                        case '!':
                        case ';':
                        case '|':
                        case '&':
                        case '{':
                        case '}':
                        case '[':
                        case ']':
                        case ',':
                        case '.':
                            currentState = State.O;
                            break;
                        default:
                            currentState = State.F;
                            break;
                    }
                    break;

                case State.I:
                    switch (currentChar)
                    {
                        case var letterOrDigit when char.IsLetterOrDigit(letterOrDigit):
                            semanticActions.AppendChar(currentChar);
                            index++;
                            break;
                        default:
                            string lexeme = semanticActions.GetLexeme();
                            TokenType tokenType = TokenType.Identifier;
                            if (keywords.ContainsKey(lexeme))
                                tokenType = keywords[lexeme];
                            Console.WriteLine($"Lexeme: {lexeme} (Type: {tokenType})");
                            semanticActions.Reset();
                            currentState = State.Z;
                            break;
                    }
                    break;

                case State.N:
                    // State N transitions
                    switch (currentChar)
                    {
                        // Handling digits in integer part
                        case var digit when char.IsDigit(digit):
                            semanticActions.AppendChar(currentChar);
                            index++;
                            break;
                        // Transition to State D for fractional part if '.' is encountered
                        case '.':
                            currentState = State.D;
                            semanticActions.AppendChar(currentChar);
                            index++;
                            break;
                        default:
                            // Handling number lexeme completion
                            Console.WriteLine($"Lexeme: {semanticActions.GetLexeme()} (Type: {TokenType.Number})");
                            semanticActions.Reset();
                            currentState = State.Z;
                            break;
                    }
                    break;

                case State.D:
                    // State D transitions
                    switch (currentChar)
                    {
                        // Handling digits in fractional part
                        case var digit when char.IsDigit(digit):
                            semanticActions.AppendChar(currentChar);
                            index++;
                            break;
                        case '.':
                            // If another '.' is encountered, it's an error
                            Console.WriteLine("Error: Invalid input - multiple decimal points in a number.");
                            currentState = State.F;
                            break; // Exit from the method Analyze()
                        default:
                            // Handling number lexeme completion
                            Console.WriteLine($"Lexeme: {semanticActions.GetLexeme()} (Type: {TokenType.Number})");
                            semanticActions.Reset();
                            currentState = State.Z;
                            break;
                    }
                    break;

                case State.O:
                    // Logic for reading operators
                    char nextChar = (index + 1 < input.Length) ? input[index + 1] : '\0';

                    switch (currentChar)
                    {
                        case '=':
                            if (nextChar == '=')
                            {
                                // Compound operator '=='
                                Console.WriteLine($"Lexeme: {currentChar}{nextChar} (Type: {TokenType.Operator})");
                                index += 1;
                            }
                            else
                            {
                                Console.WriteLine($"Lexeme: {currentChar} (Type: {TokenType.Operator})");
                                index++;
                            }
                            break;
                        case '<':
                        case '>':
                        case '!':
                            if (nextChar == '=')
                            {
                                // Compound operators '<=', '>=', '!='
                                Console.WriteLine($"Lexeme: {currentChar}{nextChar} (Type: {TokenType.Operator})");
                                index += 1;
                            }
                            else
                            {
                                Console.WriteLine("Error: Invalid input - unrecognized operator '!'.");
                                currentState = State.F;
                                break;
                            }
                            break;
                        case '&':
                            if (nextChar == '&')
                            {
                                // Compound operator '&&'
                                Console.WriteLine($"Lexeme: {currentChar}{nextChar} (Type: {TokenType.Operator})");
                                index += 1;
                            }
                            else
                            {
                                // Error for single '&' operator
                                Console.WriteLine("Error: Invalid input - unrecognized operator '&'.");
                                currentState = State.F;
                                break;
                            }
                            break;
                        case '|':
                            if (nextChar == '|')
                            {
                                // Compound operator '||'
                                Console.WriteLine($"Lexeme: {currentChar}{nextChar} (Type: {TokenType.Operator})");
                                index += 1;
                            }
                            else
                            {
                                // Error for single '|' operator
                                Console.WriteLine("Error: Invalid input - unrecognized operator '|'.");
                                currentState = State.F;
                                break;
                            }
                            break;
                        default:
                            // Error for unrecognized operator
                            Console.WriteLine($"Lexeme: {currentChar} (Type: {TokenType.Operator})");
                            break;
                    }

                    currentState = State.Z;
                    break;

                case State.Z:
                    // Logic for completing reading and moving to the next character
                    index++;
                    currentState = State.S;
                    break;

                case State.F:
                    // Logic for handling errors
                    Console.WriteLine("Error: Invalid input");
                    currentState = State.Z;
                    break;

                default:
                    Console.WriteLine("Invalid state.");
                    break;
            }
        }
    }
}

public class SemanticActions
{
    private readonly List<char> lexeme;

    public SemanticActions()
    {
        lexeme = new List<char>();
    }

    public void AppendChar(char c)
    {
        lexeme.Add(c);
    }

    public string GetLexeme()
    {
        return new string(lexeme.ToArray());
    }

    public void Reset()
    {
        lexeme.Clear();
    }
}

public enum State
{
    S,
    I,
    N,
    O,
    Z,
    F,
    D
}

public enum TokenType
{
    Identifier,
    Number,
    Operator,
    KeyWord,
}


class Program
{
    static void Main(string[] args)
    {
        string input = "if (a == 52 && c == 5.2) { b = a - c; print(b);} else { print(a);}";
        
        input += " ";

        LexicalAnalyzer analyzer = new LexicalAnalyzer(input);
        analyzer.Analyze();
    }
}
