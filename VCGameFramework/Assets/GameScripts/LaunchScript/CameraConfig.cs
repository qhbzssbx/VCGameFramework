using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class CameraConfig : MonoBehaviour
{

    [Inject]
    Test test;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Button("Test")]
    public void PrintTest()
    {
        Debug.Log("Test: " + test.test);
    }
}
