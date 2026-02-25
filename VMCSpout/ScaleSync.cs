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

        private void Update()
        {
            if (TargetTransform)
            {
                transform.position = TargetTransform.position;
                transform.rotation = TargetTransform.rotation;
                transform.localScale = TargetTransform.localScale;
            }
        }
    }
}
