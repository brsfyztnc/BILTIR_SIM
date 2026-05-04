using UnityEngine;

public class TrackerBridge : MonoBehaviour
{
    [SerializeField]
    public  Transform rightControllerTransform;
    
    public Transform leftControllerTransform ;
    
    public GameObject tempLegObject;


    // Update is called once per frame
    void Update()
    {
        tempLegObject.transform.position = rightControllerTransform.position;
        tempLegObject.transform.rotation = rightControllerTransform.rotation;
    }
}
