/*
 * 
 * Simple World Generator
 * © 2023 Micha? Red?ko https://github.com/RedCore0
 * This work is licensed under the Creative Commons Attribution 4.0 International License. 
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or send a letter to Creative Commons, PO Box 1866, Mountain View, CA 94042, USA.
 * 
 * Version 1.0.0
 * 
 * This script holds data for each tile
 * it is also used to generate structures on a tile
 * 
*/

using System.Collections.Generic;
using UnityEngine;

namespace map
{
    public class TileData : MonoBehaviour
    {
        //Variables
        [HideInInspector] public float Temperature;
        [HideInInspector] public Biome biome;
        [HideInInspector] public string TempType;
        [HideInInspector] public List<Biome> biomes;
        [HideInInspector] public bool ForbidFauna = false;

        //When tile is instantiated this function is called
        //It's purpuse is to list all possible biomes.
        //This list will be used for wave function collapse
        public void CreateTile()
        {
            TileMapGenerator GeneratorScript = transform.parent.GetComponent<TileMapGenerator>();
            foreach (Biome biome in GeneratorScript.Hot)
            {
                biomes.Add(biome);
            }
            foreach (Biome biome in GeneratorScript.Temperate)
            {
                biomes.Add(biome);
            }
            foreach (Biome biome in GeneratorScript.Cold)
            {
                biomes.Add(biome);
            }
        }

        //Generate Flora function is used to populate world with plants
        public void GenerateFlora()
        {
            //Decide if tile should be populated
            float GrowFlora = Random.Range(0, 100);
            if(biome.Flora.Length>0 && GrowFlora <= biome.FloraDensity)
            {
                //Draw raycast into the surface of a tile and instantiate
                //plant on top of it
                var pos = transform.position;
                Vector3 RayPos = new Vector3(pos.x, 1000, pos.z);
                RaycastHit hit;
                if(Physics.Raycast(RayPos, transform.TransformDirection(Vector3.down), out hit,
                    Mathf.Infinity) && hit.transform.GetComponent<TileData>())
                {
                    if(!hit.transform.GetComponent<TileData>().ForbidFauna)
                    {
                        //Choose plant from list
                        int ChosenFlora = Random.Range(0, biome.Flora.Length);
                        var inst = Instantiate(biome.Flora[ChosenFlora], hit.point, Quaternion.identity);
                        inst.transform.parent = transform;
                    }                
                }
            }
        }
    }
}