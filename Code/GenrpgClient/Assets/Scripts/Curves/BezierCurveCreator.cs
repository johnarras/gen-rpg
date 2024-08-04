using UnityEngine;
using UnityEngine.Splines;

public class BezierCurveCreator : BaseBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector3 point1 = Vector3.zero;
        Vector3 tan1 = new Vector3(1, 0, 0);

        Vector3 point2 = new Vector3(10, 10, 0);
        Vector3 tan2 = new Vector3(0, 1, 0);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
