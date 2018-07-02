using System.CodeDom;

namespace Ascension.Compiler
{
    public class DomBlock
    {
        private int tmpVar;
        private readonly string prefix = "";
        private readonly CodeStatementCollection stmts;

        public DomBlock(CodeStatementCollection stmts, string prefix)
        {
            this.stmts = stmts;
            this.prefix = prefix;
        }

        public DomBlock(CodeStatementCollection stmts)
            : this(stmts, "")
        {
        }

        public CodeStatementCollection Stmts
        {
            get { return stmts; }
        }

        public void Add(CodeExpression expression)
        {
            Stmts.Add(expression);
        }

        public void Add(CodeStatement statement)
        {
            Stmts.Add(statement);
        }

        public string TempVar()
        {
            return prefix + "tmp" + (tmpVar++);
        }
    }
}