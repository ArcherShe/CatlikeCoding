using UnityEngine;

namespace BehaviorTree
{
    public class CheckEnemyInFOVRange : Node
    {
        private static int enmyLayer = 1 << 6;
        
        private Transform transform;
        private Animator animator;

        public CheckEnemyInFOVRange(Transform transform)
        {
            this.transform = transform;
            this.animator = this.transform.GetComponent<Animator>();
        }

        public override NodeState Evaluate()
        {
            object t = GetData("target");
            if (t == null)
            {
                var colliders = Physics.OverlapSphere(transform.position, GuardBT.fovRange, enmyLayer);
                if (colliders.Length > 0)
                {
                    parent.parent.SetData("target", colliders[0].transform);
                    animator.SetBool("walking", true);
                    state = NodeState.SUCCESS;
                    return state;
                }
                
                state = NodeState.FAILURE;
                return state;
            }

            state = NodeState.SUCCESS;
            return state;
        }
    }
}