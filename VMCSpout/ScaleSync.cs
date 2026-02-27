using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VMCSpout
{
    public class ScaleSync : MonoBehaviour
    {
        public Transform TargetTransform;
        public bool IsSync = true;
        private void Update()
        {
            if (TargetTransform)
            {
                if (IsSync)
                {
                    transform.position = TargetTransform.position;
                    transform.rotation = TargetTransform.rotation;
                    transform.localScale = TargetTransform.localScale;
                }
                else
                {
                    transform.position =Vector3.zero;
                    transform.rotation = Quaternion.identity;
                    transform.localScale = Vector3.one;
                }
            }
        }
    }
}
