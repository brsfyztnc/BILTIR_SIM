using System.Collections;
using UnityEngine;

namespace LegTracking.HandTracking
{
    public class HandTest : MonoBehaviour
    {
        [SerializeField] OVRSkeleton skeleton;

        private void Start()
        {
            Debug.Log("STARTED HAND TEST");
            foreach (var bone in skeleton.Bones)
            {
                Debug.Log("Printing");
                Debug.Log(bone.Id + " " + bone.Transform.name);
            }

            StartCoroutine(BonePrintRoutine());
        }

        private IEnumerator BonePrintRoutine()
        {
            while (true)
            {
                foreach (var bone in skeleton.Bones)
                {
                    Debug.Log(bone.Id + " " + bone.Transform.name);
                }

                yield return new WaitForSeconds(3);
            }
        }
        
    }
}