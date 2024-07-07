using UnityEngine;

namespace BehaviorTree
{
    public class TaskGoToTarget : Node
    {
        public Transform transform;
        
        public TaskGoToTarget(Transform transform)
        {
            this.transform = transform;
        }
        
        public override NodeState Evaluate()
        {
            var target = (Transform)GetData("target");

            if (Vector3.Distance(transform.position, target.position) > 0.01f)
            {
                transform.position =
                    Vector3.MoveTowards(transform.position, target.position, GuardBT.speed * Time.deltaTime);
                this.transform.LookAt(target.transform);
            }

            state = NodeState.RUNNING;
            return state;
        }
    }
}