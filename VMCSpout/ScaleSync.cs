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

        private Vector3 _targetPosition = Vector3.zero;
        private void Update()
        {
            if (TargetTransform)
            {
                _targetPosition = TargetTransform.position;
                if (IsSync)
                {
                    transform.position = TargetTransform.position;
                    transform.rotation = TargetTransform.rotation;
                    transform.localScale = TargetTransform.localScale;
                }
                else
                {
                    _targetPosition.x = TargetTransform.position.x / TargetTransform.localScale.x;
                    _targetPosition.y = TargetTransform.position.y / TargetTransform.localScale.y;
                    _targetPosition.z = TargetTransform.position.z / TargetTransform.localScale.z;

                    transform.position = _targetPosition;
                    transform.rotation = TargetTransform.rotation;
                    transform.localScale = Vector3.one;
                }
            }
        }
    }
}
