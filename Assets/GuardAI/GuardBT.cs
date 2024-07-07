using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public class GuardBT : Tree
    {
        public Transform[] wayPoints;

        public static float speed;
        public static float fovRange = 6f;
        public static float attackRange = 6f;
        
        protected override Node SetupTree()
        {
            var root = new Selector(new List<Node>
            {
                new Sequence(new List<Node>()
                {
                    new CheckEnemyInFOVRange(transform), 
                    new TaskGoToTarget(transform)
                }),
                new TaskPatrol(transform, wayPoints),
            });

            return root;
        }
    }
}