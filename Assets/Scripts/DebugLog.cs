using UnityEngine;

namespace DefaultNamespace {
    public class DebugLog : MonoBehaviour {
        private string _myLog = "*begin log";
        private bool _doShow = false;
        private Vector2 _scrollPosition = new(0, 0);

        private void OnEnable() {
            Application.logMessageReceived += Log;
        }

        private void OnDisable() {
            Application.logMessageReceived -= Log;
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Space)){
                _doShow = !_doShow;
            }
        }

        private void Log(string logString, string stackTrace, LogType type) {
            _myLog = _myLog + "\n" + logString;
        }

        private void OnGUI() {
            if (!_doShow){
                return;
            }
            var width = Screen.width / 2.0f;
            var height = Screen.height / 3.0f;
            GUILayout.BeginArea(new Rect(Screen.width - width - 10, 10, width, height));
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(height),
                GUILayout.Width(width));
            GUILayout.TextArea(_myLog, GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}