using Larje.Core.Extensions;
using Larje.Character;
using System.Collections.Generic;
using System.Linq;
using Larje.Character.AI;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Larje.Character.Editor
{
    [CustomEditor(typeof(AIBrain))]
    public class AIBrainEditor : UnityEditor.Editor
    {
        private Dictionary<AIState, bool> _stateFoldouts = new Dictionary<AIState, bool>();

        private float _boxColorAlpha = 0.1f;
        private Color[] _colors = {
            new Color(     .333f, .345f, .475f, 1f),
            new Color(.871f, .827f, .769f, 1f),
            new Color(     .596f, .631f, .737f, 1f),
            new Color(     .957f, .922f, .827f, 1f)
        };

        public override void OnInspectorGUI()
        {
            AIBrain aiBrain = (AIBrain)target;

            DrawDebug();
            DrawStates();
            DrawFrequencies();
            DrawResetOptions();

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            Repaint();
        }

        private void DrawDebug()
        {
            if (Application.isPlaying)
            {
                GUILayout.Label("Debug", EditorStyles.boldLabel);
                GUILayout.BeginVertical(EditorStyles.helpBox);

                AIBrain aiBrain = (AIBrain)target;

                string state = aiBrain.CurrentState != null ? aiBrain.CurrentState.StateName : "None";
                GUILayout.Label($"Current state: {state}", EditorStyles.label);
                GUILayout.Label($"Time in state: {aiBrain.TimeInThisState.ToString("F1")}", EditorStyles.label);

                if (GUILayout.Button($"Target: {aiBrain.Target?.name ?? "None"}", EditorStyles.linkLabel))
                {
                    if (aiBrain.Target != null)
                    {
                        Selection.activeGameObject = aiBrain.Target.gameObject;
                    }
                }

                if (GUILayout.Button($"Owner: {aiBrain.Owner?.name ?? "None"}", EditorStyles.linkLabel))
                {
                    if (aiBrain.Owner != null)
                    {
                        Selection.activeGameObject = aiBrain.Owner.gameObject;
                    }
                }

                GUILayout.EndVertical();
                GUILayout.Space(20f);
            }
        }

        private void DrawStates()
        {
            GUILayout.Label("States", EditorStyles.boldLabel);

            int i = 0;
            foreach (AIState state in ((AIBrain)target).states.ToArray())
            {
                DrawState(state, i);
                i++;
            }       

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUIStyle buttonStyle = EditorStyles.miniButton;
            buttonStyle.fixedWidth = 50f;
            if (GUILayout.Button("+"))
            {
                ((AIBrain)target).states.Add(new AIState());
                EditorUtility.SetDirty(target);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(20f);
        }

        private void DrawState(AIState state, int i)
        {
            if (!_stateFoldouts.ContainsKey(state))
            {
                _stateFoldouts.Add(state, false);
            }

            Rect rect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.DrawRect(rect, _colors[i % _colors.Length].SetAlpha(_boxColorAlpha));

            EditorGUILayout.BeginHorizontal();

            string stateName = string.IsNullOrEmpty(state.StateName) ? "---" : state.StateName;
            string icon = _stateFoldouts[state] ? "▼ " : "▶ ";
            if (GUILayout.Button($"{icon} {stateName}", EditorStyles.boldLabel))
            {
                _stateFoldouts[state] = !_stateFoldouts[state];
            }

            GUIStyle buttonStyle = EditorStyles.miniButton;
            buttonStyle.fixedWidth = 20f;
            if (GUILayout.Button($"X", buttonStyle))
            {
                ((AIBrain)target).states.Remove(state);
                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.EndHorizontal();

            if (_stateFoldouts[state])
            {
                Separator();
                EditorGUI.indentLevel++;

                string stateNamePrev = state.StateName;
                state.StateName = EditorGUILayout.TextField("State Name", state.StateName);
                if (state.StateName != stateNamePrev) EditorUtility.SetDirty(target);

                DrawActions(state);
                DrawTransitions(state);

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawActions(AIState state)
        {
            state.Actions ??= new List<AIAction>();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Actions");
            GUIStyle buttonStyle = EditorStyles.miniButton;
            buttonStyle.fixedWidth = 20f;
            if (GUILayout.Button($"+", buttonStyle))
            {
                state.Actions.Add(GetAllActions()[0]);
                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.EndHorizontal();

            Separator();

            for (int i = 0; i < state.Actions.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                AIAction currentAction = state.Actions[i];
                List<AIAction> allActions = GetAllActions();

                int selectedIndex = currentAction != null ? allActions.IndexOf(currentAction) : -1;
                int selectedIndexPrev = selectedIndex;
                string[] options = allActions.Select(a => a.Label).ToArray();
                selectedIndex = EditorGUILayout.Popup("", selectedIndex, options);
                if (selectedIndex >= 0 && selectedIndex < allActions.Count)
                {
                    state.Actions[i] = allActions[selectedIndex];
                }
                else
                {
                    state.Actions[i] = null;
                }

                if (selectedIndex != selectedIndexPrev)
                {
                    EditorUtility.SetDirty(target);
                }

                GUIStyle removeButtonStyle = EditorStyles.miniButton;
                removeButtonStyle.fixedWidth = 20f;
                if (GUILayout.Button($"X", removeButtonStyle))
                {
                    state.Actions.RemoveAt(i);
                    EditorUtility.SetDirty(target);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawTransitions(AIState state)
        {
            state.Transitions ??= new List<AITransition>();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Transitions");
            GUIStyle buttonStyle = EditorStyles.miniButton;
            buttonStyle.fixedWidth = 20f;
            if (GUILayout.Button($"+", buttonStyle))
            {
                state.Transitions.Add(new AITransition());
                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.EndHorizontal();

            Separator();

            foreach (AITransition transition in state.Transitions)
            {
                EditorGUILayout.BeginHorizontal();

                List<AIDecision> allDecisions = GetAllDecisions();
                int selectedIndex = transition.Decision != null ? allDecisions.IndexOf(transition.Decision) : -1;
                int selectedIndexPrev = selectedIndex;
                string[] options = allDecisions.Select(a => a.Label).ToArray();
                selectedIndex = EditorGUILayout.Popup("", selectedIndex, options);
                if (selectedIndex >= 0 && selectedIndex < allDecisions.Count)
                {
                    transition.Decision = allDecisions[selectedIndex];
                }
                else
                {
                    transition.Decision = null;
                }

                if (selectedIndex != selectedIndexPrev)
                {
                    EditorUtility.SetDirty(target);
                }

                GUIStyle removeButtonStyle = EditorStyles.miniButton;
                removeButtonStyle.fixedWidth = 20f;
                if (GUILayout.Button($"X", removeButtonStyle))
                {
                    state.Transitions.Remove(transition);
                    EditorUtility.SetDirty(target);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel++;
                transition.TrueState = DrawTransitionState("True State", transition.TrueState);
                transition.FalseState = DrawTransitionState("False State", transition.FalseState);
                EditorGUILayout.Space(10);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private string DrawTransitionState(string label, string value)
        {
            List<AIState> allStates = GetAllStates();

            List<string> options = new List<string>();
            options.Add("None");
            options.AddRange(allStates.Select(a => a.StateName));

            AIState selectedState = allStates.Find(x => x.StateName == value);

            int selectedIndex = selectedState != null ? allStates.IndexOf(selectedState) + 1 : 0;
            int selectedIndexPrev = selectedIndex;
            selectedIndex = EditorGUILayout.Popup(label, selectedIndex, options.ToArray());

            selectedState = selectedIndex > 0 ? allStates[selectedIndex - 1] : null;

            if (selectedIndex != selectedIndexPrev)
            {
                EditorUtility.SetDirty(target);
            }

            return selectedState != null ? selectedState.StateName : "";
        }

        private void DrawFrequencies()
        {
            GUILayout.Label("Frequencies", EditorStyles.boldLabel);

            SerializedProperty actionsFrequencyProperty = serializedObject.FindProperty("actionsFrequency");
            SerializedProperty decisionFrequencyProperty = serializedObject.FindProperty("decisionFrequency");

            EditorGUILayout.PropertyField(actionsFrequencyProperty, new GUIContent("Actions Frequency"));
            EditorGUILayout.PropertyField(decisionFrequencyProperty, new GUIContent("Decision Frequency"));


            GUILayout.Space(20f);
        }

        private void DrawResetOptions()
        {
            GUILayout.Label("Reset", EditorStyles.boldLabel);

            SerializedProperty brainActiveProperty = serializedObject.FindProperty("brainActive");
            SerializedProperty resetBrainOnStartProperty = serializedObject.FindProperty("resetBrainOnStart");
            SerializedProperty resetBrainOnEnableProperty = serializedObject.FindProperty("resetBrainOnEnable");

            EditorGUILayout.PropertyField(brainActiveProperty, new GUIContent("Brain Active"));
            EditorGUILayout.PropertyField(resetBrainOnStartProperty, new GUIContent("Reset Brain On Start"));
            EditorGUILayout.PropertyField(resetBrainOnEnableProperty, new GUIContent("Reset Brain On Enable"));

            GUILayout.Space(20f);
        }

        private List<AIAction> GetAllActions()
        {
            return ((AIBrain)target).GetComponentsInChildren<AIAction>().ToList();
        }

        private List<AIDecision> GetAllDecisions()
        {
            return ((AIBrain)target).GetComponentsInChildren<AIDecision>().ToList();
        }

        private List<AIState> GetAllStates()
        {
            return ((AIBrain)target).states;
        }

        public void Separator(float thickness = 2f, float paddingTop = 2f, float paddingBot = 5f)
        {
            Color color = Color.black.SetAlpha(0.2f);

            GUILayout.Space(paddingTop);
            Rect rect = EditorGUILayout.GetControlRect(false, thickness);
            EditorGUI.DrawRect(rect, color);
            GUILayout.Space(paddingBot);
        }
    }
}
