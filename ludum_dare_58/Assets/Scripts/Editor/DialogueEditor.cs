using UnityEditor;

namespace LudumDare58.DialogueSystem
{
    [CustomEditor(typeof(Dialogue))]
    public class DialogueEditor : Editor
    {
        Dialogue dialogue;

        SerializedProperty text;
        SerializedProperty continueAction;
        SerializedProperty characterDuration;
        SerializedProperty lineDelay;
        SerializedProperty freezeTime;

        private void OnEnable()
        {
            dialogue = target as Dialogue;

            text = serializedObject.FindProperty(nameof(dialogue.TextDisplay));
            continueAction = serializedObject.FindProperty(nameof(dialogue.ContinueAction));
            characterDuration = serializedObject.FindProperty(nameof(dialogue.CharacterDuration));
            lineDelay = serializedObject.FindProperty(nameof(dialogue.LineDelay));
            freezeTime = serializedObject.FindProperty(nameof(dialogue.FreezeTime));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(text);
            EditorGUILayout.Space(8);
            EditorGUILayout.TextField("Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(characterDuration);
            EditorGUILayout.PropertyField(lineDelay);
            EditorGUILayout.PropertyField(freezeTime);
            EditorGUILayout.Space(8);
            EditorGUILayout.TextField("Input", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(continueAction);

            serializedObject.ApplyModifiedProperties();
        }
    }
}