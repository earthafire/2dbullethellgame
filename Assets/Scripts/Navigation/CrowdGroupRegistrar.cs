using ProjectDawn.Navigation.Hybrid;
using UnityEngine;

namespace BulletHell.Navigation
{
    // Publishes this scene's CrowdGroupAuthoring to GlobalReferences.crowdGroup so
    // enemy prefabs can wire AgentCrowdPathingAuthoring.Group at runtime without
    // holding a direct serialized reference to a scene object (which prefab assets
    // can't do - Unity strips scene-object references from prefabs on save).
    [RequireComponent(typeof(CrowdGroupAuthoring))]
    public class CrowdGroupRegistrar : MonoBehaviour
    {
        void Awake() => GlobalReferences.crowdGroup = GetComponent<CrowdGroupAuthoring>();
    }
}
