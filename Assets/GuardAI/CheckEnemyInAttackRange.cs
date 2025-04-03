using UnityEngine;

namespace BehaviorTree
{
    public class CheckEnemyInAttackRange : Node
    {
        // private static int emenyLayerMask = 1 << 6;

        private Transform transform;
        public Animator animator;

        public override NodeState Evaluate()
        {
            var t = GetData("target");
            if (t == null)
            {
                state = NodeState.FAILURE;
                return state;
            }

            var target = (Transform)t;
            if (Vector3.Distance(transform.position, target.position) <= GuardBT.attackRange)
            {
                animator.SetBool("Attacking", true);
                animator.SetBool("walking", false);

                state = NodeState.SUCCESS;
                return state;
            }
            
            state = NodeState.FAILURE;
            return state;
        }
    }
}