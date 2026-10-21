using System.Collections.Generic;
using System.IO;
using Core.Actors.Configs;
using Core.Actors.Modules.Abstract;
using Core.Actors.Modules.Behaviour.Configs;
using Core.Actors.Modules.Behaviour.Nodes.Configs;
using Core.Actors.Traits;
using Core.Actors;
using Game.Actors.Modules.Behaviour.Configs;
using Game.Actors.Modules.Collision.Configs;
using Game.Actors.Modules.Health.Configs;
using Game.Actors.Modules.Movement.Configs;
using Game.Actors.Modules.Visual.Configs;
using Game.Actors.Traits;
using Game.Installers;
using Game.Level;
using UI;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using Zenject;

namespace Editor
{
    public static class ZooWorldSceneBuilder
    {
        private const string Root = "Assets/Content";
        private const string LabelPrefabPath = Root + "/Prefabs/ActorLabel.prefab";
        private const string ObstaclePrefabPath = Root + "/Prefabs/Obstacle.prefab";
        private const string CatalogPath = Root + "/Configs/Actors/ActorCatalog.asset";
        private const string ScenePath = "Assets/Scenes/Zoo.unity";

        [MenuItem("ZooWorld/Build Demo Scene")]
        public static void Build()
        {
            AssetDatabase.DeleteAsset(Root);
            Directory.CreateDirectory(Root + "/Configs");
            Directory.CreateDirectory(Root + "/Prefabs");
            Directory.CreateDirectory(Root + "/Materials");

            PreyTrait prey = CreateAsset<PreyTrait>("Traits", "PreyTrait");
            PredatorTrait predator = CreateAsset<PredatorTrait>("Traits", "PredatorTrait");
            ActorModuleConfig[] common =
            {
                CreateAsset<MovementModuleConfig>("Modules", "MovementModule"),
                CreateAsset<VisualModuleConfig>("Modules", "VisualModule"),
                CreateAsset<CollisionModuleConfig>("Modules", "CollisionModule"),
                CreateAsset<HealthModuleConfig>("Modules", "HealthModule")
            };

            PhysicsMaterial animalMaterial = CreatePhysicsMaterial("Actor", 0.3f, 0.5f);
            ActorConfig frog = CreateFrog(animalMaterial, prey, common);
            ActorConfig snake = CreateSnake(animalMaterial, predator, common);

            ActorCatalog catalog = CreateAsset<ActorCatalog>("Actors", "ActorCatalog");
            SetList(catalog, "actors", frog, snake);

            ActorLabel labelPrefab = CreateLabelPrefab();
            CreateObstaclePrefab();

            AssetDatabase.SaveAssets();
            BuildScene(catalog, labelPrefab);
        }

        private static ActorConfig CreateFrog(PhysicsMaterial material, PreyTrait prey,
            ActorModuleConfig[] common)
        {
            Actor prefab = CreateActor("Frog", PrimitiveType.Sphere, new Vector3(0.8f, 0.8f, 0.8f),
                new Color(0.2f, 0.8f, 0.2f), material);

            JumpMovementConfig jump = CreateAsset<JumpMovementConfig>("Movement", "JumpMovement");
            MoveBehaviourConfig jumpAround = CreateAsset<MoveBehaviourConfig>("Behaviour/Frog", "FrogMove");
            SetRef(jumpAround, "movement", jump);

            JumpMovementConfig panicJump = CreateAsset<JumpMovementConfig>("Movement", "PanicJump");
            SetFloat(panicJump, "interval", 0f);
            SetFloat(panicJump, "distance", 4f);
            SetFloat(panicJump, "height", 0.6f);
            FleeBehaviourConfig flee = CreateAsset<FleeBehaviourConfig>("Behaviour/Frog", "Flee");
            SetRef(flee, "movement", panicJump);

            SelectorBehaviourConfig root = CreateAsset<SelectorBehaviourConfig>("Behaviour/Frog", "FrogRoot");
            SetList(root, "children", flee, jumpAround);
            BehaviourModuleConfig brain = CreateAsset<BehaviourModuleConfig>("Modules", "FrogBrain");
            SetRef(brain, "root", root);

            return CreateActorConfig("Frog", prefab, 1f, new ActorTrait[] { prey }, Combine(common, brain));
        }

        private static ActorConfig CreateSnake(PhysicsMaterial material, PredatorTrait predator,
            ActorModuleConfig[] common)
        {
            Actor prefab = CreateActor("Snake", PrimitiveType.Cube, new Vector3(0.5f, 0.4f, 1.6f),
                new Color(0.8f, 0.15f, 0.15f), material);

            LinearMovementConfig wander = CreateAsset<LinearMovementConfig>("Movement", "LinearMovement");
            LinearMovementConfig huntRun = CreateAsset<LinearMovementConfig>("Movement", "HuntRun");
            SetFloat(huntRun, "speed", 4.5f);
            HuntBehaviourConfig hunt = CreateAsset<HuntBehaviourConfig>("Behaviour/Snake", "Hunt");
            SetRef(hunt, "movement", huntRun);

            MoveBehaviourConfig stroll = CreateAsset<MoveBehaviourConfig>("Behaviour/Snake", "Stroll");
            SetRef(stroll, "movement", wander);
            SetFloat(stroll, "duration", 3f);
            WaitBehaviourConfig rest = CreateAsset<WaitBehaviourConfig>("Behaviour/Snake", "Rest");
            SetFloat(rest, "seconds", 1.5f);
            SequenceBehaviourConfig strollAndRest = CreateAsset<SequenceBehaviourConfig>("Behaviour/Snake", "StrollAndRest");
            SetList(strollAndRest, "children", stroll, rest);

            SelectorBehaviourConfig root = CreateAsset<SelectorBehaviourConfig>("Behaviour/Snake", "SnakeRoot");
            SetList(root, "children", hunt, strollAndRest);
            BehaviourModuleConfig brain = CreateAsset<BehaviourModuleConfig>("Modules", "SnakeBrain");
            SetRef(brain, "root", root);

            return CreateActorConfig("Snake", prefab, 2f, new ActorTrait[] { predator }, Combine(common, brain));
        }

        private static ActorModuleConfig[] Combine(ActorModuleConfig[] common, params ActorModuleConfig[] own)
        {
            List<ActorModuleConfig> all = new List<ActorModuleConfig>(common);
            all.AddRange(own);
            return all.ToArray();
        }

        private static void BuildScene(ActorCatalog catalog, ActorLabel labelPrefab)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            catalog = AssetDatabase.LoadAssetAtPath<ActorCatalog>(CatalogPath);
            labelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LabelPrefabPath).GetComponent<ActorLabel>();

            Camera camera = Camera.main;
            camera.orthographic = false;
            camera.fieldOfView = 40f;
            Vector3 cameraPosition = new Vector3(0f, 19f, -6f);
            camera.transform.SetPositionAndRotation(cameraPosition, Quaternion.LookRotation(-cameraPosition));
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.05f, 0.2f, 0.05f);

            PhysicsMaterial groundMaterial = CreatePhysicsMaterial("Ground", 0.6f, 0f);
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.position = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(60f, 1f, 60f);
            floor.GetComponent<Collider>().sharedMaterial = groundMaterial;
            floor.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Floor", new Color(0.1f, 0.5f, 0.1f));

            GameObject installers = new GameObject("Installers");
            ZooInstaller installer = installers.AddComponent<ZooInstaller>();
            SetRef(installer, "worldCamera", camera);
            SetRef(installer, "catalog", catalog);
            SetRef(installer, "obstaclePrefab",
                AssetDatabase.LoadAssetAtPath<GameObject>(ObstaclePrefabPath).GetComponent<Obstacle>());

            SceneContext context = new GameObject("SceneContext").AddComponent<SceneContext>();
            context.Installers = new MonoInstaller[] { installer };

            CreateUi(labelPrefab);

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
        }

        private static void CreateUi(ActorLabel labelPrefab)
        {
            GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            Text prey = CreateText(canvasObject.transform, "PreyText", new Vector2(-30f, -30f));
            Text predators = CreateText(canvasObject.transform, "PredatorsText", new Vector2(-30f, -80f));

            StatsView view = canvasObject.AddComponent<StatsView>();
            SetRef(view, "preyText", prey);
            SetRef(view, "predatorsText", predators);

            RectTransform layer = new GameObject("TastyLayer", typeof(RectTransform)).GetComponent<RectTransform>();
            layer.SetParent(canvasObject.transform, false);
            layer.anchorMin = Vector2.zero;
            layer.anchorMax = Vector2.one;
            layer.offsetMin = layer.offsetMax = Vector2.zero;

            TastyView tastyView = layer.gameObject.AddComponent<TastyView>();
            SetRef(tastyView, "labelPrefab", labelPrefab);

            RectTransform statusLayer = new GameObject("StatusLayer", typeof(RectTransform)).GetComponent<RectTransform>();
            statusLayer.SetParent(canvasObject.transform, false);
            statusLayer.anchorMin = Vector2.zero;
            statusLayer.anchorMax = Vector2.one;
            statusLayer.offsetMin = statusLayer.offsetMax = Vector2.zero;

            StatusLabelsView statusView = statusLayer.gameObject.AddComponent<StatusLabelsView>();
            SetRef(statusView, "labelPrefab", labelPrefab);
        }

        private static Text CreateText(Transform parent, string name, Vector2 position)
        {
            Text text = new GameObject(name, typeof(Text)).GetComponent<Text>();
            text.transform.SetParent(parent, false);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 40;
            text.alignment = TextAnchor.UpperRight;
            text.color = Color.white;
            text.raycastTarget = false;

            RectTransform rect = text.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one;
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(600f, 50f);
            return text;
        }

        private static ActorLabel CreateLabelPrefab()
        {
            GameObject root = new GameObject("ActorLabel", typeof(RectTransform), typeof(Text), typeof(ActorLabel));
            RectTransform rect = (RectTransform)root.transform;
            rect.sizeDelta = new Vector2(300f, 80f);

            Text text = root.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = "Tasty!";
            text.fontSize = 48;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.yellow;
            text.raycastTarget = false;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, LabelPrefabPath);
            Object.DestroyImmediate(root);
            return prefab.GetComponent<ActorLabel>();
        }

        private static Actor CreateActor(string name, PrimitiveType shape, Vector3 scale, Color color,
            PhysicsMaterial physicsMaterial)
        {
            GameObject go = GameObject.CreatePrimitive(shape);
            go.name = name;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name, color);
            go.GetComponent<Collider>().sharedMaterial = physicsMaterial;
            Rigidbody body = go.AddComponent<Rigidbody>();
            body.angularDamping = 5f;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            go.AddComponent<Actor>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{Root}/Prefabs/{name}.prefab");
            Object.DestroyImmediate(go);
            return prefab.GetComponent<Actor>();
        }

        private static ActorConfig CreateActorConfig(string name, Actor prefab, float mass, ActorTrait[] traits,
            ActorModuleConfig[] modules)
        {
            ActorConfig config = CreateAsset<ActorConfig>("Actors", name);
            SerializedObject so = new SerializedObject(config);
            so.FindProperty("prefab").objectReferenceValue = prefab;
            so.FindProperty("mass").floatValue = mass;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssetIfDirty(config);
            SetList(config, "traits", traits);
            SetList(config, "modules", modules);
            return config;
        }

        private static void CreateObstaclePrefab()
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Obstacle";
            go.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Obstacle", new Color(0.45f, 0.35f, 0.25f));
            go.AddComponent<Obstacle>();
            PrefabUtility.SaveAsPrefabAsset(go, ObstaclePrefabPath);
            Object.DestroyImmediate(go);
        }

        private static T CreateAsset<T>(string folder, string name) where T : ScriptableObject
        {
            string directory = $"{Root}/Configs/{folder}";
            Directory.CreateDirectory(directory);
            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, $"{directory}/{name}.asset");
            return asset;
        }

        private static Material CreateMaterial(string name, Color color)
        {
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", color);
            AssetDatabase.CreateAsset(material, $"{Root}/Materials/{name}.mat");
            return material;
        }

        private static PhysicsMaterial CreatePhysicsMaterial(string name, float friction, float bounciness)
        {
            PhysicsMaterial material = new PhysicsMaterial(name) { dynamicFriction = friction, staticFriction = friction, bounciness = bounciness };
            AssetDatabase.CreateAsset(material, $"{Root}/Materials/{name}.physicMaterial");
            return material;
        }

        private static void SetRef(Object target, string field, Object value)
        {
            SerializedObject so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssetIfDirty(target);
        }

        private static void SetBool(Object target, string field, bool value)
        {
            SerializedObject so = new SerializedObject(target);
            so.FindProperty(field).boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssetIfDirty(target);
        }

        private static void SetFloat(Object target, string field, float value)
        {
            SerializedObject so = new SerializedObject(target);
            so.FindProperty(field).floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssetIfDirty(target);
        }

        private static void SetList(Object target, string field, params Object[] values)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty list = so.FindProperty(field);
            list.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssetIfDirty(target);
        }
    }
}
