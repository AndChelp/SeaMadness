using UnityEngine;

namespace DefaultNamespace {
    public class DebugLog : MonoBehaviour {
        bool _doShow;
        string _myLog = "*begin log";
        Vector2 _scrollPosition = new Vector2(0, 0);

        void Update() {
            if (Input.GetKeyDown(KeyCode.Space)) {
                _doShow = !_doShow;
            }
        }

        void OnEnable() {
            Application.logMessageReceived += Log;
        }

        void OnDisable() {
            Application.logMessageReceived -= Log;
        }

        void OnGUI() {
            if (!_doShow) {
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

        void Log(string logString, string stackTrace, LogType type) {
            _myLog = _myLog + "\n" + logString;
        }
    }
}