using ProjectDawn.Navigation.Hybrid;
using UnityEngine;

namespace BulletHell.Navigation
{
    // Phase 2 smoke-test harness only (see Docs/NAVIGATION_MIGRATION.md).
    // Continuously seeks toward GlobalReferences.player via DOTS. Delete once
    // the real Enemy.cs integration (Phase 4) replaces Vector3.MoveTowards.
    [RequireComponent(typeof(AgentAuthoring))]
    public class NavTestSeeker : MonoBehaviour
    {
        // The package default (Speed=3.5) is much faster than any real enemy - e.g.
        // Green Cube's Attributes asset has moveSpeed=0.15. Phase 4 will drive this
        // from Attributes.GetAttribute(Attribute.moveSpeed) per enemy instead; this is
        // just a sane placeholder so the isolated smoke test feels representative.
        public float TestSpeed = 2f;

        AgentAuthoring _agent;

        void Awake() => _agent = GetComponent<AgentAuthoring>();

        void Start()
        {
            var locomotion = _agent.EntityLocomotion;
            locomotion.Speed = TestSpeed;
            _agent.EntityLocomotion = locomotion;

            // Prefab assets can't hold a direct reference to the scene's crowd group
            // (see CrowdGroupRegistrar), so wire it up here instead, once it's known
            // to exist - AgentCrowdPathingAuthoring.OnEnable() has already run by Start().
            var pathing = GetComponent<AgentCrowdPathingAuthoring>();
            if (pathing != null && GlobalReferences.crowdGroup != null)
                pathing.Group = GlobalReferences.crowdGroup;
        }

        void Update()
        {
            GameObject player = GlobalReferences.player;
            if (player != null)
                _agent.SetDestinationDeferred(player.transform.position);
        }
    }
}
