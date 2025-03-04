using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;

namespace TrafficModule.LicensePlate
{
    public class LicensePlateSpawner : MonoBehaviour
    {
        public TextMeshPro twoLetter;
        public TextMeshPro oneLetter;
        public TextMeshPro region;
        public TextMeshPro number;
        public GameObject licensePlate;
        private const string LETTERS = "АВЕКМНОРСТУХ";
        private const string NUMBERS = "0123456789";
        private readonly List<int> _regions = new List<int> {77, 97, 99, 177, 777, 78, 98, 178};

        private static List<LicensePlate> usedLicensePlates = new List<LicensePlate>();

        public enum PlateType
        {
            USUAL,
            TRANSPORT,
            MILITARY,
            EU
        };

        public PlateType plateType;

        public LicensePlate Spawn()
        {
            LicensePlate newPlate = null;
            do
            {
                switch (plateType)
                {
                    case PlateType.USUAL:
                        newPlate = CreateUsualPlate();
                        break;
                    case PlateType.TRANSPORT:
                        number.text = "USA";
                        break;
                    case PlateType.MILITARY:
                        number.text = "Canada";
                        break;
                    case PlateType.EU:
                        number.text = "Eu";
                        break;
                }
            } while (usedLicensePlates.Contains(newPlate));

            usedLicensePlates.Add(newPlate);
            return newPlate;
        }

        private LicensePlate CreateUsualPlate()
        {
            number.text = "";
            region.text = "";
            oneLetter.text = "";
            twoLetter.text = "";
            while (twoLetter.text.Length != 2)
            {
                if (oneLetter.text.Length == 0) oneLetter.text += LETTERS[Random.Range(0, LETTERS.Length)];
                if (number.text.Length == 3)
                {
                    twoLetter.text += LETTERS[Random.Range(0, LETTERS.Length)];
                }
                else
                {
                    number.text += NUMBERS[Random.Range(0, NUMBERS.Length)];
                }
            }

            region.text += _regions[Random.Range(0, _regions.Count)];
            var newPlate = Instantiate(licensePlate, new Vector3(0, 0, 0), Quaternion.Euler(0f, 0, 0f));
            newPlate.SetActive(true);

            var licensePlateObj = new LicensePlate()
            {
                LicensePlateObject = newPlate,
                Number = oneLetter.text + number.text + twoLetter.text,
                Region = region.text
            };
            return licensePlateObj;
        }

        public LicensePlate CreateCertainUsualPlate(string certainNumber, string certainRegion)
        {
            LicensePlate licensePlateObj = null;
            if (certainNumber.Length == 6 && _regions.Contains(Convert.ToInt32(certainRegion))) // if plate format is ok
            {
                twoLetter.text = certainNumber.Substring(certainNumber.Length - 2, 2).ToUpper();
                oneLetter.text = certainNumber.Substring(0, 1).ToUpper();
                region.text = certainRegion;
                number.text = certainNumber.Substring(1, 3);
                var newPlate = Instantiate(licensePlate, new Vector3(0, 0, 0), Quaternion.Euler(0f, 0, 0f));
                newPlate.SetActive(true);
                licensePlateObj = new LicensePlate()
                {
                    LicensePlateObject = newPlate,
                    Number = certainNumber,
                    Region = certainRegion
                };
                usedLicensePlates.Add(licensePlateObj);
            }
            else // else just create usual plate
            {
                licensePlateObj = Spawn();
            }

            return licensePlateObj;
        }
    }
}