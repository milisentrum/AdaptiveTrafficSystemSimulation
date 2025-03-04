using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideTextInGame : MonoBehaviour
{
    private TextMesh textMesh;

    // Start is called before the first frame update
    void Start()
    {
        textMesh = GetComponent<TextMesh>();

        if (textMesh != null)
        {
            textMesh.gameObject.SetActive(false); 
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDrawGizmos()
    {
        if (textMesh != null)
        {
            UnityEditor.Handles.Label(transform.position, textMesh.text);
        }
    }
}
