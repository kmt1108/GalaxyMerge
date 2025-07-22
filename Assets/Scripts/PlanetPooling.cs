using System;
using System.Collections.Generic;
using UnityEngine;

public class PlanetPooling: MonoBehaviour
{
    const string PLANET_PATH = "Planet/";
    private List<Planet>[] planetPool = new List<Planet>[9];
    private List<Planet>[] planetActive = new List<Planet>[9];
    private GameObject[] planetPrefabs = new GameObject[9];
    private void Awake()
    {
        Planet.OnHidden += OnPlanetHidden;
    }

    private void OnPlanetHidden(Planet planet)
    {
        StorePlanet(planet);
    }

    public Planet GetPlanet(int index, Vector3 initPos, Vector3 targetPos, bool enablePhysic = false)
    {
        if (planetPool[index] == null)
        {
            planetPool[index] = new List<Planet>();
            planetActive[index] = new List<Planet>();
        }
        //If no planets are available in the pool, instantiate a new one else return one from the pool and move to active List
        if (planetPool[index].Count == 0)
        {
            if (!planetPrefabs[index])
            {
                planetPrefabs[index] = Resources.Load<GameObject>(PLANET_PATH + index);
            }
            Planet newPlanet = Instantiate(planetPrefabs[index],initPos,Quaternion.identity,transform).GetComponent<Planet>();
            planetActive[index].Add(newPlanet);
            newPlanet.Setup(targetPos, enablePhysic);
            return newPlanet;
        }
        else
        {
            Planet planet = planetPool[index][0];
            planetPool[index].RemoveAt(0);
            planetActive[index].Add(planet);
            planet.transform.position = initPos;
            planet.Setup(targetPos, enablePhysic);
            planet.gameObject.SetActive(true);
            return planet;
        }
    }
    public void StorePlanet(Planet planet)
    {
        planet.Reset();
        int index = planet.planetIndex;
        planetActive[index].Remove(planet);
        planetPool[index].Add(planet);
    }
    public void StoreAllPlanets()
    {
        for (int i = 0; i < planetActive.Length; i++)
        {
            if (planetActive[i] != null)
            {
                for (int j= planetActive[i].Count-1;j>=0;j--)
                {
                    StorePlanet(planetActive[i][j]);
                }
            }
        }
    }
}
