using System.Collections.Generic;
using UnityEngine;

namespace TrafficModule.LicensePlate
{
   public class LicensePlateController : MonoBehaviour
   {
      public List<GameObject> licensePlatePoints = new List<GameObject>();
      public string number;
      public string region;
      
      public LicensePlateSpawner licensePlateSpawner;

      public void Init()
      {

      }
      
      public void SpawnAndDestroy(GameObject licenseObject)
      {
         GameObject newPlate = null;
         LicensePlate newLicensePlate = null;
         if (number == "" || region == "")
         {
            newLicensePlate = licensePlateSpawner.Spawn();
            newPlate = newLicensePlate.LicensePlateObject;
            number = newLicensePlate.Number;
            region = newLicensePlate.Region;
         }
         else
         {
            newLicensePlate =
               licensePlateSpawner.CreateCertainUsualPlate(number, region);
            number = newLicensePlate.Number;
            region = newLicensePlate.Region;
            newPlate = newLicensePlate.LicensePlateObject;
         }

         for (var i = 0; i < licensePlatePoints.Count; i++)
         {
            var plate = Instantiate(newPlate, new Vector3(0, 0, 0), Quaternion.Euler(0f, 0, 0f));
            plate.transform.SetParent(licensePlatePoints[i].transform, false);
            if (i == 0)
            {
               newLicensePlate.LicensePlateObject = plate;
            }
         }

         DestroyImmediate(newPlate);
      }
   }
}