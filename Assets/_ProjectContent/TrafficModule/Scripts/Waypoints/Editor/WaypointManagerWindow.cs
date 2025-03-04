using UnityEngine;
using UnityEditor;

namespace TrafficModule.Waypoints.Editor
{
// This class is used to create and delete Waypoints (EditorWindow)
    public class WaypointManagerWindow : EditorWindow
    {
        private const string WAYPOINT_NAME = "Waypoint";
        private const string WAYPOINT_TAG = "Waypoint";
        
        [MenuItem("Tools/Waypoint Editor")]
        public static void Open()
        {
            GetWindow<WaypointManagerWindow>();
        }

        public Transform waypointRoot;

        // drawing GUI
        private void OnGUI()
        {
            var obj = new SerializedObject(this);

            EditorGUILayout.PropertyField(obj.FindProperty("waypointRoot"));
            // show text that you need to put waypointRoot
            if (waypointRoot == null)
            {
                EditorGUILayout.HelpBox(
                    "Root transform must be selected. Please assign a root transform", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.BeginVertical("box");
                DrawButtons();
                EditorGUILayout.EndVertical();
            }

            obj.ApplyModifiedProperties();
        }

        private void DrawButtons()
        {
            // draw main button
            if (GUILayout.Button("Create Waypoint"))
            {
                CreateWaypoint();
            }

            // if selected object is waypoint draw some more buttons
            if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Waypoint>())
            {
                if (GUILayout.Button("Create branch"))
                {
                    CreateBranch();
                }

                if (GUILayout.Button("Create waypoint before"))
                {
                    CreateWaypointBefore();
                }

                if (GUILayout.Button("Create waypoint after"))
                {
                    CreateWaypointAfter();
                }

                if (GUILayout.Button("Delete waypoint"))
                {
                    DeleteWaypoint();
                }
            }
        }

        private void CreateWaypointBefore()
        {
            // create object
            var waypointObj = SpawnPoint();

            var newWaypoint = waypointObj.GetComponent<Waypoint>();
            var selectedWaypoint = Selection.activeGameObject.GetComponent<Waypoint>();

            waypointObj.transform.position = selectedWaypoint.transform.position;
            waypointObj.transform.forward = selectedWaypoint.transform.forward;

            // set new links
            if (selectedWaypoint.previous != null)
            {
                newWaypoint.previous = selectedWaypoint.previous;
                selectedWaypoint.previous.next = newWaypoint;
            }

            newWaypoint.next = selectedWaypoint;
            selectedWaypoint.previous = newWaypoint;

            newWaypoint.transform.SetSiblingIndex(selectedWaypoint.transform.GetSiblingIndex());
            Selection.activeGameObject = newWaypoint.gameObject;
            // add to main WaypointMager
            WaypointManager.Instance.allWaypoints.Add(newWaypoint);
        }

        private void CreateWaypointAfter()
        {
            // create object
            var waypointObj = SpawnPoint();
            var newWaypoint = waypointObj.GetComponent<Waypoint>();
            var selectedWaypoint = Selection.activeGameObject.GetComponent<Waypoint>();
            waypointObj.transform.position = selectedWaypoint.transform.position;
            waypointObj.transform.forward = selectedWaypoint.transform.forward;

            // set new links
            newWaypoint.previous = selectedWaypoint;

            if (selectedWaypoint.next != null)
            {
                selectedWaypoint.next.previous = newWaypoint;
                newWaypoint.next = selectedWaypoint.next;
            }

            selectedWaypoint.next = newWaypoint;

            newWaypoint.transform.SetSiblingIndex(selectedWaypoint.transform.GetSiblingIndex());
            Selection.activeGameObject = newWaypoint.gameObject;
            WaypointManager.Instance.allWaypoints.Add(newWaypoint);
        }

        private static void DeleteWaypoint()
        {
            var selectedWaypoint = Selection.activeGameObject.GetComponent<Waypoint>();
            // set links algorithm
            if (selectedWaypoint.next != null)
            {
                selectedWaypoint.next.previous = selectedWaypoint.previous;
                selectedWaypoint.next.RemoveFromAllPrevious(selectedWaypoint);
            }

            if (selectedWaypoint.previous != null)
            {
                selectedWaypoint.previous.next = selectedWaypoint.next;
                Selection.activeGameObject = selectedWaypoint.previous.gameObject;
                selectedWaypoint.previous.RemoveFromAllFuture(selectedWaypoint);
            }

            if (selectedWaypoint.previousBranches.Count != 0)
            {
                foreach (var previous in selectedWaypoint.previousBranches)
                {
                    previous.branches.Remove(selectedWaypoint);
                    previous.RemoveFromAllFuture(selectedWaypoint);
                }
            }

            if (selectedWaypoint.branches.Count != 0)
            {
                foreach (var next in selectedWaypoint.branches)
                {
                    next.previousBranches.Remove(selectedWaypoint);
                    next.RemoveFromAllPrevious(selectedWaypoint);
                }
            }

            WaypointManager.Instance.allWaypoints.Remove(selectedWaypoint);
            DestroyImmediate(selectedWaypoint.gameObject);
        }

        private void CreateWaypoint()
        {
            // create object
            var waypointObj = SpawnPoint();
            var waypoint = waypointObj.GetComponent<Waypoint>();

            Selection.activeGameObject = waypoint.gameObject;
            WaypointManager.Instance.allWaypoints.Add(waypoint);
        }

        private void CreateBranch()
        {
            // create object
            var waypointObj = new GameObject(WAYPOINT_NAME + waypointRoot.childCount, typeof(Waypoint));
            waypointObj.tag = WAYPOINT_TAG;
            waypointObj.transform.SetParent(waypointRoot, false);

            var waypoint = waypointObj.GetComponent<Waypoint>();
            var branchedFrom = Selection.activeGameObject.GetComponent<Waypoint>();
            
            // add it to WaypointLinks as branches
            branchedFrom.branches.Add(waypoint);
            waypoint.previousBranches.Add(branchedFrom);
            waypoint.transform.position = branchedFrom.transform.position;
            waypoint.transform.forward = branchedFrom.transform.forward;
            Selection.activeGameObject = waypoint.gameObject;
            WaypointManager.Instance.allWaypoints.Add(waypoint);
        }

        // spawn point on RootWaypoint
        private GameObject SpawnPoint()
        {
            var waypointObj = new GameObject(WAYPOINT_NAME + waypointRoot.childCount, typeof(Waypoint));
            waypointObj.tag = WAYPOINT_TAG;
            waypointObj.transform.SetParent(waypointRoot, false);
            return waypointObj;
        }
    }
}