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
 * This script holds data about each biome
 * 
*/

using UnityEngine;

[CreateAssetMenu(fileName ="New Biome")]
public class Biome : ScriptableObject
{
    public string Name;
    public Material LandColor;
    public Biome[] CompatibleBiomes;
    public GameObject[] Flora;
    public float FloraDensity;
    public float HeightAmplification; 
}