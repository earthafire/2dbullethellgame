using ProjectDawn.Navigation.Hybrid;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using BulletHell.Navigation;

namespace BulletHell.Navigation.Editor
{
    // Adds the ProjectDawn.Navigation authoring components to enemy prefabs,
    // tuned per Docs/NAVIGATION_MIGRATION.md §6. Safe to re-run: existing
    // components are left untouched, only missing ones are added.
    public static class AgentAuthoringSetupTool
    {
        const string TestPrefabPath = "Assets/Resources/Prefabs/Enemies/Ground Enemies/Green Cube.prefab";

        [MenuItem("Tools/Navigation/Setup Crowd Surface In Open Scene")]
        static void SetupCrowdSurfaceInOpenScene()
        {
            var wallsGenerator = FindWallsLevelGenerator();
            if (wallsGenerator == null)
            {
                Debug.LogError("[AgentAuthoringSetupTool] Couldn't find a LevelGenerator whose mapSetting is 'Walls' in the open scene.");
                return;
            }
            if (wallsGenerator.tilemap == null)
            {
                Debug.LogError("[AgentAuthoringSetupTool] The Walls LevelGenerator has no Tilemap assigned.");
                return;
            }

            if (Object.FindAnyObjectByType<CrowdSurfaceAuthoring>() != null)
            {
                Debug.LogWarning("[AgentAuthoringSetupTool] A CrowdSurfaceAuthoring already exists in the scene - not creating another one.");
                return;
            }

            var cellSize = wallsGenerator.tilemap.cellSize;
            var worldSize = new float2(wallsGenerator.width * cellSize.x, wallsGenerator.height * cellSize.y);

            var surfaceGO = new GameObject("Crowd Surface (Walls)");
            Undo.RegisterCreatedObjectUndo(surfaceGO, "Setup Crowd Surface");
            surfaceGO.transform.position = Vector3.zero;
            var surface = surfaceGO.AddComponent<CrowdSurfaceAuthoring>();

            var surfaceSO = new SerializedObject(surface);
            surfaceSO.FindProperty("m_Width").intValue = wallsGenerator.width;
            surfaceSO.FindProperty("m_Height").intValue = wallsGenerator.height;
            var sizeProp = surfaceSO.FindProperty("m_Size");
            sizeProp.FindPropertyRelative("x").floatValue = worldSize.x;
            sizeProp.FindPropertyRelative("y").floatValue = worldSize.y;
            surfaceSO.ApplyModifiedPropertiesWithoutUndo();

            var groupGO = new GameObject("Crowd Group (Enemies)");
            Undo.RegisterCreatedObjectUndo(groupGO, "Setup Crowd Surface");
            var group = groupGO.AddComponent<CrowdGroupAuthoring>();
            var groupSO = new SerializedObject(group);
            groupSO.FindProperty("m_Surface").objectReferenceValue = surface;
            groupSO.ApplyModifiedPropertiesWithoutUndo();
            // Publishes itself to GlobalReferences.crowdGroup at runtime - enemy prefabs
            // can't hold a direct serialized reference to this scene object, so they
            // read it from there instead (see NavTestSeeker.Start / CrowdGroupRegistrar).
            groupGO.AddComponent<CrowdGroupRegistrar>();

            var bakerGO = new GameObject("Crowd Wall Baker");
            Undo.RegisterCreatedObjectUndo(bakerGO, "Setup Crowd Surface");
            var baker = bakerGO.AddComponent<CrowdWallBaker>();
            var bakerSO = new SerializedObject(baker);
            bakerSO.FindProperty("_wallsGenerator").objectReferenceValue = wallsGenerator;
            bakerSO.FindProperty("_wallsTilemap").objectReferenceValue = wallsGenerator.tilemap;
            bakerSO.FindProperty("_surface").objectReferenceValue = surface;
            bakerSO.ApplyModifiedPropertiesWithoutUndo();

            Selection.objects = new Object[] { surfaceGO, groupGO, bakerGO };
            Debug.Log("[AgentAuthoringSetupTool] Crowd surface set up (Width=" + wallsGenerator.width + ", Height=" + wallsGenerator.height +
                ", Size=" + worldSize + "). Re-run 'Add Agent Navigation To Selected Prefabs' / 'Spawn Nav Test Enemy In Open Scene' " +
                "to pick up AgentCrowdPathingAuthoring pointed at 'Crowd Group (Enemies)'.");
        }

        static LevelGenerator FindWallsLevelGenerator()
        {
            foreach (var gen in Object.FindObjectsByType<LevelGenerator>(FindObjectsSortMode.None))
            {
                if (gen.mapSetting != null && gen.mapSetting.name == "Walls")
                    return gen;
            }
            return null;
        }

        [MenuItem("Tools/Navigation/Spawn Nav Test Enemy In Open Scene")]
        static void SpawnNavTestEnemyInOpenScene()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TestPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[AgentAuthoringSetupTool] Couldn't find prefab at {TestPrefabPath}.");
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = "NAV TEST - Green Cube (delete me)";
            Undo.RegisterCreatedObjectUndo(instance, "Spawn Nav Test Enemy");

            // The real Enemy.cs still runs Vector3.MoveTowards in FixedUpdate - disable
            // it on this instance so it doesn't fight the DOTS-driven position (Phase 2
            // is only testing the movement bridge in isolation, not the full enemy).
            var enemy = instance.GetComponent<Enemy>();
            if (enemy != null)
                enemy.enabled = false;

            ApplyAgentSetup(instance);

            if (instance.GetComponent<NavTestSeeker>() == null)
                Undo.AddComponent<NavTestSeeker>(instance);

            Selection.activeGameObject = instance;
            EditorGUIUtility.PingObject(instance);
            Debug.Log("[AgentAuthoringSetupTool] Spawned nav test enemy. Enter Play Mode and watch it seek toward GlobalReferences.player. Delete the GameObject when done.");
        }

        [MenuItem("Tools/Navigation/Add Agent Navigation To Selected Prefabs")]
        static void AddAgentNavigationToSelectedPrefabs()
        {
            var selected = Selection.GetFiltered<GameObject>(SelectionMode.Assets);
            if (selected.Length == 0)
            {
                Debug.LogWarning("[AgentAuthoringSetupTool] No prefab assets selected.");
                return;
            }

            int changed = 0;
            foreach (var asset in selected)
            {
                string path = AssetDatabase.GetAssetPath(asset);
                if (string.IsNullOrEmpty(path) || !path.EndsWith(".prefab"))
                    continue;

                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    if (ApplyAgentSetup(root))
                    {
                        PrefabUtility.SaveAsPrefabAsset(root, path);
                        changed++;
                        Debug.Log($"[AgentAuthoringSetupTool] Updated {path}");
                    }
                    else
                    {
                        Debug.Log($"[AgentAuthoringSetupTool] {path} already up to date, skipped.");
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            Debug.Log($"[AgentAuthoringSetupTool] Done. {changed}/{selected.Length} prefab(s) updated.");
        }

        static bool ApplyAgentSetup(GameObject root)
        {
            bool changedAny = false;

            var agent = root.GetComponent<AgentAuthoring>();
            if (agent == null)
            {
                agent = root.AddComponent<AgentAuthoring>();
                changedAny = true;

                // Enemies must fully close distance and stay pressed against the
                // player for the contact-damage trigger to keep firing - not brake
                // and stop short like an RTS unit reaching a waypoint (the package's
                // own defaults are StoppingDistance=0.1/AutoBreaking=true, which
                // would break the damage trigger). Matches Age-of-Sprites' Slime.
                //
                // AngularSpeed=0 keeps the sprite upright: AgentLocomotionSystem always
                // slerps LocalTransform.Rotation toward the facing direction using
                // AngularSpeed as the interpolation rate, but slerp(a, b, 0) == a, so
                // this permanently disables rotation with no effect on movement itself.
                // Our 2D sprites use a separate horizontal-flip (Enemy.FixedUpdate),
                // not transform rotation.
                var agentSO = new SerializedObject(agent);
                agentSO.FindProperty("StoppingDistance").floatValue = 0f;
                agentSO.FindProperty("AutoBreaking").boolValue = false;
                agentSO.FindProperty("AngularSpeed").floatValue = 0f;
                agentSO.ApplyModifiedPropertiesWithoutUndo();
            }

            var circle = root.GetComponent<CircleCollider2D>();
            float radius = circle != null ? circle.radius : 0.5f;

            if (root.GetComponent<AgentCircleShapeAuthoring>() == null)
            {
                var shape = root.AddComponent<AgentCircleShapeAuthoring>();
                changedAny = true;

                var shapeSO = new SerializedObject(shape);
                shapeSO.FindProperty("Radius").floatValue = radius;
                shapeSO.ApplyModifiedPropertiesWithoutUndo();
            }

            if (root.GetComponent<AgentColliderAuthoring>() == null)
            {
                root.AddComponent<AgentColliderAuthoring>();
                changedAny = true;
            }

            if (root.GetComponent<AgentAvoidAuthoring>() == null)
            {
                var avoid = root.AddComponent<AgentAvoidAuthoring>();
                changedAny = true;

                // Starting point mirrors Age-of-Sprites' Slime (agent radius 0.2 ->
                // sonar radius 0.3, i.e. ~1.5x), scaled to this enemy's own collider
                // radius instead of hardcoding Slime's absolute numbers. Angle/MaxAngle
                // match Slime; Mode/UseWalls left at package defaults (Mode already
                // defaults to IgnoreBehindAgents = Slime's Mode=3, UseWalls already
                // defaults to false). Re-tune in Phase 3 for an omnidirectional mob
                // instead of Slime's RTS unit-navigating-around-units context - see
                // Docs/NAVIGATION_MIGRATION.md §6.
                var avoidSO = new SerializedObject(avoid);
                avoidSO.FindProperty("Radius").floatValue = radius * 1.5f;
                avoidSO.FindProperty("Angle").floatValue = 230f;
                avoidSO.FindProperty("MaxAngle").floatValue = 300f;
                avoidSO.FindProperty("BlockedStop").boolValue = true;
                avoidSO.ApplyModifiedPropertiesWithoutUndo();
            }

            // Routes around walls via Continuum Crowds instead of the plain straight-line
            // seek (§4a). Deliberately does NOT set m_Group here: this runs on both
            // prefab assets and scene instances, and prefab assets can't hold a direct
            // serialized reference to a scene object (Unity strips it on save) - the
            // group gets wired up at runtime instead, from GlobalReferences.crowdGroup
            // (see CrowdGroupRegistrar, NavTestSeeker.Start). Harmless no-op if no crowd
            // group ever gets registered - the package docs: "Agents in null crowds
            // group will skip pathing."
            if (root.GetComponent<AgentCrowdPathingAuthoring>() == null)
            {
                root.AddComponent<AgentCrowdPathingAuthoring>();
                changedAny = true;
            }

            return changedAny;
        }
    }
}
