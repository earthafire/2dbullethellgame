using ProjectDawn.Navigation;
using ProjectDawn.Navigation.Hybrid;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BulletHell.Navigation
{
    // Feeds the procedurally-generated Walls tilemap into the Continuum Crowds
    // obstacle field so enemies route around walls instead of pushing into them
    // (see Docs/NAVIGATION_MIGRATION.md §4a). Reads whatever the tilemap actually
    // rendered (via Tilemap.HasTile), rather than duplicating LevelGenerator's
    // array logic, so it can't drift out of sync with what's on screen.
    //
    // Stamps obstacles directly into the live CrowdWorld via SplatObstacleQuad
    // instead of pre-baking a CrowdData asset - CrowdSurfaceSystem only reads
    // CrowdData once, on the frame the surface entity is first created, so a
    // pre-bake approach would race LevelGenerator.GenerateMap() (called from
    // gameSupervisorController.Start()). Splatting into the already-live world
    // has no such ordering requirement - it can happen any time after both the
    // surface entity and the tilemap exist.
    public class CrowdWallBaker : MonoBehaviour
    {
        [SerializeField] LevelGenerator _wallsGenerator;
        [SerializeField] Tilemap _wallsTilemap;
        [SerializeField] CrowdSurfaceAuthoring _surface;

        void OnEnable()
        {
            if (_wallsGenerator != null)
                _wallsGenerator.OnGenerated.AddListener(BakeWhenReady);
        }

        void OnDisable()
        {
            if (_wallsGenerator != null)
                _wallsGenerator.OnGenerated.RemoveListener(BakeWhenReady);
        }

        void BakeWhenReady() => StartCoroutine(BakeWhenReadyRoutine());

        System.Collections.IEnumerator BakeWhenReadyRoutine()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            var entity = _surface.GetOrCreateEntity();

            // CrowdSurfaceWorld is added by CrowdSurfaceSystem the first time it
            // sees the surface entity, which may be a frame or two after Awake.
            int guardFrames = 0;
            while (!world.EntityManager.HasComponent<CrowdSurfaceWorld>(entity))
            {
                if (++guardFrames > 120)
                {
                    Debug.LogError("[CrowdWallBaker] Timed out waiting for CrowdSurfaceWorld - is CrowdSurfaceAuthoring in the scene and enabled?");
                    yield break;
                }
                yield return null;
            }

            var crowdWorld = world.EntityManager.GetComponentData<CrowdSurfaceWorld>(entity).World;

            int stamped = 0;
            var bounds = _wallsTilemap.cellBounds;
            var cellSize = _wallsTilemap.cellSize;
            foreach (var cell in bounds.allPositionsWithin)
            {
                if (!_wallsTilemap.HasTile(cell))
                    continue;

                float3 worldCenter = _wallsTilemap.GetCellCenterWorld(cell);
                crowdWorld.SplatObstacleQuad(worldCenter, new float3(cellSize.x, cellSize.y, 0), 1);
                stamped++;
            }

            Debug.Log($"[CrowdWallBaker] Stamped {stamped} wall cell(s) into the crowd obstacle field.");
        }
    }
}
