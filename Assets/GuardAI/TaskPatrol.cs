using UnityEngine;

namespace BehaviorTree
{
    public class TaskPatrol : Node
    {
        private Transform transform;
        private Transform[] wayPoints;
        private Animator animatior;

        private int currentWayPointIdx = 0;
        
        private float waitTime = 1f;
        private float waitCounter = 0f;
        private bool wating = false;
        
        
        private static readonly int Walking = Animator.StringToHash("walking");

        public TaskPatrol(Transform transform, Transform[] wayPoints)
        {
            this.transform = transform;
            this.wayPoints = wayPoints;
            this.animatior = this.transform.GetComponent<Animator>();
        }

        public override NodeState Evaluate()
        {
            if (wating)
            {
                waitCounter += Time.deltaTime;

                if (waitCounter >= waitTime)
                {
                    wating = false;
                    animatior.SetBool(Walking, true);
                }
            }

            var wp = wayPoints[currentWayPointIdx];
            if (Vector3.Distance(transform.position, wp.position) < 0.01f)
            {
                transform.position = wp.position;
                waitCounter = 0;
                wating = true;

                currentWayPointIdx = (currentWayPointIdx + 1) % wayPoints.Length;
            }
            else
            {
                transform.position =
                    Vector3.MoveTowards(transform.position, wp.position, GuardBT.speed * Time.deltaTime);
                transform.LookAt(wp.position);
            }

            state = NodeState.RUNNING;
            return state;
        }
    }
}