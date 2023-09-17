
# Mini World Gen

Mini world gen is a tool made for creation of small scale procedural worlds in unity. It allows user to create custom biomes and define under what conditions they can appear. Mini world gen provides a very flexible way of modifying how world generation behaves based on many variables such as global temperature, global/local height, biome size and much more.

# What can you make with it

Mini world gen is very flexible and it allows designer to modify almost any aspect of world generation you could think of. As designer you can do the following:

* create custom biomes
    * Assign material
    * Give name
    * List other biomes that can appear next to it
    * List objects that can spawn in the biome
    * Define density of object instances in the biome
    * Define local height
    * Define biome temperature
* Modify global temperature
* Modify temperature fluctuations
* Modify map size
* Modify size of biomes
* Define sea level

# How to use it

To put world generator into your scene create an empty game object and attatch "Tile map generator" script to it.
First thing you'll be able to do is set the map size. it is recommended that the map size isn't more than 50x50.

 Next thing you'll see is seed. you can choose to generate a random one by pressing "Generate random seed" button or type in the seed yourself.

 In temperature settings you'll see two sliders. Global temperature will let you define how cold or hot the world is. this would effect what biomes can spawn. Another slider let's you modify temperature fluctuations, High temperature fluctuations will make temperature change drastically between two points on the map. low fluctuations will make temperature change more gradual.

 In biome settings you MUST assign biomes to three arrays that are listed. those arrays are: Cold, Temperate and Hot. here you will have to place what biomes can spawn in those temperature ranges. There needs to be at least one biome in each of the array. At the bottom of biome settings there's a slider "Biome Size". As the name suggests this slider defines how large biomes can be.

 In other settings there's couple variables you can modify. First one if global height, this will determine how mountainous the whole terrain is. Second one is Sea level, this will determine at what height the sea plane will generate. lastly there's a slot for water material which you MUST assign.

 To create a biome you need to in project window right click and go create->biome. This will create new scriptable object which has couple more variables that need to be assigned. First one is name which is not mandatory. Second is land colour, this is a field for a material and it MUST be assigned. Further down there's a list of compatible biomes. This list needs to hold all the biomes within the same temperature range (Hot, Cold, Temperate) that can connect to the new biome. Another list called "Flora" holds all the objects than can spawn in the biome for example Tree, mushroom, bush. Another variable is "Flora Density", this will define how likely is it for something to spawn in the biome tile. Lastly there's height amplification which let's you modify global heights.  
