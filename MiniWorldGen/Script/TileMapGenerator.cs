/*
 * 
 * Simple World Generator
 * © 2023 Michał Redźko https://github.com/RedCore0
 * This work is licensed under the Creative Commons Attribution 4.0 International License. 
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or send a letter to Creative Commons, PO Box 1866, Mountain View, CA 94042, USA.
 * 
 * Version 1.0.0
 * 
 * This code manages the world generation process.
 * It allows designer to input all the attributes by hich map will be generated.
 * based on designer's input this script will run through all it's functions to generate
 * terrain step by step.
 * 
*/

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections;

namespace map {
    public class TileMapGenerator : MonoBehaviour
    {
        //Map size variables
        [SerializeField] private int SizeZ;
        [SerializeField] private int SizeX;
        //World Seed variables 
        [SerializeField] private float SeedZTemp;
        [SerializeField] private float SeedXTemp;
        [SerializeField] private float SeedZHeight;
        [SerializeField] private float SeedXHeight;
        //Perlin noise modifiers
        [SerializeField] private float TempFluctuations;
        [SerializeField] private float GlobalHeight;
        //Editor layout variables
        private bool ShowBiomes = false;
        private bool ShowBiomesSetiings = false;
        private bool TemperatureSettings = false;
        private bool OtherSettings = false;
        //Map Biomes Variables
        [SerializeField] Material[] Biomes;
        [SerializeField]private float BiomeSize;
        public Biome[] Hot;
        public Biome[] Cold;
        public Biome[] Temperate;
        //Water variables
        [SerializeField] private float WaterLevel;
        [SerializeField] private Material WaterMat;
        //Temperature settings
        [SerializeField] private float GlobalTemperatureEditor;
        [SerializeField] private float GlobalTemperature;
        //Other variables
        public List<Transform> Tiles;

        //Called when code is initialised
        private void Start(){
            GenerateMap(); //Starts world generations
        }

        //GenerateMap function goes through all stages of generating map
        private void GenerateMap() {
            //removes previous map if there is one
            foreach (Transform t in transform) {
                Destroy(t.gameObject);
            }            
            Destroy(GameObject.Find("Sea"));
            Tiles.Clear();
            //runs through all function to generate new world
            CreateTiles(SizeX, SizeZ);
            foreach (Transform t in transform){
                t.GetComponent<TileData>().CreateTile();
            }
            SetTemperature();
            CreateBiomes(0);
            SetHeight();
            WaterGeneration();
            StartCoroutine(Wait());
        }

        /*
         * Program uses raycast to place flora on top of the tile.
         * Raycast is being updated every frame, therethore generating height
         * and flora needed to be done at different frames. To achieve this
         * script has a cooldown in form of coroutine to make sure raycast hit data
         * is up to date.
        */
        private IEnumerator Wait() {
            yield return new WaitForSeconds(.5f);
            FloraGeneration();
        }

        //Generates random Seed
        private void GenerateSeed() {
            SeedXTemp = Random.Range(0f, 9999f);
            SeedZTemp = Random.Range(0f, 9999f);
            SeedXHeight = Random.Range(0f, 9999f);
            SeedZHeight = Random.Range(0f, 9999f);
        }

        //Creates a blank tile map based on given size
        private void CreateTiles(int SizeX, int SizeZ){
            GameObject TilePrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            TilePrefab.AddComponent<TileData>();
            for (int i = 0; i < SizeX; i++){
                for (int j = 0; j < SizeZ; j++){
                    Vector3 TilePos = new(i, 0, j);
                    var inst = Instantiate(TilePrefab, TilePos, Quaternion.identity, transform);
                    Tiles.Add(inst.transform);

                }
            }
            Destroy(TilePrefab);
        }

        //Set temperature on each tile
        /*
         * Each tile is assigned with temperature. This will later be used to determine
         * what biome will occupy specific tile
        */
        private void SetTemperature() {
            int Loops = 0;
            for (int i = 0; i < SizeX; i++){
                for (int j = 0; j < SizeZ; j++){
                    float Cordx = (float)i/SizeX*TempFluctuations+SeedXTemp;
                    float Cordz = (float)j/SizeZ*TempFluctuations+SeedZTemp;
                    float Temp = Mathf.PerlinNoise(Cordx, Cordz);
                    Transform Tile = Tiles[Loops];
                    TileData Tdata = Tile.GetComponent<TileData>();
                    Tdata.Temperature = Temp;
                    //Narrow down list of possible biomes based on tile's temperature
                    if (Temp > GlobalTemperature + .66f){
                        Tdata.biomes = Tdata.biomes.Intersect(Hot).ToList();
                        Tdata.TempType = "Hot";
                    }
                    else if (Temp > GlobalTemperature + .33){
                        Tdata.biomes = Tdata.biomes.Intersect(Temperate).ToList();
                        Tdata.TempType = "Temperate";
                    }
                    else {
                        Tdata.biomes = Tdata.biomes.Intersect(Cold).ToList();
                        Tdata.TempType = "Cold";
                    }
                    Loops++;
                }
            }
        }

        //Creates list of all adjacent tiles
        private List<TileData> FindAdjacentTiles(int TileIndex){
            List<TileData> AdjacentTiles = new List<TileData>();
            if (TileIndex + 1 < Tiles.Count && TileIndex + 1 % SizeZ >= 0){
                AdjacentTiles.Add(Tiles[TileIndex + 1].GetComponent<TileData>());
            }
            if (TileIndex - 1 % SizeZ >= 0){
                AdjacentTiles.Add(Tiles[TileIndex - 1].GetComponent<TileData>());
            }
            if (TileIndex - SizeX >= 0){
                AdjacentTiles.Add(Tiles[TileIndex - SizeX].GetComponent<TileData>());
            }
            if (TileIndex + SizeX < Tiles.Count){
                AdjacentTiles.Add(Tiles[TileIndex + SizeX].GetComponent<TileData>());
            }
            return AdjacentTiles;
        }

        //Place biomes in approprete positions using wave function collapse
        private void CreateBiomes(int TileIndex) {
            //Get all adjacent tiles with same temperature category
            List<TileData> AdjacentTiles = new List<TileData>();
            TileData ThisTile = Tiles[TileIndex].GetComponent<TileData>();
            foreach (TileData Tile in FindAdjacentTiles(TileIndex)){
                if(ThisTile.TempType == Tile.TempType){
                    AdjacentTiles.Add(Tile);
                }
            }

            /*
             * Create biomes using wave function collapse
             * Find tile with the smallest amount of possible biomes and choose one
             * if there's a biome on adjacent tile and it has same temperature category
             * script can choose to continue expanding adjacent biome into next tile
            */

            //Choose if to expand existing biome or create new one
            float RandomChoice = Random.Range(0f, 100f);
            if(RandomChoice < BiomeSize){
                //Expand existing biome
                //Check if any adjacent tile has biome asigned
                List<TileData> AdjacentWithBiome = new List<TileData>();
                foreach (TileData t in AdjacentTiles){
                    if (t.biome) { AdjacentWithBiome.Add(t); }
                }
                if (AdjacentWithBiome.Count>0){
                    int RandomAdjacent = Random.Range(0, AdjacentWithBiome.Count);
                    ThisTile.biome = AdjacentWithBiome[RandomAdjacent].biome;
                }
                //If there isn't create new biome
                else{
                    ThisTile.biome = ThisTile.biomes[Random.Range(0, ThisTile.biomes.Count)];
                }
            }
            else{
                //create new biome
                ThisTile.biome = ThisTile.biomes[Random.Range(0, ThisTile.biomes.Count)];
            }

            //colour in the tile
            ThisTile.GetComponent<MeshRenderer>().material = ThisTile.biome.LandColor;

            //Update list of compatible tiles for adjacent tiles
            foreach (TileData Tile in AdjacentTiles){
                Tile.biomes = Tile.biomes.Intersect(ThisTile.biome.CompatibleBiomes).ToList();
            }

            //Find tile with the smallest list of compatible tiles and continue recursion
            int SmallestPossibility = 999;
            int NextTileIndex = 0;
            int ExistingBiomes = 0;
            for (int i = 0; i < Tiles.Count; i++){
                if (!Tiles[i].GetComponent<TileData>().biome){
                    int possibility = Tiles[i].GetComponent<TileData>().biomes.Count;
                    if (possibility < SmallestPossibility){
                        SmallestPossibility = possibility;
                        NextTileIndex = i;
                    }
                }
                else { ExistingBiomes++; }   
            }
            if(ExistingBiomes < Tiles.Count){
                CreateBiomes(NextTileIndex);
            }
        }

        //set height for each tile
        private void SetHeight()
        {
            int loops = 0;
            for (int i = 0; i < SizeX; i++){
                for (int j = 0; j < SizeZ; j++){
                    float Cordx = (float)i / SizeX * GlobalHeight + SeedXHeight;
                    float Cordz = (float)j / SizeZ * GlobalHeight + SeedZHeight;
                    float height = Mathf.PerlinNoise(Cordx, Cordz);
                    Transform Tile = Tiles[loops];
                    float Amplify = Tiles[loops].GetComponent<TileData>().biome.HeightAmplification;
                    if(height*Amplify > 0){
                        Tile.localScale = new Vector3(Tile.localScale.x, height * Amplify, Tile.localScale.z);
                    }
                    else { Tile.localScale = new Vector3(Tile.localScale.x, .1f, Tile.localScale.z); }
                    loops++;
                }
            }
        }

        //generate bodies of water
        private void WaterGeneration(){
            //Create water plane on global water level
            //creates plane that covers entire map
            //use to form large bodies of water
            GameObject Water = GameObject.CreatePrimitive(PrimitiveType.Plane);
            Water.transform.position = new Vector3((SizeX / 2) - .5f, WaterLevel, (SizeZ / 2) - .5f);
            Water.transform.localScale = new Vector3(SizeX / 10, 1, SizeZ / 10);
            Water.GetComponent<MeshRenderer>().material = WaterMat;
            Water.transform.name = "Sea";
            //Create Rivers
            //Generate Springs
            int RiversAmount = 25;
            List<int> SpringIndex = new List<int>();
            for (int i = 0; i < RiversAmount; i++){
                int RandomPos = Random.Range(0, Tiles.Count);
                SpringIndex.Add(RandomPos);
                Transform spring = Tiles[RandomPos];
                spring.GetComponent<MeshRenderer>().material = WaterMat;
                spring.GetComponent<TileData>().ForbidFauna = true;
            }
            //Goes through each spring and creates river flow
            foreach(int i in SpringIndex){
                RiverFlow(i);
            }
            
        }

        //Generate River Path
        private void RiverFlow(int i){
            //Find Tiles with lower height
            List<Transform> RiverTiles = new List<Transform>();
            foreach (TileData t in FindAdjacentTiles(i)){
                if (t.transform.localScale.y < Tiles[i].localScale.y)
                {
                    RiverTiles.Add(t.transform);
                }
            }
            //If there are tiles with lower height continue river flow into one of them
            if (RiverTiles.Count > 0){
                int NextRiverTileIndex = Random.Range(0, RiverTiles.Count);
                Transform NextRiverTile = RiverTiles[NextRiverTileIndex];
                //Elevate terrain around river to avoid breaking physics
                for (int j = 0; j < RiverTiles.Count; j++){
                    if (j != NextRiverTileIndex){
                        int k = Tiles.IndexOf(RiverTiles[j]);
                        Tiles[k].transform.localScale = new Vector3(Tiles[k].transform.localScale.x,
                            Tiles[i].localScale.y, Tiles[k].transform.localScale.z);
                    }
                }
                //Colour in the river, forbid fauna generation on river tile,
                //and continue building river flow
                NextRiverTile.GetComponent<MeshRenderer>().material = WaterMat;
                NextRiverTile.GetComponent<TileData>().ForbidFauna = true;
                RiverFlow(Tiles.IndexOf(NextRiverTile));
            }
        }

        //Generate Flora
        private void FloraGeneration(){
            foreach(Transform t in Tiles){
                t.GetComponent<TileData>().GenerateFlora();
            }
        }

        //Custom Inspector Layout
        [CustomEditor(typeof(TileMapGenerator))]
        public class MapData : Editor{
            public override void OnInspectorGUI(){
                //Map Size
                //Display and allow to modify X and Z size of the map;
                var MapGen = (TileMapGenerator)target;
                GUILayout.Label("Map Size", EditorStyles.whiteLargeLabel);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Size X: ");
                MapGen.SizeX = EditorGUILayout.IntField(MapGen.SizeX);
                GUILayout.Label("Size Z: ");
                MapGen.SizeZ = EditorGUILayout.IntField(MapGen.SizeZ);
                GUILayout.EndHorizontal();
                if (MapGen.SizeX > 50 || MapGen.SizeZ > 50) {
                    EditorGUILayout.HelpBox("Large maps could cause lag and crashes",
                        MessageType.Warning);
                }

                //Seed
                //Seed defines how map is generated
                GUILayout.Space(10);
                GUILayout.Label("Seed", EditorStyles.whiteLargeLabel);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Temp X: ");
                MapGen.SeedXTemp = EditorGUILayout.FloatField(MapGen.SeedXTemp);
                GUILayout.Label("Temp Z: ");
                MapGen.SeedZTemp = EditorGUILayout.FloatField(MapGen.SeedZTemp);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Height X: ");
                MapGen.SeedXHeight = EditorGUILayout.FloatField(MapGen.SeedXHeight);
                GUILayout.Label("Height Z: ");
                MapGen.SeedZHeight = EditorGUILayout.FloatField(MapGen.SeedZHeight);
                GUILayout.EndHorizontal();
                if (GUILayout.Button("Generate Random Seed")) { 
                    MapGen.GenerateSeed();
                }

                //Temperature
                //Temperature defines what type of biomes can spawn on each tile
                GUILayout.Space(10);
                MapGen.TemperatureSettings = 
                    EditorGUILayout.Foldout(MapGen.TemperatureSettings, "Temperature Settings", true);
                if (MapGen.TemperatureSettings){
                    EditorGUI.indentLevel++;
                    //Temperature
                    //Set global temperature
                    GUILayout.Space(10);
                    GUILayout.Label("Global Temperature", EditorStyles.label);
                    MapGen.GlobalTemperatureEditor =
                        EditorGUILayout.Slider(MapGen.GlobalTemperatureEditor, -.5f, .5f);
                    MapGen.GlobalTemperature = MapGen.GlobalTemperatureEditor * -1;

                    //Changes in temperature
                    //Define how drastically can temperature change between two locations 
                    GUILayout.Space(10);
                    GUILayout.Label("Temperature Fluctuations", EditorStyles.label);
                    MapGen.TempFluctuations = EditorGUILayout.Slider(MapGen.TempFluctuations, 0f, 10f);
                    EditorGUI.indentLevel--;
                }


                //Biome Settings
                GUILayout.Space(10);
                MapGen.ShowBiomesSetiings =
                    EditorGUILayout.Foldout(MapGen.ShowBiomesSetiings, "Biome Settings", true);
                if (MapGen.ShowBiomesSetiings){
                    EditorGUI.indentLevel++;
                    GUILayout.Space(5);
                    //Hold data about each biome that can spawn on the map;
                    MapGen.ShowBiomes = EditorGUILayout.Foldout(MapGen.ShowBiomes, "Biomes", true);
                    if (MapGen.ShowBiomes){
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Hot"), true);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Temperate"), true);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Cold"), true);
                        serializedObject.ApplyModifiedProperties();
                        EditorGUI.indentLevel--;
                    }
                    //Define how big biomes should be
                    GUILayout.Space(5);
                    GUILayout.Label("Biome Size", EditorStyles.label);
                    MapGen.BiomeSize = EditorGUILayout.Slider(MapGen.BiomeSize, 0f, 100f);
                }

                //Other Settings
                GUILayout.Space(10);
                MapGen.OtherSettings = EditorGUILayout.Foldout(MapGen.OtherSettings, "Other Settings", true);
                if (MapGen.OtherSettings){
                    EditorGUI.indentLevel++;
                    GUILayout.Space(5);
                    GUILayout.Label("Global Height", EditorStyles.label);
                    MapGen.GlobalHeight = EditorGUILayout.Slider(MapGen.GlobalHeight, 0f, 10);
                    GUILayout.Space(5);
                    GUILayout.Label("Sea Level", EditorStyles.label);
                    MapGen.WaterLevel = EditorGUILayout.Slider(MapGen.WaterLevel, 0f, 10f);
                    GUILayout.Space(5);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Water Material", EditorStyles.label);
                    MapGen.WaterMat = (Material)EditorGUILayout.ObjectField(MapGen.WaterMat, typeof(Material), false);
                    GUILayout.EndHorizontal();
                }

                //Debug Generate New Map
                GUILayout.Space(10);
                if (GUILayout.Button("Generate New Map")){
                    MapGen.GenerateMap();
                }

                //Saves Changes
                if (GUI.changed){
                    EditorUtility.SetDirty(target);
                }
            }
        }
    }
}