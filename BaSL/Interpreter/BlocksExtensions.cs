using System.Collections.Generic;
using BaSL.Syntax;

namespace BaSL.Interpreter;

internal static class BlocksExtensions
{

    extension(Stack<(KeywordSegment Segment, bool Skip)> blocks)
    {

        public bool Skip(int code) => blocks.Skip(code == 0);

        public bool Skip(bool @true)
        {
            blocks.Skip(@true ? KeywordSegment.Then : KeywordSegment.Else);
            return true;
        }

        private void Skip(KeywordSegment to) => blocks.Push((to, true));

        public bool Transition(KeywordSegment segment, bool skip)
        {
            blocks.Pop();
            blocks.Push((segment, skip));
            return !skip;
        }

    }

}
