using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityDevKit.Utils.Objects
{
    public interface IVisibilityValidator
    {
        bool Validate(Transform fromPoint, Transform validationObjectTransform);

        IEnumerator ValidateInInterval(
            Transform fromPoint,
            Transform validationObjectTransform,
            float interval,
            int checksCount, List<bool> isVisibleResults);
    }
}