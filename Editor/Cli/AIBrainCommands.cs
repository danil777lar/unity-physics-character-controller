using System;
using System.Collections.Generic;
using Larje.Character.AI;
using Larje.Core.Editor.Cli;
using Newtonsoft.Json;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Larje.Character.Editor.Cli
{
    public static class AIBrainCommands
    {
        private const string GROUP = "larje.ai";
        private const string LABEL_PROPERTY = "<Label>k__BackingField";
        private const string LIST_DESCRIPTION = "Lists Larje AI building blocks: every AIAction and AIDecision type with its serialized fields (name and type). Use the type names in larje.ai edit.";
        private const string DESCRIBE_DESCRIPTION = "Describes a Larje AIBrain prefab (asset path) as JSON: states in order (the first is initial), their actions (type, label, fields) and transitions (decision type, label, fields, true/false target states).";
        private const string EDIT_DESCRIPTION = "Edits a Larje AIBrain prefab with a JSON array of operations (inline --ops or a file path --file) applied in order and saved once. Ops: {op:add_state,state}, {op:remove_state,state}, {op:add_action,state,type,label}, {op:add_transition,state,type,label,trueState,falseState} where type is an AIDecision type, {op:set_field,label,property,value} sets a serialized field on the action/decision with that label (numbers invariant, vectors '(x,y,z)', enums by name, assets by path).";

        [CliGroupCommand(GROUP, LIST_DESCRIPTION, Tags = new[] { "larje", "assets" })]
        public static object ListTypes()
        {
            return new
            {
                actions = DescribeTypes(TypeCache.GetTypesDerivedFrom<AIAction>()),
                decisions = DescribeTypes(TypeCache.GetTypesDerivedFrom<AIDecision>())
            };
        }

        [CliGroupCommand(GROUP, DESCRIBE_DESCRIPTION, Tags = new[] { "larje", "assets" })]
        public static object Describe(
            [CliArg("prefab", "AIBrain prefab asset path", Required = true)] string prefab)
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(prefab);
            AIBrain brain = asset != null ? asset.GetComponentInChildren<AIBrain>(true) : null;
            if (brain == null)
            {
                throw new ArgumentException($"No AIBrain prefab at '{prefab}'");
            }

            return DescribeBrain(brain);
        }

        [CliGroupCommand(GROUP, EDIT_DESCRIPTION, Tags = new[] { "larje", "assets" })]
        public static object Edit(
            [CliArg("prefab", "AIBrain prefab asset path", Required = true)] string prefab,
            [CliArg("ops", "Operations as a JSON array")] string ops = null,
            [CliArg("file", "Path to a file with the operations JSON array, relative to the project root")] string file = null)
        {
            List<Operation> operations = ParseOperations(CliText.Resolve(ops, file, "ops"));
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefab) == null)
            {
                throw new ArgumentException($"Prefab not found at '{prefab}'");
            }

            GameObject root = PrefabUtility.LoadPrefabContents(prefab);
            try
            {
                AIBrain brain = root.GetComponentInChildren<AIBrain>(true);
                if (brain == null)
                {
                    throw new ArgumentException("Prefab has no AIBrain");
                }

                int applied = 0;
                foreach (Operation operation in operations)
                {
                    string error = Apply(brain, operation);
                    if (error != null)
                    {
                        throw new InvalidOperationException($"Operation {applied + 1} ({operation.Op}): {error}. Nothing was saved.");
                    }

                    applied++;
                }

                PrefabUtility.SaveAsPrefabAsset(root, prefab);
                return new { applied, brain = DescribeBrain(brain) };
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static List<Operation> ParseOperations(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<List<Operation>>(json) ?? new List<Operation>();
            }
            catch (JsonException exception)
            {
                throw new ArgumentException("Invalid operations JSON: " + exception.Message);
            }
        }

        private static string Apply(AIBrain brain, Operation operation)
        {
            switch (operation.Op)
            {
                case "add_state":
                    if (FindState(brain, operation.State) != null)
                    {
                        return $"state '{operation.State}' already exists";
                    }

                    brain.states.Add(new AIState { StateName = operation.State, Actions = new List<AIAction>(), Transitions = new List<AITransition>() });
                    return null;
                case "remove_state":
                    AIState removed = FindState(brain, operation.State);
                    if (removed == null)
                    {
                        return $"state '{operation.State}' not found";
                    }

                    brain.states.Remove(removed);
                    return null;
                case "add_action":
                    return AddAction(brain, operation);
                case "add_transition":
                    return AddTransition(brain, operation);
                case "set_field":
                    return SetField(brain, operation);
                default:
                    return "unknown op";
            }
        }

        private static string AddAction(AIBrain brain, Operation operation)
        {
            AIState state = FindState(brain, operation.State);
            Type type = ResolveType<AIAction>(operation.Type);
            if (state == null || type == null)
            {
                return state == null ? $"state '{operation.State}' not found" : $"action type '{operation.Type}' not found";
            }

            AIAction action = (AIAction)brain.gameObject.AddComponent(type);
            SetLabel(action, operation.Label);
            state.Actions ??= new List<AIAction>();
            state.Actions.Add(action);
            return null;
        }

        private static string AddTransition(AIBrain brain, Operation operation)
        {
            AIState state = FindState(brain, operation.State);
            Type type = ResolveType<AIDecision>(operation.Type);
            if (state == null || type == null)
            {
                return state == null ? $"state '{operation.State}' not found" : $"decision type '{operation.Type}' not found";
            }

            AIDecision decision = (AIDecision)brain.gameObject.AddComponent(type);
            SetLabel(decision, operation.Label);
            state.Transitions ??= new List<AITransition>();
            state.Transitions.Add(new AITransition { Decision = decision, TrueState = operation.TrueState, FalseState = operation.FalseState });
            return null;
        }

        private static string SetField(AIBrain brain, Operation operation)
        {
            Component target = FindByLabel(brain, operation.Label);
            if (target == null)
            {
                return $"no action or decision labeled '{operation.Label}'";
            }

            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(operation.Property) ?? serialized.FindProperty($"<{operation.Property}>k__BackingField");
            if (property == null)
            {
                return $"property '{operation.Property}' not found on {target.GetType().Name}";
            }

            if (!SerializedValueWriter.TryWrite(property, operation.Value, out string error))
            {
                return error;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return null;
        }

        private static void SetLabel(Component component, string label)
        {
            SerializedObject serialized = new SerializedObject(component);
            SerializedProperty property = serialized.FindProperty(LABEL_PROPERTY);
            if (property != null)
            {
                property.stringValue = string.IsNullOrEmpty(label) ? component.GetType().Name : label;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static Component FindByLabel(AIBrain brain, string label)
        {
            foreach (MonoBehaviour behaviour in brain.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour is AIAction || behaviour is AIDecision)
                {
                    SerializedProperty property = new SerializedObject(behaviour).FindProperty(LABEL_PROPERTY);
                    if (property != null && property.stringValue == label)
                    {
                        return behaviour;
                    }
                }
            }

            return null;
        }

        private static AIState FindState(AIBrain brain, string name)
        {
            brain.states ??= new List<AIState>();
            return brain.states.Find(x => x.StateName == name);
        }

        private static Type ResolveType<T>(string name)
        {
            foreach (Type type in TypeCache.GetTypesDerivedFrom<T>())
            {
                if (!type.IsAbstract && (type.Name == name || type.FullName == name))
                {
                    return type;
                }
            }

            return null;
        }

        private static object DescribeBrain(AIBrain brain)
        {
            List<object> states = new List<object>();
            foreach (AIState state in brain.states ?? new List<AIState>())
            {
                List<object> actions = new List<object>();
                foreach (AIAction action in state.Actions ?? new List<AIAction>())
                {
                    actions.Add(DescribeComponent(action));
                }

                List<object> transitions = new List<object>();
                foreach (AITransition transition in state.Transitions ?? new List<AITransition>())
                {
                    transitions.Add(new { decision = DescribeComponent(transition.Decision), trueState = transition.TrueState, falseState = transition.FalseState });
                }

                states.Add(new { name = state.StateName, actions, transitions });
            }

            return new { brain = brain.name, states };
        }

        private static object DescribeComponent(Object component)
        {
            if (component == null)
            {
                return null;
            }

            Dictionary<string, string> fields = new Dictionary<string, string>();
            SerializedObject serialized = new SerializedObject(component);
            SerializedProperty iterator = serialized.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.name != "m_Script")
                {
                    fields[CleanName(iterator.name)] = SerializedValueWriter.Read(iterator);
                }
            }

            return new { type = component.GetType().Name, fields };
        }

        private static string CleanName(string name)
        {
            return name.StartsWith("<") && name.EndsWith(">k__BackingField") ? name.Substring(1, name.Length - 17) : name;
        }

        private static List<object> DescribeTypes(TypeCache.TypeCollection types)
        {
            List<object> result = new List<object>();
            foreach (Type type in types)
            {
                if (!type.IsAbstract)
                {
                    result.Add(new { type = type.Name, fields = DescribeFields(type) });
                }
            }

            return result;
        }

        private static List<string> DescribeFields(Type type)
        {
            List<string> fields = new List<string>();
            Type current = type;
            while (current != null && current != typeof(MonoBehaviour))
            {
                foreach (System.Reflection.FieldInfo field in current.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly))
                {
                    if (field.IsPublic || field.IsDefined(typeof(SerializeField), false))
                    {
                        fields.Add($"{CleanName(field.Name)}: {field.FieldType.Name}");
                    }
                }

                current = current.BaseType;
            }

            return fields;
        }

        public sealed class Operation
        {
            public string Op { get; set; }
            public string State { get; set; }
            public string Type { get; set; }
            public string Label { get; set; }
            public string TrueState { get; set; }
            public string FalseState { get; set; }
            public string Property { get; set; }
            public string Value { get; set; }
        }
    }
}
