using System.Collections.Generic;
using System.Text;

namespace Gaooo
{
    internal class GaoooRpn
    {
        // 演算子の優先順位
        readonly static List<string> OperatorPriorities = new List<string>
        {
            "*", "/", "%",
            "+", "-",
            "<", "<=", ">", ">=", "==", "!=",
            "&&", "||",
            "=",
        };

        private static bool isKindOfOperator(char c)
        {
            switch (c)
            {
                case '*':
                case '/':
                case '%':
                case '+':
                case '-':
                case '<':
                case '>':
                case '=':
                case '!':
                case '&':
                case '|':
                    return true;
                default:
                    return false;
            }
        }

        private static string parseOperator(char first, Queue<char> queue)
        {
            var sb = new StringBuilder();
            sb.Append(first);
            while (queue.Count > 0)
            {
                var c = queue.Peek();
                if (isKindOfOperator(c))
                {
                    sb.Append(queue.Dequeue());
                }
                else
                {
                    break;
                }
            }
            return sb.ToString();
        }

        private static List<string> parseInParentheses(Queue<char> queue, char endOfParentheses)
        {
            if (endOfParentheses == '\"' ||
                endOfParentheses == '\'')
            {
                var sb = new StringBuilder();
                var c = queue.Dequeue();
                while (c != endOfParentheses)
                {
                    if (c == '\\') {
                        c = queue.Dequeue();
                    }
                    sb.Append(c);
                    c = queue.Dequeue();
                }
                return new List<string>() { "\"" + new string(sb.ToString()) + "\"" };
            }

            var rpn = new List<string>();
            var undeterminedId = new StringBuilder();
            var reservedOperator = new Stack<string>();

            void captureId()
            {
                var s = undeterminedId.ToString().Trim();
                if (!string.IsNullOrEmpty(s))
                {
                    rpn.Add(s);
                }
                undeterminedId.Clear();
            }

            void captureFunction()
            {
                var s = undeterminedId.ToString().Trim();
                if (!string.IsNullOrEmpty(s))
                {
                    reservedOperator.Push("@" + s);
                }
                undeterminedId.Clear();
            }

            void captureOperator()
            {
                while (reservedOperator.Count > 0)
                {
                    rpn.Add(reservedOperator.Pop());
                }
            }

            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                // スペース or カンマ？
                if (char.IsWhiteSpace(c))
                {
                    captureId();
                }
                // カンマ？
                else if (c == ',')
                {
                    captureId();
                    captureOperator();
                }
                // 括弧が終わった？
                else if (c == endOfParentheses)
                {
                    break;      // ループを抜けて括弧の外に出る
                }
                // 括弧の始まり？
                else if (c == '(')
                {
                    captureFunction();  // 関数名がある場合はキャプチャ
                    rpn.AddRange(parseInParentheses(queue, ')')); // 括弧の中身をすぐに処理する
                }
                else if (c == '\'' || c == '\"')
                {
                    captureId();  // 前の識別子を確定する
                    rpn.AddRange(parseInParentheses(queue, c)); // 括弧の中身をすぐに処理する
                }
                // 演算子オペレータ？
                else if (isKindOfOperator(c))
                {
                    // 前の識別子を確定する
                    captureId();

                    // 演算子を確定
                    var ope = parseOperator(c, queue);

                    // 保存されている演算子と自分を比較
                    if (reservedOperator.Count > 0)
                    {
                        while (reservedOperator.Count > 0)
                        {
                            var reserved = reservedOperator.Peek();
                            // 自分の優先度が高ければ保存されている演算子はそのままスキップ
                            if (OperatorPriorities.IndexOf(ope) < OperatorPriorities.IndexOf(reserved))
                            {
                                break;
                            }
                            // 自分の優先度が低ければ保存されていた演算子を実行
                            else
                            {
                                rpn.Add(reservedOperator.Pop());
                            }
                        }
                    }
                    // 自分を保存して右側を処理する
                    reservedOperator.Push(ope);
                }
                // まだ確定していない識別子
                else
                {
                    undeterminedId.Append(c);
                }
            }

            // 残っていた処理を実行
            captureId();
            captureOperator();

            return rpn;
        }

        public static List<string> Parse(string formula)
        {
            return parseInParentheses(new Queue<char>(formula.ToCharArray()), '\0');
        }
    }
}
