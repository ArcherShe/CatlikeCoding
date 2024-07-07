using System.Collections.Generic;

namespace BehaviorTree
{
    public class Sequence : Node
    {
        public Sequence() : base()
        {
        }

        public Sequence(List<Node> children) : base()
        {
            
        }

        public override NodeState Evaluate()
        {
            var anyChildIsRuning = false;
            foreach (var node in children)
            {
                switch (node.Evaluate())
                {
                    case NodeState.RUNNING:
                        anyChildIsRuning = true;
                        continue;
                    case NodeState.FAILURE:
                        state = NodeState.FAILURE;
                        return state;
                    default:
                        state = NodeState.SUCCESS;
                        return state;
                }
            }

            state = anyChildIsRuning ? NodeState.RUNNING : NodeState.SUCCESS;
            return state;
        }
    }
}